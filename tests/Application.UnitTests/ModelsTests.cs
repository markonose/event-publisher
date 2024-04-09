using FluentAssertions;

namespace Application.UnitTests;

public class ModelsTests
{
    [Fact]
    public void TestImplicitOperatorPlayerAchievementsEventFromPlayerRegistration()
    {
        var playerRegistration = new PlayerRegistration
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
        };

        var e = (PlayerAchievementsEvent)playerRegistration;

        e.PlayerId.Should().BeEquivalentTo(playerRegistration.Id);
        e.Achievements[0].Should().BeEquivalentTo(playerRegistration.Achievements[0]);
        e.Achievements[1].Should().BeEquivalentTo(playerRegistration.Achievements[1]);
    }
    
    [Fact]
    public void TestImplicitOperatorPlayerRegistrationEventFromPlayerRegistration()
    {
        var playerRegistration = new PlayerRegistration
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
        };

        var e = (PlayerRegistrationEvent)playerRegistration;

        e.Player.Should().BeEquivalentTo(new Player
        {
            Id = playerRegistration.Id,
            Name = playerRegistration.Name,
            Age = playerRegistration.Age,
            Country = playerRegistration.Country,
            Position = playerRegistration.Position,
        });
    }
}