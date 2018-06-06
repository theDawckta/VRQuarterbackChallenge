using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class TeamStatistics : Statistics
{
    public string Name { get; set; }
    public string Abbreviation { get; set; }
    public string Location { get; set; }
    public string LogoURL { get; set; }

    //public static readonly StatisticDefinitionCollection StatisticDefinitions = new StatisticDefinitionCollection
    //{
    //};
}