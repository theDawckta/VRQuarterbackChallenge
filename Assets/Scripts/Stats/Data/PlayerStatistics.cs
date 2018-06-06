using System;

[Serializable]
public class PlayerStatistics : Statistics
{
    public string Name { get; set; }
    public bool Played { get; set; }

    //public static readonly StatisticDefinitionCollection StatisticDefinitions = new StatisticDefinitionCollection
    //{
    //};
}