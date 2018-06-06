using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ScoreboardEventArgs : EventArgs
{
    public ScoreboardEventArgs(string sportType, string leagueType, string matchId, string name, string value)
    {
        PropertyName = name;
        PropertyValue = value;
    }

    public string SportType;
    public string LeagueType;
    public string MatchId;
    public string PropertyName;
    public string PropertyValue;
}