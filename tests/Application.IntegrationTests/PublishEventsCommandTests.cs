using System.Text;
using FluentAssertions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Application.IntegrationTests;

public class PublishEventsCommandTests
{
    private readonly ICommandHandler<PublishEventsCommand> _sut;
    
    public PublishEventsCommandTests()
    {
        IConnectionFactory connectionFactory = new ConnectionFactory();
        IEventsMapper eventsMapper = new EventsMapper();

        _sut = new PublishEventsCommandHandler(connectionFactory, eventsMapper);
    }
    
    [Fact]
    public void SendToExchangeWithBoundQueue()
    {
        ConfigureQueueBinding("test_queue");
        
        var command = new PublishEventsCommand
        {
            File = new FileInfo("./players.xml"),
            Hostnames = ["localhost"],
            ExchangeName = "amq.headers",
        };
        
        _sut.Handle(command);

        var messages = ConsumeAllMessages("test_queue");
        messages.Should().Equal(new List<string>
        {
            "{\"player\":{\"id\":\"1\",\"name\":\"John Doe\",\"age\":\"21\",\"country\":\"USA\",\"position\":\"Guard\"},\"event_type\":\"player_achievements\"}",
            "{\"player_id\":\"1\",\"achievements\":[{\"year\":\"2022\",\"title\":\"League Winner\"},{\"year\":\"2023\",\"title\":\"Top Scorer\"}],\"event_type\":\"player_registration\"}",
            "{\"player\":{\"id\":\"2\",\"name\":\"Jane Doe\",\"age\":\"22\",\"country\":\"Slovenia\",\"position\":\"Forward\"},\"event_type\":\"player_achievements\"}",
            "{\"player_id\":\"2\",\"achievements\":[{\"year\":\"2022\",\"title\":\"League Winner\"},{\"year\":\"2023\",\"title\":\"Top Scorer\"}],\"event_type\":\"player_registration\"}",
            "{\"player\":{\"id\":\"3\",\"name\":\"Achievements McNoachievmentson\",\"age\":\"23\",\"country\":\"Slovenia\",\"position\":\"Backwards\"},\"event_type\":\"player_achievements\"}",
        });
    }

    private static void ConfigureQueueBinding(string queueName)
    {
        var connectionFactory = new ConnectionFactory();
        using var connection = connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();
        
        channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueBind(queueName, "amq.headers", string.Empty);
    }

    private static List<string> ConsumeAllMessages(string queueName)
    {
        var connectionFactory = new ConnectionFactory();
        using var connection = connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();
        
        var messages = new List<string>();
        
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            messages.Add(message);
        };
        channel.BasicConsume(queue: queueName,
            autoAck: true,
            consumer: consumer);

        Thread.Sleep(TimeSpan.FromSeconds(5));
        
        return messages;
    }
}