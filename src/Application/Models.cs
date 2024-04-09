using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Application;

[JsonDerivedType(typeof(PlayerAchievementsEvent))]
[JsonDerivedType(typeof(PlayerRegistrationEvent))]
public abstract record Event
{
    public required string EventType { get; init; }
}

public record PlayerAchievementsEvent : Event
{
    public required string PlayerId { get; init; }
    public required Achievement[] Achievements { get; init; }

    public static implicit operator PlayerAchievementsEvent(PlayerRegistration playerRegistration)
    {
        return new PlayerAchievementsEvent
        {
            EventType = "player_registration",
            PlayerId = playerRegistration.Id,
            Achievements = playerRegistration.Achievements,
        };
    }
}

public record PlayerRegistrationEvent : Event
{
    public required Player Player { get; init; }
    
    public static implicit operator PlayerRegistrationEvent(PlayerRegistration playerRegistration)
    {
        return new PlayerRegistrationEvent
        {
            EventType = "player_achievements",
            Player = new Player
            {
                Id = playerRegistration.Id,
                Name = playerRegistration.Name,
                Age = playerRegistration.Age,
                Country = playerRegistration.Country,
                Position = playerRegistration.Position,
            }
        };
    }
}

public record Player
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Age { get; init; }
    public required string Country { get; init; }
    public required string Position { get; init; }
}

public abstract record PlayerData
{
    [XmlElement("id")]
    public required string Id { get; init; }
    
    public abstract bool IsValid();
}

[XmlRoot("player_registration")]
public record PlayerRegistration : PlayerData
{
    [XmlElement("name")]
    public required string Name { get; init; }

    [XmlElement("age")]
    public required string Age { get; init; }

    [XmlElement("country")]
    public required string Country { get; init; }

    [XmlElement("position")]
    public required string Position { get; init; }

    [XmlArray("achievements")]
    [XmlArrayItem("achievement")]
    public required Achievement[] Achievements { get; init; }

    public override bool IsValid()
    {
        return !string.IsNullOrEmpty(Id)
               && !string.IsNullOrEmpty(Name)
               && !string.IsNullOrEmpty(Age)
               && !string.IsNullOrEmpty(Country)
               && !string.IsNullOrEmpty(Position)
               && Achievements is not null
               && Achievements.All(x => x.IsValid());
    }
}

public record Achievement
{
    [XmlAttribute("year")]
    public required string Year { get; init; }

    [XmlText]
    public required string Title { get; init; }
    
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Year) && !string.IsNullOrEmpty(Title);
    }
}