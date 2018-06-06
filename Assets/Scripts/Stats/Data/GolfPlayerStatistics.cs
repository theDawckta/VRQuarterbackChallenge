using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfPlayerStatistics : PlayerStatistics {
    //PGA stats
    public string Position { get; set; }
    public string Country { get; set; }
    public string Total { get; set; }
    public string Thru { get; set; }
    public string Round { get; set; }
    public string PlayerID { get; set; }

    public static readonly StatisticDefinitionCollection StatisticDefinitions = new StatisticDefinitionCollection
    {
    };
}
