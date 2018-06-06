using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BallGameScoreboardDataModel : ScoreboardDataModel {

    protected bool _isDataModelUpdated = false;

    protected string _version;
    protected string _gameType;
    protected string _leagueType;
    protected string _matchId;
    protected string _utcTime;
    protected string _teamInfoUrl;
    protected string _leagueLogoUrl;
    protected string _playerInfoUrl;

    // Common score fields for the ball games
    public string AwayTeamID;
    public string HomeTeamID;
    public string AwayTeamName;
    public string HomeTeamName;
    public string AwayTeamNickname;
    public string HomeTeamNickname;
    public string AwayTeamColor;
    public string HomeTeamColor;
    public string AwayTeamLogoUrl;
    public string HomeTeamLogoUrl;
    public int AwayTeamScore;
    public int HomeTeamScore;
    public int AwayTeamTimeouts;
    public int HomeTeamTimeouts;
    public int Period;
	public string PeriodStr;

    public enum SportsDataProviderType
    {
        None,
        SMT,
        SportRadar,
        Stats,
        PGATour
    };

    // Team Info
    protected Dictionary<string, TeamInfo> mTeamInfos;

    // Events
    public event EventHandler ScoreboardDataChanged;

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
        
    public string TeamInfoUrl
    {
        get { return _teamInfoUrl; }
        set 
        {
            if (string.IsNullOrEmpty(_teamInfoUrl) || (_teamInfoUrl != value))
            {
                mTeamInfos.Clear();
                _teamInfoUrl = value;
            }
        }
    }

    // TODO
    public string PlayerInfoUrl
    {
        get { return _playerInfoUrl; }
        set
        {
            if (_playerInfoUrl != value)
            {
                _playerInfoUrl = value;
                OnScoreboardDataChanged("PlayerInfoUrl", _playerInfoUrl);
            }
        }
    }

    protected BallGameScoreboardDataModel()
    {
        mTeamInfos = new Dictionary<string, TeamInfo>();
    }

    protected virtual void OnScoreboardDataChanged(string propName, string propValue)
    {
        ScoreboardEventArgs sbe = new ScoreboardEventArgs(_gameType, _leagueType, _matchId, propName, propValue);

        if (ScoreboardDataChanged != null)
            ScoreboardDataChanged(this, sbe);
    }

    public virtual bool UpdateScoreboardData(string[] scorefields)
    {
        // Update the team info
        if (mTeamInfos.Count == 0)
        {
            downloadAndPopulateTeamInfos(_teamInfoUrl);
        }

        return true;
    }

    /// <summary>
    /// Called to download the JSON file based on the game and league played
    /// </summary>
    private bool downloadAndPopulateTeamInfos(string url)
    {
        // Verify the input parameters
        if (string.IsNullOrEmpty(url))
        {
            // JK DEBUG, Throw an exception
            return false;
        }
            
        Debug.Log("BallGameScoreboardDataModel::downloadAndPopulateTeamInfos(): Downloading a JSON file from " + url);

        // Download the JSON file
        WWW www = new WWW(url);
        while (!www.isDone) { }     // TODO: JK COMMENT, Is there any good way to download the JSON fils synchronously?
        if (string.IsNullOrEmpty(www.error))
        {
            if (url.ToLower().EndsWith(".json"))
            {
                populateTeamInfosFromJson(www.text);
            }
            else
            {
                Debug.LogError("BallGameScoreboardDataModel::downloadAndPopulateTeamInfos(): The given URL is not supported yet. url = " + url);
            }

            Debug.Log("BallGameScoreboardDataModel::downloadAndPopulateTeamInfos(): Finish parsing the JSON file.");
        }
        else
        {
            Debug.LogError("BallGameScoreboardDataModel::downloadAndPopulateTeamInfos(): Fail to get the JSON file from " + url + ". Error=" + www.error);
            return false;
        }

        return true;
    }

    private void populateTeamInfosFromJson(string jsonString)
    {
        /***
         * Sample JSON file
         * {
                "team_info": {
                    "SEA": {
                      "name": "SEAHAWKS",
                      "logo": "http://config.vokevr.com/vokegearvrapp/2016-NFL/TeamLogos/seattleSeahawks.jpg"
                    }
            }
         ***/

        JSONObject jsonRoot = new JSONObject(jsonString);
        if (jsonRoot.type != JSONObject.Type.OBJECT || jsonRoot.keys.Count <= 0)
        {
            Debug.LogError("BallGameScoreboardDataModel::populateTeamInfosFromJson(): JSON FORMAT ERROR: Could not get the 'team_info' key from the JSON string.");
            return;
        }

        JSONObject jsonTeamInfos = (JSONObject)jsonRoot.list[0];
        if (jsonTeamInfos.type != JSONObject.Type.OBJECT || jsonTeamInfos.keys.Count <= 0)
        {
            Debug.LogError("BallGameScoreboardDataModel::populateTeamInfosFromJson(): JSON FORMAT ERROR: Could not get any team info from the JSON string.");
            return;
        }

        // Parse the each team information
        for (int i = 0; i < jsonTeamInfos.list.Count; i++)
        {
            string key = (string)jsonTeamInfos.keys[i];
            JSONObject jsonTeamInfo = (JSONObject)jsonTeamInfos.list[i];
            TeamInfo teamInfo = populateTeamInfoFromJson(jsonTeamInfo);
            if (teamInfo != null)
            {
                teamInfo.ID = key;
                mTeamInfos.Add(key, teamInfo);
				Debug.Log ("key: " + key + ", " + teamInfo);
            }
        }

        // JK DEBUG, ONLY USED FOR DEBUGGING
//        Debug.Log(jsonString);
//        foreach(var pair in mTeamInfos)
//        {
//            string key = pair.Key;
//            TeamInfo ti = pair.Value;
//            Debug.Log("ID=" + ti.ID + ", Name=" + ti.Name + ", Nickname=" + ti.Nickname +
//                ", PrimaryColorCode=" + ti.ColorHome + ", SecondaryColorCode=" + ti.ColorAway);
//        }
    }

    private TeamInfo populateTeamInfoFromJson(JSONObject jsonTeamInfo)
    {
        if (jsonTeamInfo.type != JSONObject.Type.OBJECT || jsonTeamInfo.keys.Count <= 0)
        {
            Debug.LogError("BallGameScoreboardDataModel::populateTeamInfoFromJson(): JSON FORMAT ERROR: Could not parse an each team info from the JSON string.");
            return null;
        }

        TeamInfo teamInfo = new TeamInfo();

        for (int i = 0; i < jsonTeamInfo.list.Count; i++)
        {
            if (jsonTeamInfo.list[i].type == JSONObject.Type.STRING)
            {
                string key = (string)jsonTeamInfo.keys[i];
                string value = (string)jsonTeamInfo.list[i].str;

				if (key.Equals ("name"))
				{
					teamInfo.Name = value;
				} else if (key.Equals ("teamName"))
				{
					teamInfo.Nickname = value;
				} else if (key.Equals ("logo"))
				{
					teamInfo.LogoURL = value;
				} else if (key.Equals ("colorHome"))
				{
					teamInfo.ColorHome = value;
				} else if (key.Equals ("colorAway"))
				{
					teamInfo.ColorAway = value;
				} else if (key.Equals ("logoAway"))
				{
					teamInfo.LogoAway = value;
				} else if (key.Equals ("logoHome"))
				{
					teamInfo.LogoHome = value;
				}
            }
        }

        return teamInfo;
    }
        
    /// <summary>
    /// Looks up dictionary of teamInfo to get the name of the team associated with it
    /// SEA: Seattle
    /// CLE: Cleveleand
    /// </summary>
    protected string getTeamNameById(string id)
    {
        TeamInfo teamInfo;
        return mTeamInfos.TryGetValue(id, out teamInfo) ? teamInfo.Name : id;
    }

    /// <summary>
    /// Looks up dictionary of teamInfo to get the name of the team associated with it
    /// SEA: Seahawks
    /// CLE: Cavaliers
    /// </summary>
    protected string getTeamNicknameById(string id)
    {
        TeamInfo teamInfo;
        return mTeamInfos.TryGetValue(id, out teamInfo) ? teamInfo.Nickname : id;
    }

    /// <summary>
    /// Looks up dictionary of teamInfo to get the logo associated with the team
    /// </summary>
    protected string getTeamLogoById(string id)
    {
        TeamInfo teamInfo;
        return mTeamInfos.TryGetValue(id, out teamInfo) ? teamInfo.LogoURL : id;
    }

	/// <summary>
	/// Looks up dictionary of teamInfo to get the logo associated with the team
	/// </summary>
	protected string getTeamAwayLogoById(string id)
	{
		TeamInfo teamInfo;
		return mTeamInfos.TryGetValue(id, out teamInfo) ? teamInfo.LogoAway : id;
	}

	/// <summary>
	/// Looks up dictionary of teamInfo to get the logo associated with the team
	/// </summary>
	protected string getTeamHomeLogoById(string id)
	{
		TeamInfo teamInfo;
		return mTeamInfos.TryGetValue(id, out teamInfo) ? teamInfo.LogoHome : id;
	}

    /// <summary>
    /// Looks up dictionary of teamInfo to get the logo associated with the team
    /// </summary>
    protected string getTeamColorById(string id, bool isHomeTeam)
    {
        TeamInfo teamInfo;
        if (!mTeamInfos.TryGetValue(id, out teamInfo))
            return null;

        return (isHomeTeam == true) ? teamInfo.ColorHome : teamInfo.ColorAway;
    }
}
