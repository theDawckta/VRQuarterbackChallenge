using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootballScoreboardDataModel : BallGameScoreboardDataModel {

    public static int MAX_TIMEOUTS_OF_FOOTBALL = 3;

    public int NumberOfDowns;
    public int ToGoInYard;
    public int BallOnInYard;
    public bool IsOffendedByHome = true;

    public FootballScoreboardDataModel() : base()
    {
        _gameType = "FTB";
    }

    /// <summary>
    /// Updates the scoreboard data.
    /// SportRadar Data Format: R\FTB\NFL\1\MIN\NEB\PRI\SEC\7\21\3\3\1\10\2\23\NEB
    ///    - Field 0:  Response packet type (always “R”)
    ///    - Field 1:  Sport identifier (always “FTB” for Football)
    ///    - Field 2:  League identifier ( NFL/NCAA)
    ///    - Field 3:  Match ID
    ///    - Field 4:  Away team ID
    ///    - Field 5:  Home team ID
    ///    - Field 6:  Away team color [PRI/SEC]
    ///    - Field 7:  Home team color [PRI/SEC]
    ///    - Field 8:  Away score
    ///    - Field 9:  Home score
    ///    - Field 10:  Away time out left (1-3)
    ///    - Field 11:  Home time out left (1-3)
    ///    - Field 12: Down (1-4)
    ///    - Field 13: Distance (yds)
    ///    - Field 14: Quarter (1-4)
    ///    - Field 15: Ball on (yard line: 1-50)
    ///    - Field 16: Possession (Away team ID or Home team ID)
    /// 
    /// </summary>
    /// <returns><c>true</c>, if scoreboard data was updated, <c>false</c> otherwise.</returns>
    /// <param name="scoreFields">Score fields.</param>
    public override bool UpdateScoreboardData(string[] scoreFields)
    {
        // Validate the input data
        if (scoreFields.Length < 17)
        {
            Debug.LogError("FootballScoreboardDataModel::UpdateScoreboardData():The input parameter did not contain enough information.");
            return false;
        }

        base.UpdateScoreboardData(scoreFields);

        int nVal;
        bool bVal;
        string strVal;

        int index = 1;
        if (scoreFields[index].Contains(".") || scoreFields[1].StartsWith("V"))
        {
            _version = scoreFields[index++];
        }

        // Game Type
        _gameType = scoreFields[index++];

        // League ID
        _leagueType = scoreFields[index++];

        // Match ID
        _matchId = scoreFields[index++];

        // Away Team ID
        strVal = scoreFields[index++];
        if (AwayTeamID != strVal)
        {
            AwayTeamID = strVal;

            AwayTeamName = getTeamNameById(AwayTeamID);;
            OnScoreboardDataChanged("AwayTeamName", AwayTeamName);

            AwayTeamNickname = getTeamNicknameById(AwayTeamID);;
            OnScoreboardDataChanged("AwayTeamNickname", AwayTeamNickname);
        }

        // Home Team ID
        strVal = scoreFields[index++];
        if (HomeTeamID != strVal)
        {
            HomeTeamID = strVal;

            HomeTeamName = getTeamNameById(HomeTeamID);;
            OnScoreboardDataChanged("HomeTeamName", HomeTeamName);

            HomeTeamNickname = getTeamNicknameById(HomeTeamID);;
            OnScoreboardDataChanged("HomeTeamNickname", HomeTeamNickname);
        }

        // Away Team Color & Logo
        AwayTeamColor = getTeamColorById(AwayTeamID, false);
        strVal = getTeamLogoById(AwayTeamID);
        if (AwayTeamLogoUrl != strVal)
        {
            AwayTeamLogoUrl = strVal;
            OnScoreboardDataChanged("AwayTeamLogoUrl", AwayTeamLogoUrl);
        }
        index++;

        // Home Team Color & Logo
        HomeTeamColor = getTeamColorById(HomeTeamID, true);
        strVal = getTeamLogoById(HomeTeamID);
        if (HomeTeamLogoUrl != strVal)
        {
            HomeTeamLogoUrl = strVal;
            OnScoreboardDataChanged("HomeTeamLogoUrl", HomeTeamLogoUrl);
        }
        index++;

        // Scores
        int.TryParse(scoreFields[index++], out nVal);
        if (AwayTeamScore != nVal)
        {
            AwayTeamScore = nVal;
            OnScoreboardDataChanged("AwayTeamScore", AwayTeamScore.ToString());
        }
        int.TryParse(scoreFields[index++], out nVal);
        if (HomeTeamScore != nVal)
        {
            HomeTeamScore = nVal;
            OnScoreboardDataChanged("HomeTeamScore", HomeTeamScore.ToString());
        }
 
        // Remaining TimeOut
        int.TryParse(scoreFields[index++], out nVal);
        if (nVal != AwayTeamTimeouts && nVal >= 0 && nVal <= MAX_TIMEOUTS_OF_FOOTBALL)
        {
            AwayTeamTimeouts = nVal;
            OnScoreboardDataChanged("AwayTeamTimeouts", AwayTeamTimeouts.ToString());
        }
        int.TryParse(scoreFields[index++], out nVal);
        if (nVal != HomeTeamTimeouts && nVal >= 0 && nVal <= MAX_TIMEOUTS_OF_FOOTBALL)
        {
            HomeTeamTimeouts = nVal;
            OnScoreboardDataChanged("HomeTeamTimeouts", HomeTeamTimeouts.ToString());
        }
            
        // Current Down 1-4
        int.TryParse(scoreFields[index++], out nVal);
        if (nVal != NumberOfDowns && NumberOfDowns >= 0 && NumberOfDowns <= 4)
        {
            NumberOfDowns = nVal;
            OnScoreboardDataChanged("NumberOfDowns", NumberOfDowns.ToString());
        }

        // Distange to go
        int.TryParse(scoreFields[index++], out nVal);
        if (nVal != ToGoInYard && nVal >= 0 && nVal <= 100)
        {
            ToGoInYard = nVal;
            OnScoreboardDataChanged("ToGoInYard", ToGoInYard.ToString());
        }

        // Period
        int.TryParse(scoreFields[index++], out nVal);
        if (nVal != Period)
        {
            Period = nVal;
            OnScoreboardDataChanged("Period", Period.ToString());
        }

        // Ball on
        int.TryParse(scoreFields[index++], out nVal);
        if (nVal != BallOnInYard && nVal >= 0 && nVal <= 100)
        {
            BallOnInYard = nVal;
            OnScoreboardDataChanged("BallOnInYard", BallOnInYard.ToString());
        }

        // Possession
        bVal = (scoreFields[index++] == HomeTeamID)? true : false;
        if (bVal != IsOffendedByHome)
        {
            IsOffendedByHome = bVal;
            OnScoreboardDataChanged("IsOffendedByHome", IsOffendedByHome.ToString());
        }

        // UTC Time only if exists because the old version does not have it
        if (scoreFields.Length > index)
        {
            _utcTime = scoreFields[index++];
        }

        return (_isDataModelUpdated = true);

    }
}
