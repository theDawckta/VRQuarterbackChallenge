using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GolfScoreboardDataModel : ScoreboardDataModel 
{
    public event EventHandler ScoreboardDataChanged;

    public static int MAX_NUM_OF_PLAYERS = 3;

    public class GolfPlayer
    {
        public string ID;
        public string Name;
        public string CountryCode;
        public string Rank;
        public int    Score;
    }

    protected bool _isDataModelUpdated = false;

    protected string _version;
    protected string _gameType;
    protected string _leagueType;
    protected string _matchId;
    protected string _utcTime;
    protected string _leagueLogoUrl;
 
    public bool IsDataModelUpdated
    {
        get { return _isDataModelUpdated; }
        set { _isDataModelUpdated = value; }
    }

    public string Version
    {
        get { return _version; }
    }

    public string GameType
    {
        get { return _gameType; }
    }

    public string LeagueType
    {
        get { return _leagueType; }
    }

    public string MatchID
    {
        get { return _matchId; }
    }

    public string UTCTime
    {
        get { return _utcTime; }
    }

    public string LeagueLogoUrl
    {
        get { return _leagueLogoUrl; }
        set 
        { 
            _leagueLogoUrl = value;
            OnScoreboardDataChanged("LeagueLogoUrl", LeagueLogoUrl);
        }
    }
        
    public string PlayerInfoUrl
    {
        get;
        set;
    }

    public string ScoreDataSource;
    public GolfPlayer[] GolfPlayers;
//    public string Player1ID;
//    public string Player1Rank;
//    public int    Player1Score;
//    public string Player2ID;
//    public string Player2Rank;
//    public int    Player2Score;
//    public string Player3ID;
//    public string Player3Rank;
//    public int    Player3Score;

    public GolfScoreboardDataModel()
    {
        GolfPlayers = new GolfPlayer[MAX_NUM_OF_PLAYERS];
        for (int i = 0; i < MAX_NUM_OF_PLAYERS; i++)
        {
            GolfPlayers[i] = new GolfPlayer();
        }
    }

    protected virtual void OnScoreboardDataChanged(string propName, string propValue)
    {
        ScoreboardEventArgs sbe = new ScoreboardEventArgs(_gameType, _leagueType, _matchId, propName, propValue);

        if (ScoreboardDataChanged != null)
            ScoreboardDataChanged(this, sbe);
    }
        
    /// <summary>
    /// Updates the scoreboard data.
    /// Data Format: R\V1.0\GLF\PGAT\PGAT\R2017041\29974\T1\-12\29975\T1\-12\29976\T1\-12\2017-04-20 16:47:33
    ///     - Field 0:  Response packet type (always “R”)
    ///     - Field 1:  Version Number of scoreboard client
    ///     - Field 2:  Sport identifier (always “GLF” for Golf)
    ///     - Field 3:  Source for data
    ///     - Field 4:  League identifier ( PGAT- PGA Tour)
    ///     - Field 5:  Tournament ID
    ///     - Field 6:  Rank 1 Player ID
    ///     - Field 7:  Player Rank
    ///     - Field 8:  Player Score
    ///     - Field 9:  Rank 2 Player ID
    ///     - Field 10: Player Rank
    ///     - Field 11: Player Score
    ///     - Field 12: Rank 3 Player ID
    ///     - Field 13: Player Rank
    ///     - Field 14: Player Score
    ///     - Field 15: UTC Time stamp
    /// </summary>
    /// <returns><c>true</c>, if scoreboard data was updated, <c>false</c> otherwise.</returns>
    /// <param name="scoreFields">Score fields.</param>
    public virtual bool UpdateScoreboardData(string[] scoreFields)
    {
        // Validate the input data
        if (scoreFields.Length < 16)
        {
            Debug.LogError("GolfScoreboardDataModel::UpdateScoreboardData():The input parameter did not contain enough information.");
            return false;
        }

        int nVal;
        string strVal;

        int index = 1;
        if (scoreFields[index].Contains(".") || scoreFields[1].StartsWith("V"))
        {
            _version = scoreFields[index++];
        }

        // Game Type
        _gameType = scoreFields[index++];

        // Source for data
        ScoreDataSource = scoreFields[index++];

        // League ID
        _leagueType = scoreFields[index++];

        // Match ID
        _matchId = scoreFields[index++];

        // Players' ID/Name/Country Code/Rank/Score
        for (int i = 0; i < MAX_NUM_OF_PLAYERS; i++)
        {
            GolfPlayer player = GolfPlayers[i];

            String affix = string.Format("Player[{0}].", i + 1);

            strVal = scoreFields[index++];
            //if (!string.IsNullOrEmpty(strVal))
            //{
                if (player.ID != strVal)
                {
                    player.ID = strVal;
                    OnScoreboardDataChanged(affix + "ID", player.ID);

                    fillPlayerDetailInfo(PlayerInfoUrl, player);
                    OnScoreboardDataChanged(affix + "Name", player.Name);
                    OnScoreboardDataChanged(affix + "CountryCode", player.CountryCode);
                }
                    
                strVal = scoreFields[index++];
                if (player.Rank != strVal)
                {
                    player.Rank = strVal;
                    OnScoreboardDataChanged(affix + "Rank", player.Rank);
                }

                int.TryParse(scoreFields[index++], out nVal);
                if (player.Score != nVal)
                {
                    player.Score = nVal;
                    OnScoreboardDataChanged(affix + "Score", player.Score.ToString("+#;-#;0"));
                }

                // ONLY FOR TEST
//                Debug.Log(string.Format("Player Info: ID={0}, Name={1}, CountryCode={2}, Rank={3}, Score={4}",
//                        player.ID, player.Name, player.CountryCode, player.Rank, player.Score));
            //}
        }
            
        // UTC Time
        _utcTime = scoreFields[index++];

        return (_isDataModelUpdated = true);
    }


    private bool fillPlayerDetailInfo(string playerInfoUrl, GolfPlayer player)
    {
        if (string.IsNullOrEmpty(playerInfoUrl) || player == null || string.IsNullOrEmpty(player.ID))
        {
            Debug.LogError("GolfScoreboardDataModel::fetchPlayerDetailInfo(): The given url is not correct or a player id is missing.");
        }
            
        string[] playerDetails;
        WWW www = new WWW(string.Format(playerInfoUrl, player.ID));
        while (!www.isDone) { }     // TODO: JK COMMENT, Is there any good way to download the JSON fils synchronously?
        if (string.IsNullOrEmpty(www.error))
        {
            playerDetails = www.text.Split(',');
            if (playerDetails.Length > 2)
            {
                player.Name = playerDetails[1].ToUpper();
                player.CountryCode = playerDetails[2].Substring(0, 3);
            }
        }
        else
        {
            Debug.LogErrorFormat("GolfScoreboardDataModel::fetchPlayerDetailInfo():Error fetching the player info (ID={0}), Error={1}", player.ID, www.error);
            return false;
        }

        return true;
   }
}
