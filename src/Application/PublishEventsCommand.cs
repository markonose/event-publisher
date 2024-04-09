using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using RabbitMQ.Client;

namespace Application;

public interface ICommand;

public record PublishEventsCommand: ICommand
{
    public required FileInfo File { get; init; }
    public required string[] Hostnames { get; init; }
    public required string ExchangeName { get; init; }
}

public interface ICommandHandler<in TRequest> where TRequest : ICommand
{
    void Handle(TRequest request);
}

public class PublishEventsCommandHandler : ICommandHandler<PublishEventsCommand>
{
    private static readonly Dictionary<string, Type> XmlNodePlayerDataTypeMap = new()
    {
        {"player_registration", typeof(PlayerRegistration)}
    };
    
    private readonly IConnectionFactory _connectionFactory;
    private readonly IEventsMapper _eventsMapper;

    public PublishEventsCommandHandler(IConnectionFactory connectionFactory, IEventsMapper eventsMapper)
    {
        _connectionFactory = connectionFactory;
        _eventsMapper = eventsMapper;
    }

    public void Handle(PublishEventsCommand request)
    {
        var xmlDocument = LoadXmlDocument(request.File);
        var playersData = ParseXmlDocument(xmlDocument);
        PublishEvents(request, playersData);
    }

    private static XmlDocument LoadXmlDocument(FileInfo file)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.Load(file.OpenRead());
        
        if (xmlDocument.DocumentElement is null)
        {
            throw new Exception("Invalid Xml document");
        }

        if (xmlDocument.DocumentElement.Name != "players")
        {
            throw new Exception("Invalid Xml document root");
        }
        
        if (xmlDocument.DocumentElement.ChildNodes.Count == 0)
        {
            throw new Exception("Xml document contains no player data");
        }

        return xmlDocument;
    }
    
    private static IEnumerable<PlayerData> ParseXmlDocument(XmlDocument xmlDocument)
    {
        var seenPlayerData = new Dictionary<Type, HashSet<string>>(XmlNodePlayerDataTypeMap.Count);
        foreach(var value in XmlNodePlayerDataTypeMap.Values)
        {
            seenPlayerData[value] = new HashSet<string>();
        }

        foreach (XmlNode node in xmlDocument.DocumentElement!.ChildNodes)
        {
            var tryGetValueResult = XmlNodePlayerDataTypeMap.TryGetValue(node.Name, out var xmlNodeType);
            if (!tryGetValueResult)
            {
                Console.Error.WriteLine($"Unknown XmlNode: {node.OuterXml}");
                continue;
            }
            
            var serializer = new XmlSerializer(xmlNodeType!);
            using TextReader reader = new StringReader(node.OuterXml);

            if (serializer.Deserialize(reader) is not PlayerData playerData || !playerData.IsValid())
            {
                Console.Error.WriteLine($"Invalid XmlNode: {node.OuterXml}");
                continue;
            }
            
            if (!seenPlayerData[xmlNodeType!].Add(playerData.Id))
            {
                Console.Error.WriteLine($"Duplicate XmlNode: {node.OuterXml}");
                continue;
            }
            
            yield return playerData;
        }
    }
    
    private void PublishEvents(PublishEventsCommand request, IEnumerable<PlayerData> playersData)
    {
        using var connection = _connectionFactory.CreateConnection(request.Hostnames);
        using var channel = connection.CreateModel();
        channel.TxSelect();
        
        try
        {
            var batch = channel.CreateBasicPublishBatch();
            
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

            var fileHash = CalculateMd5(request.File);

            var eventsPublishedCounter = 0;
            foreach (var e in _eventsMapper.Map(playersData))
            {
                var properties = channel.CreateBasicProperties();
                properties.Headers = new Dictionary<string, object>
                {
                    { "id", $"{Guid.NewGuid()}" },
                    { "filename", request.File.Name },
                    { "file-hash", fileHash }
                };

                var jsonString = JsonSerializer.Serialize(e, serializeOptions);
                var body = Encoding.UTF8.GetBytes(jsonString);

                batch.Add(exchange: request.ExchangeName,
                    routingKey: string.Empty,
                    mandatory: false,
                    properties: properties,
                    body: body.AsMemory());

                eventsPublishedCounter++;
            }
            
            batch.Publish();
            channel.TxCommit();
            
            Console.WriteLine($"Published {eventsPublishedCounter} events");
        }
        catch (Exception)
        {
            channel.TxRollback();
            throw;
        }
    }
    
    private static string CalculateMd5(FileInfo file)
    {
        using var stream = file.OpenRead();
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
    }
}
