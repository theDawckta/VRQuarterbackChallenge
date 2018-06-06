using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Scoreboard data model.
/// The detail information about the scoreboard data format can be found at 
/// https://vokevr.atlassian.net/wiki/spaces/GPT/pages/26083429/Scoreboard+Data+Format
/// </summary>
public interface ScoreboardDataModel 
{
    // Event
    event EventHandler ScoreboardDataChanged;

    // Properties
    bool IsDataModelUpdated
    {
        get;
        set;
    }

    string Version
    {
        get;
    }

    string GameType
    {
        get;
    }

    string LeagueType
    {
        get;
    }

    string LeagueLogoUrl
    {
        get;
        set;
    }

    string MatchID
    {
        get;
    }

    string UTCTime
    {
        get;
    }

    // Interfaces
    bool UpdateScoreboardData(string[] scorefields);
}
