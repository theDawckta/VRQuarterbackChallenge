using System;

public class BasketballPlayerStatistics : PlayerStatistics {

    public TimeSpan MinutesPlayed { get; set; }
    public bool Active { get; set; }
    public bool Starter { get; set; }
    public DateTime Updated { get; set; }

    public static readonly StatisticDefinitionCollection StatisticDefinitions = new StatisticDefinitionCollection
    {
        { "points", "PTS" },
        { "FGM-A", "FG" },
        { "3PM-A",  "3PT" },
        { "rebounds", "REB" },
        { "assists", "AST"  },
        { "personal_fouls", "PF" },
    };

}
