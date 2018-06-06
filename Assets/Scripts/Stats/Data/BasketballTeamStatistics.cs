using System;
using System.Collections.Generic;

public class BasketballTeamStatistics : TeamStatistics {

    public IList<BasketballPlayerStatistics> Players { get; private set; }

    public BasketballTeamStatistics()
    {
        Players = new List<BasketballPlayerStatistics>();
    }

    public static readonly StatisticDefinitionCollection StatisticDefinitions = new StatisticDefinitionCollection
    {
        { "field_goals_pct", "FG%" },
        { "three_points_pct", "3PT%" },
        { "turnovers", "TO"  },
        { "rebounds", "REB"  }
    };
}
