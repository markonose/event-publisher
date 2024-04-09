using FluentAssertions;

namespace Application.UnitTests;

public record UnknownPlayerData : PlayerData
{
    public override bool IsValid()
    {
        throw new NotImplementedException();
    }
}

public class EventsMapperTests
{
    private readonly IEventsMapper _sut;
    
    public EventsMapperTests()
    {
        _sut = new EventsMapper();
    }
    
    [Fact]
    public void TestMap()
    {
        var playersData = new List<PlayerData>
        {
            new PlayerRegistration
            {
                Id = "1",
                Name = "Name",
                Age = "Age",
                Country = "Country",
                Position = "Position",
                Achievements = 
                [
                    new Achievement
                    {
                        Title = "Title1",
                        Year = "1",
                    },
                    new Achievement
                    {
                        Title = "Title2",
                        Year = "2",
                    },
                ],
            },
            new PlayerRegistration
            {
                Id = "2",
                Name = "Name",
                Age = "Age",
                Country = "Country",
                Position = "Position",
                Achievements = [],
            },
            new UnknownPlayerData
            {
                Id = "3",
            }
        };

        var result = _sut.Map(playersData);

        var enumeratedResult = result.ToList();
        enumeratedResult.Should().HaveCount(3);
    }
}