namespace Application;

public interface IEventsMapper
{
    IEnumerable<Event> Map(IEnumerable<PlayerData> playersData);
}

public class EventsMapper : IEventsMapper
{
    private static readonly Dictionary<Type, Func<PlayerData, IEnumerable<Event>>> PlayerDataTypeHandlerMap = new()
    {
        {typeof(PlayerRegistration), AddEventsForPlayerRegistration}
    };
    
    public IEnumerable<Event> Map(IEnumerable<PlayerData> playersData)
    {
        foreach (var playerData in playersData)
        {
            var playerDataType = playerData.GetType();
            
            var tryGetValueResult = PlayerDataTypeHandlerMap.TryGetValue(playerDataType, out var action);
            if (!tryGetValueResult)
            {
                Console.Error.WriteLine($"Unknown PlayerData type: {playerDataType}");
                continue;
            }

            foreach (var e in action!(playerData))
            {
                yield return e;
            }
        }
    }

    private static IEnumerable<Event> AddEventsForPlayerRegistration(PlayerData playerData)
    {
        var playerRegistration = (PlayerRegistration) playerData;
        
        yield return (PlayerRegistrationEvent) playerRegistration;

        if (playerRegistration.Achievements.Length != 0)
        {
            yield return (PlayerAchievementsEvent) playerRegistration;
        }
    }
}
