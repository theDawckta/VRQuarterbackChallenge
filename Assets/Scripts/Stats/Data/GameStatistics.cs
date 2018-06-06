using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using UnityEngine;

[Serializable]
public class GameStatistics
{
    public GameStatistics()
    { }

    //public Guid ApiID { get; set; }
	public string ApiID { get; set; }
    public string ID { get; set; }
    public string Name { get; set; }
	public string LogoURL { get; set; }
    public DateTime Scheduled { get; set; }
    public GameStatus Status { get; set; }
    public GameType Type { get; set; }

	public virtual GameStatistics Parse(string json, string statType="")
    {
        Debug.Log("****************************** Base class GameStatistics Parse method ****************************************");
        return new GameStatistics();
    }

    protected void AddGenericStats(Statistics obj, JSONObject json, StatisticDefinitionCollection definitions)
    {
        var orderedKeys = from k in json.keys
                          orderby definitions.IndexOf(k)
                          select k;

        foreach (var key in orderedKeys)
        {
            StatisticDefinition def;
            if (!definitions.TryGetValue(key, out def))
                continue;

            string label = def.GetLabel(key);
            string value = def.GetValue(json[key]);

            obj[label] = value;
        }
    }

    protected TimeSpan ParseTime(string value)
    {
        // nonstandard time format

        string[] parts = value.Split(':');

        int minutes, seconds;

        if (parts.Length != 2 || !Int32.TryParse(parts[0], out minutes) || !Int32.TryParse(parts[1], out seconds))
        {
            Debug.LogWarningFormat("Invalid time format '{0}'", value);
            return TimeSpan.Zero;
        }

        var ts = new TimeSpan(0, minutes, seconds);

        return ts;
    }

    protected DateTime ParseDate(string s)
    {
        return DateTime.Parse(s, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);
    }
}