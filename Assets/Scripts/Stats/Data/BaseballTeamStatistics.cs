using System.Collections.Generic;

public class BaseballTeamStatistics : TeamStatistics
{
    public string Runs { get; set; }
    public string Hits { get; set; }
    public string Errors { get; set; }
    public List<InningInfo> Innings { get; set; }

    public IList<BaseballPlayerStatistics> Players { get; private set; }
    public IList<BaseballPlayerStatistics> Pitchers { get; private set; }

    public BaseballTeamStatistics()
    {
        Players = new List<BaseballPlayerStatistics>();
        Pitchers = new List<BaseballPlayerStatistics>();
    }

    public static readonly StatisticDefinitionCollection StatisticDefinitions = new StatisticDefinitionCollection
    {
    };
}
