using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketballScoreboardDataModel : BallGameScoreboardDataModel {

    public static int MAX_TIMEOUTS_OF_BASKETBALL = 7;

    public int AwayTeamFouls = -1;
	public int HomeTeamFouls = -1;
    public string GameProgressStatus;
    public bool IsOffendedByHome = true;

    public BasketballScoreboardDataModel() : base ()
    {
    }

    /// <summary>
    /// Updates the scoreboard data.
	/// R\BKB\NCAA\1\Kentucky\Louisville\40\39\3\4\0\0\1\
    /// SportsRadar Format: R\BKB\NCAA\1\CHI\CLE\7\21\5\5\3\3\2\CHI
    ///    - Field 0:  Response packet type (always “R”)
    ///    - Field 1:  Sport identifier (always “BKB” for Basketball)
    ///    - Field 2:  League identifier ( NBA/NCAA)
    ///    - Field 3:  Match ID
    ///    - Field 4:  Away team ID
    ///    - Field 5:  Home team ID
    ///    - Field 6:  Away score
    ///    - Field 7:  Home score
    ///    - Field 8:  Away time out left (0-5)
    ///    - Field 9:  Home time out left (0-5)
    ///    - Field 10: Away Fouls to give
    ///    - Field 11: Home Fouls to give
    ///    - Field 12: Quarter (1-4), OT {1,2...}
    ///    - Field 13: Possession (Away team ID or Home team ID)
	///
	/// new data
	///
	/// R\V1.0\BKB\NCAA\1\Kentucky\Louisville\7\21\5\5\3\3\2\2017-04-10 16:47:33
	///
//	Field 0: Response packet type Always 'R'
//	Field 1: Version Number
//	Field 2: Sport identifier “BKB” represents a basketball
//	Field 3: League identifier ( NBA/NCAA)
//	Field 4: Match ID
//	Field 5: Away team ID
//	Field 6: Home team ID
//	Field 7: Away score
//	Field 8: Home score
//	Field 9: Away time out left
//	Field 10: Home time out left
//	Field 11: Away Fouls to give
//	Field 12: Home Fouls to give
//	Field 13: Period - Current Period in a game
//	NBA: Pre-game(0), Quarter (1-4), OT
//
//	{5,..,14}
//	Field 14: UTC Time stamp
    ///
    /// SMT Format: R\1\UGA\SCAR\74\72\2\3\10\11\F\2\\
    ///    - Field 0:  Response packet type (always “R”)
    ///    - Field 1:  Response machine number (always “1”)
    ///    - Field 2:  Away team ID
    ///    - Field 3:  Home team ID
    ///    - Field 4:  Away score
    ///    - Field 5:  Home score
    ///    - Field 6:  Away timeouts remaining
    ///    - Field 7:  Home timeouts remaining
    ///    - Field 8:  Away team fouls for the current half
    ///    - Field 9:  Home team fouls for the current half
    ///    - Field 10: Game status (possible values: 'B': pregame, 'A': in progress, 'H': halftime, 'E': end regulation, 'F': final)
    ///    - Field 11: Period ('1': 1st half, '2': 2nd half, '3': 1st overtime, '4': 2nd overtime, etc.
    ///
    /// </summary>
    /// <returns><c>true</c>, if scoreboard data was updated, <c>false</c> otherwise.</returns>
    /// <param name="scoreFields">Score fields.</param>
    public override bool UpdateScoreboardData(string[] scoreFields)
    {

        // Validate the input data
        if ((scoreFields[1] == "1" && scoreFields.Length < 12) ||
            (scoreFields[1] != "1" && scoreFields.Length < 14))
        {
            Debug.LogError("BasketballScoreboardDataModel::UpdateScoreboardData(): The input parameter did not contain enough information.");
            return false;
        }

        base.UpdateScoreboardData(scoreFields);

        string strVal;
        int nVal;
        int index = 1;

        // Check the data provider
        SportsDataProviderType dataProvider = SportsDataProviderType.None;
        if (scoreFields[index] == "1")
        {
            dataProvider = SportsDataProviderType.SMT;
        }
        else
        {
            dataProvider = SportsDataProviderType.SportRadar;
        }

        // Parse the data
        if (dataProvider == SportsDataProviderType.SMT)
        {
            // Game Type
            if (_gameType != "BKB")
            {
                _gameType = "BKB";
                OnScoreboardDataChanged("GameType", _gameType);
            }

            // League ID
            if (_leagueType != "NCAA")
            {
                _leagueType = "NCAA";
                OnScoreboardDataChanged("LeagueTuype", _leagueType);
            }

            // Match ID
            _matchId = "0";
        }
        else
        {
            // Version
            if (scoreFields[index].Contains(".") || scoreFields[1].StartsWith("V"))
            {
                _version = scoreFields[index++];
            }

            // Game Type
            strVal = scoreFields[index++];
            if (_gameType != strVal)
            {
                _gameType = strVal;
                OnScoreboardDataChanged("GameType", _gameType);
            }

            // League ID
            strVal = scoreFields[index++];
            if (_leagueType != strVal)
            {
                _leagueType = strVal;
                OnScoreboardDataChanged("LeagueTuype", _leagueType);
            }

            // Match ID
            strVal = scoreFields[index++];
            if (_matchId != strVal)
            {
                _matchId = strVal;
                OnScoreboardDataChanged("MatchID", _matchId);
            }
        }

        // Away Team ID
        strVal = scoreFields[index++];
        if (AwayTeamID != strVal)
        {
            AwayTeamID = strVal;
            OnScoreboardDataChanged("AwayTeamID", AwayTeamID);

            AwayTeamName = getTeamNameById(AwayTeamID);
            OnScoreboardDataChanged("AwayTeamName", AwayTeamName);

            AwayTeamNickname = getTeamNicknameById(AwayTeamID);
            OnScoreboardDataChanged("AwayTeamNickname", AwayTeamNickname);

			//get our logos
			string awayTeamLogo = getTeamAwayLogoById(AwayTeamID);
			if (AwayTeamLogoUrl != awayTeamLogo)
			{
				AwayTeamLogoUrl = awayTeamLogo;
				OnScoreboardDataChanged("AwayTeamLogoUrl", AwayTeamLogoUrl);
			}
       }

        // Home Team ID
        strVal = scoreFields[index++];
        if (HomeTeamID != strVal)
        {
            HomeTeamID = strVal;
            OnScoreboardDataChanged("HomeTeamID", HomeTeamID);

            HomeTeamName = getTeamNameById(HomeTeamID);
            OnScoreboardDataChanged("HomeTeamName", HomeTeamName);

            HomeTeamNickname = getTeamNicknameById(HomeTeamID);
            OnScoreboardDataChanged("HomeTeamNickname", HomeTeamNickname);

			//get our logos
			string homeTeamLogo = getTeamHomeLogoById(HomeTeamID);
			if (HomeTeamLogoUrl != homeTeamLogo)
			{
				HomeTeamLogoUrl = homeTeamLogo;
				OnScoreboardDataChanged("HomeTeamLogoUrl", HomeTeamLogoUrl);
			}
        }

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
        if (AwayTeamTimeouts != nVal && nVal >= 0 && nVal <= MAX_TIMEOUTS_OF_BASKETBALL)
        {
            AwayTeamTimeouts = nVal;
            OnScoreboardDataChanged("AwayTeamTimeouts", AwayTeamTimeouts.ToString());
        }
        int.TryParse(scoreFields[index++], out nVal);
        if (HomeTeamTimeouts != nVal && nVal >= 0 && nVal <= MAX_TIMEOUTS_OF_BASKETBALL)
        {
            HomeTeamTimeouts = nVal;
            OnScoreboardDataChanged("HomeTeamTimeouts", HomeTeamTimeouts.ToString());
        }

        // Fouls
        int.TryParse(scoreFields[index++], out nVal);
        if (AwayTeamFouls != nVal)
        {
            AwayTeamFouls = nVal;
            OnScoreboardDataChanged("AwayTeamFouls", AwayTeamFouls.ToString());
        }
        int.TryParse(scoreFields[index++], out nVal);
        if (HomeTeamFouls != nVal)
        {
            HomeTeamFouls = nVal;
            OnScoreboardDataChanged("HomeTeamFouls", HomeTeamFouls.ToString());
        }



        if (dataProvider == SportsDataProviderType.SMT)
        {
            // Game Status
            strVal = scoreFields[index++];
            if (GameProgressStatus != strVal)
            {
                GameProgressStatus = strVal;
                OnScoreboardDataChanged("GameProgressStatus", GameProgressStatus);
            }

            // Period
            int.TryParse(scoreFields[index++], out nVal);
            if (Period != nVal)
            {
                Period = nVal;
                OnScoreboardDataChanged("Period", Period.ToString());
            }
        }
        else
        {
			string scoreStr = scoreFields[index++];
			bool isInt = int.TryParse(scoreStr, out nVal);

			if(isInt)
			{
				if (Period != nVal)
				{
					PeriodStr = "";
					Period = nVal;
					OnScoreboardDataChanged("Period", Period.ToString());
				}
			}else{

				if (PeriodStr != scoreStr)
				{
					Period = 0;
					PeriodStr = scoreStr;
					string periodVal = scoreStr;
					if (periodVal == "HT")
						periodVal = "Half";
					
					OnScoreboardDataChanged("Period", periodVal);
				}
			}

            // Possession - does not exist in latest API
//            bVal = (scoreFields[index++] == HomeTeamID);
//            if (IsOffendedByHome != bVal)
//            {
//                IsOffendedByHome = bVal;
//                OnScoreboardDataChanged("IsOffendedByHome", IsOffendedByHome.ToString());
//            }

            // UTC Time only if exists because the old version does not have it
			//Debug.Log ("scoreFields.Length: " + scoreFields.Length + ", index: " + index);
            if (scoreFields.Length > index)
            {
                _utcTime = scoreFields[index++];
            }
         }


		string scoreString = "";
		for (int i = 0; i < scoreFields.Length; i++)
		{
			scoreString += scoreFields [i] + "/";
		}

		//EventManager.Instance.LogStringEvent (scoreString);

//		Debug.Log ("scoreString: " + scoreString);
//		Debug.Log ("_gameType: " + _gameType);
//		Debug.Log ("_leagueType: " + _leagueType);
//		Debug.Log ("_matchId: " + _matchId);
//		Debug.Log ("AwayTeamID: " + AwayTeamID);
//		Debug.Log ("AwayTeamName: " + AwayTeamName);
//		Debug.Log ("AwayTeamNickname: " + AwayTeamNickname);
//		Debug.Log ("AwayTeamLogoUrl: " + AwayTeamLogoUrl);
//		Debug.Log ("HomeTeamID: " + HomeTeamID);
//		Debug.Log ("HomeTeamName: " + HomeTeamName);
//		Debug.Log ("HomeTeamNickname: " + HomeTeamNickname);
//		Debug.Log ("HomeTeamLogoUrl: " + HomeTeamLogoUrl);
//		Debug.Log ("AwayTeamScore: " + AwayTeamScore);
//		Debug.Log ("HomeTeamScore: " + HomeTeamScore);
//		Debug.Log ("AwayTeamTimeouts: " + AwayTeamTimeouts);
//		Debug.Log ("HomeTeamTimeouts: " + HomeTeamTimeouts);
//		Debug.Log ("AwayTeamFouls: " + AwayTeamFouls);
//		Debug.Log ("HomeTeamFouls: " + HomeTeamFouls);
//		Debug.Log ("Period: " + Period);
//		Debug.Log ("IsOffendedByHome: " + IsOffendedByHome);
//		Debug.Log ("_utcTime: " + _utcTime);

        return (_isDataModelUpdated = true);
    }
}
