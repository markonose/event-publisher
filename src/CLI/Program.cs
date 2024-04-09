using System.CommandLine;
using Application;
using RabbitMQ.Client;

namespace CLI;

class Program
{
    private static int Main(string[] args)
    {
        var rootCommand = CreateRootCommand();
        return rootCommand.Invoke(args);
    }

    private static RootCommand CreateRootCommand()
    {
        var fileOption = new Option<FileInfo>(
            name: "--file",
            description: "Path to file")
        {
            IsRequired = true
        };

        var hostnamesOption = new Option<string[]>(
            name: "--hostnames",
            description: "RabbitMQ hostnames")
        {
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true,
        };

        var usernameOption = new Option<string>(
            name: "--username",
            description: "RabbitMQ username",
            getDefaultValue: () => "guest");

        var passwordOption = new Option<string>(
            name: "--password",
            description: "RabbitMQ password",
            getDefaultValue: () => "guest");
        
        var virtualHostOption = new Option<string>(
            name: "--vhost",
            description: "RabbitMQ virtual host",
            getDefaultValue: () => "/");
        
        var exchangeNameOption = new Option<string>(
            name: "--exchange-name",
            description: "RabbitMQ headers exchange name",
            getDefaultValue: () => "amq.headers");
        
        
        var rootCommand = new RootCommand
        {
            fileOption,
            hostnamesOption,
            usernameOption,
            passwordOption,
            virtualHostOption,
            exchangeNameOption,
        };
        rootCommand.SetHandler(Handler,
            fileOption,
            hostnamesOption,
            usernameOption,
            passwordOption,
            virtualHostOption,
            exchangeNameOption);

        return rootCommand;
    }

    private static Task<int> Handler(FileInfo file,
        string[] hostnames,
        string username,
        string password,
        string virtualHost,
        string exchangeName)
    {
        var connectionFactory = new ConnectionFactory
        {
            UserName = username,
            Password = password,
            VirtualHost = virtualHost,
        };
        var eventsMapper = new EventsMapper();
        var commandHandler = new PublishEventsCommandHandler(connectionFactory, eventsMapper);
        
        var command = new PublishEventsCommand()
        {
            File = file,
            Hostnames = hostnames,
            ExchangeName = exchangeName,
        };

        try
        {
            commandHandler.Handle(command);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Task.FromResult(-1);
        }
        
        return Task.FromResult(0);
    }
}
