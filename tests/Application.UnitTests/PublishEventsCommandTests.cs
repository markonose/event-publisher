using System.Xml;
using FluentAssertions;
using Moq;
using RabbitMQ.Client;

namespace Application.UnitTests;

public class PublishEventsCommandTests
{
    private readonly ICommandHandler<PublishEventsCommand> _sut;
    
    public PublishEventsCommandTests()
    {
        Mock<IConnectionFactory> connectionFactory = new();
        Mock<IEventsMapper> eventsMapper = new();
        
        _sut = new PublishEventsCommandHandler(connectionFactory.Object, eventsMapper.Object);
    }
    
    [Fact]
    public void EmptyXmlFile()
    {
        var command = new PublishEventsCommand
        {
            File = new FileInfo("./Data/empty.xml"),
            Hostnames = ["localhost"],
            ExchangeName = "amq.headers",
        };
        
        Assert.Throws<FileNotFoundException>(
            () => _sut.Handle(command));
    }
    
    [Fact]
    public void EmptyRootXmlFile()
    {
        var command = new PublishEventsCommand
        {
            File = new FileInfo("./Data/empty_root.xml"),
            Hostnames = ["localhost"],
            ExchangeName = "amq.headers",
        };
        
        var exception = Assert.Throws<Exception>(
            () => _sut.Handle(command));

        exception.Message.Should().BeEquivalentTo("Xml document contains no player data");
    }
    
    [Fact]
    public void InvalidXmlFile()
    {
        var command = new PublishEventsCommand
        {
            File = new FileInfo("./Data/invalid.xml"),
            Hostnames = ["localhost"],
            ExchangeName = "amq.headers",
        };
        
        Assert.Throws<FileNotFoundException>(
            () => _sut.Handle(command));
    }
    
    [Fact]
    public void InvalidRootXmlFile()
    {
        var command = new PublishEventsCommand
        {
            File = new FileInfo("./Data/invalid_root.xml"),
            Hostnames = ["localhost"],
            ExchangeName = "amq.headers",
        };
        
        var exception = Assert.Throws<Exception>(
            () => _sut.Handle(command));

        exception.Message.Should().BeEquivalentTo("Invalid Xml document root");
    }
    
    [Fact]
    public void MissingRootXmlFile()
    {
        var command = new PublishEventsCommand
        {
            File = new FileInfo("./Data/missing_root.xml"),
            Hostnames = ["localhost"],
            ExchangeName = "amq.headers",
        };
        
        Assert.Throws<XmlException>(
            () => _sut.Handle(command));
    }
}