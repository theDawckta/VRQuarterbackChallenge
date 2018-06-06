

public class BaseballPlayerStatistics : PlayerStatistics
{
    public static readonly StatisticDefinitionCollection StatisticDefinitions = new StatisticDefinitionCollection
    {
        //baseball
		{"innings_pitched", "IP"},
		{ "atbats", "AB"  },
		{ "hits", "H"  },
		{ "runs", "R"  },
		{ "earned_runs", "ER" },
		{ "runs_battedIn", "RBI"},
		{ "base_on_ball", "BB"},
		{ "strikeouts", "SO"},
		{ "batting_average", "BA"},
		{ "slugging_percentage", "SLG"},
		{ "home_runs", "HR"},
		{ "earned_run_average", "ERA"},
        { "playerID", "playerID"}
    };
}
