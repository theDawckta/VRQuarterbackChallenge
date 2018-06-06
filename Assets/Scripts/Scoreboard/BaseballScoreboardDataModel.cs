using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseballScoreboardDataModel : BallGameScoreboardDataModel 
{
    // Variables for Box Score
    public int AwayTeamWins;
    public int AwayTeamLosses;
    public int HomeTeamWins;
    public int HomeTeamLosses;
    public int BallCount;
    public int StrikeCount;
    public int OutCount;
    public int Inning;
    public int Hits;
    public int Errors;
    public bool PlayerOn1Base;
    public bool PlayerOn2Base;
    public bool PlayerOn3Base;
    public bool IsTopOfInning;

    // Variables for Pitcher/Batter
    public string BatterID;
    public string BatterName;
    public float BatterBattingAverage;
    public int BatterHitsCount;
    public int BatterAtBatsCount;
    public int BatterHomerunCount;
    public int BatterRunsBattedIn;

    public string PitcherID;
    public string PitcherName;
    public float PitcherEarnedRunAverage;
    public int PitcherPitchCount;
    public float PitcherInningsPitched;
     
    public BaseballScoreboardDataModel() : base ()
    {
    }

    /// <summary>
    /// Updates the scoreboard data.
    /// </summary>
    /// <returns><c>true</c>, if scoreboard data was updated, <c>false</c> otherwise.</returns>
    /// <param name="scoreFields">Score fields.</param>
    public override bool UpdateScoreboardData(string[] scoreFields)
    {
        // Validate the input data
        if (scoreFields.Length < 7)
        {
            Debug.LogError("BaseballScoreboardDataModel::UpdateScoreboardData(): The input parameter did not contain enough information.");
            return false;
        }

        base.UpdateScoreboardData(scoreFields);

        bool retVal = false;
        if (scoreFields[5] != "PBD")
        {
            retVal = parseGameScoreboardData(scoreFields);
        }
        else
        {
            retVal = parsePitcherAndBatterData(scoreFields);
        }
 
        return (_isDataModelUpdated = retVal);
    }

    /// <summary>
    /// Parses the game scoreboard data.
    /// Game Data Format: R\V1.0\BSB\MLB\19\TB\Cle\0\0\19\22\20\17\0\0\0\1\0\0\000\T\2017-05-16 22:08:06
    ///    - Field 0:  Response packet type (always “R”)
    ///    - Field 1:  Version nuber
    ///    - Field 2:  Sport identifier (always “BSB” for Baseball)
    ///    - Field 3:  League identifier ( MLB/NPB)
    ///    - Field 4:  Match ID
    ///    - Field 5:  Away team ID
    ///    - Field 6:  Home team ID
    ///    - Field 7:  Away score
    ///    - Field 8:  Home score
    ///    - Field 9:  Away Wins
    ///    - Field 10: Away loss
    ///    - Field 11: Home wins
    ///    - Field 12: Home loss
    ///    - Field 13: Ball
    ///    - Field 14: Strike
    ///    - Field 15: Out
    ///    - Field 16: Inning
    ///    - Field 17: Hits
    ///    - Field 18: Errors
    ///    - Field 19: Bases loaded*
    ///    - Field 20: Inning half
    ///    - Field 21: UTC Timestamp
    /// 
    /// </summary>
    /// <returns><c>true</c>, if game scoreboard data was updated, <c>false</c> otherwise.</returns>
    /// <param name="scoreFields">Score fields.</param>
    private bool parseGameScoreboardData(string[] scoreFields)
    {
        // Validate the input data
        if (scoreFields.Length < 22)
        {
            Debug.LogError("BaseballScoreboardDataModel::UpdateGameScoreboardData(): The input parameter did not contain enough information.");
            return false;
        }

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
            OnScoreboardDataChanged("AwayTeamID", AwayTeamID);

            AwayTeamName = getTeamNameById(AwayTeamID);
            OnScoreboardDataChanged("AwayTeamName", AwayTeamName);

            AwayTeamNickname = getTeamNicknameById(AwayTeamID);
            OnScoreboardDataChanged("AwayTeamNickname", AwayTeamNickname);
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

        // Wins/Losses
        int nVal2;
        int.TryParse(scoreFields[index++], out nVal);
        int.TryParse(scoreFields[index++], out nVal2);
        if (AwayTeamWins != nVal || AwayTeamLosses != nVal2)
        {
            AwayTeamWins = nVal;
            AwayTeamLosses = nVal2;
            OnScoreboardDataChanged("AwayTeamWinsLosses", string.Format("{0}-{1}", AwayTeamWins, AwayTeamLosses));
        }
        int.TryParse(scoreFields[index++], out nVal);
        int.TryParse(scoreFields[index++], out nVal2);
        if (HomeTeamWins != nVal || HomeTeamLosses != nVal2)
        {
            HomeTeamWins = nVal;
            HomeTeamLosses = nVal2;
            OnScoreboardDataChanged("HomeTeamWinsLosses", string.Format("{0}-{1}", HomeTeamWins, HomeTeamLosses));
        }

        // Ball/Strike/Out
        int.TryParse(scoreFields[index++], out nVal);
        if (BallCount != nVal)
        {
            BallCount = nVal;
            OnScoreboardDataChanged("BallCount", BallCount.ToString());
        }
        int.TryParse(scoreFields[index++], out nVal);
        if (StrikeCount != nVal)
        {
            StrikeCount = nVal;
            OnScoreboardDataChanged("StrikeCount", StrikeCount.ToString());
        }
        int.TryParse(scoreFields[index++], out nVal);
        if (OutCount != nVal)
        {
            OutCount = nVal;
            OnScoreboardDataChanged("OutCount", OutCount.ToString());
        }

        // Inning
        int.TryParse(scoreFields[index++], out nVal);
        if (Inning != nVal)
        {
            Inning = nVal;
            OnScoreboardDataChanged("Inning", Inning.ToString());
        }

        // Hits
        int.TryParse(scoreFields[index++], out nVal);
        if (Hits != nVal)
        {
            Hits = nVal;
            OnScoreboardDataChanged("Hits", Hits.ToString());
        }

        // Errors
        int.TryParse(scoreFields[index++], out nVal);
        if (Errors != nVal)
        {
            Errors = nVal;
            OnScoreboardDataChanged("Errors", Errors.ToString());
        }

        // Based Loaded
        strVal = scoreFields[index++];
        bVal = (strVal[2] == '1');
        if (PlayerOn1Base != bVal)
        {
            PlayerOn1Base = bVal;
            OnScoreboardDataChanged("PlayerOn1Base", PlayerOn1Base.ToString());
        }
        bVal = (strVal[1] == '1');
        if (PlayerOn2Base != bVal)
        {
            PlayerOn2Base = bVal;
            OnScoreboardDataChanged("PlayerOn2Base", PlayerOn2Base.ToString());
        }
        bVal = (strVal[0] == '1');
        if (PlayerOn3Base != bVal)
        {
            PlayerOn3Base = bVal;
            OnScoreboardDataChanged("PlayerOn3Base", PlayerOn3Base.ToString());
        }
 
        // Top/Bottom of Inning
        bVal = (scoreFields[index++] == "T");
        if (IsTopOfInning != bVal)
        {
            IsTopOfInning = bVal;
            OnScoreboardDataChanged("IsTopOfInning", IsTopOfInning.ToString());
        }

        // UTC Time
        _utcTime = scoreFields[index++];
 
        return (_isDataModelUpdated = true);
    }
         
    /// <summary>
    /// Parses the pitcher and batter data.
    /// Pitcher/Batter Data Format: R\V1.0\BSB\MLB\42\PBD\508176\W. Contreras\0.243\1-1\5\27\708217\T. Williams\5.36\51\3.0\T\
    ///    - Field 0:  Response packet type (always “R”)
    ///    - Field 1:  Version of scoreboard client
    ///    - Field 2:  Sport identifier(always “BSB” for Baseball)
    ///    - Field 3:  League identifier(MLB/NPB)
    ///    - Field 4:  Match ID
    ///    - Field 5:  Pitcher/Batter tag(Always “PBD”)
    ///    - Field 6:  Batter ID
    ///    - Field 7:  Batter Name
    ///    - Field 8:  Batter Avg
    ///    - Field 9:  Hit count/At bat
    ///    - Field 10: HR(Home Run)
    ///    - Field 11: RBI(Run Batted In)
    ///    - Field 12: Pitcher ID
    ///    - Field 13: Pitcher Name
    ///    - Field 14: ERA(Earned Run Avg)
    ///    - Field 15: Pitch count
    ///    - Field 16: IP(Innings Pitched)
    ///    - Field 17: InningInfo T/B
    /// 
    /// </summary>
    /// <returns><c>true</c>, if pitcher and batter data was updated, <c>false</c> otherwise.</returns>
    /// <param name="scoreFields">Score fields.</param>
    private bool parsePitcherAndBatterData(string[] scoreFields)
    {
        // Validate the input data
        if (scoreFields.Length < 18)
        {
            Debug.LogError("BaseballScoreboardDataModel::UpdatePitcherAndBatterData(): The input parameter did not contain enough information.");
            return false;
        }

        int nVal, nVal2;
        float fVal;
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

        // Reserved for'PBD'
        index++;

        // Batter ID/Name/Average/Hits/HR/RBI
        strVal = scoreFields[index++];
        //if (BatterID != strVal)
        //{
            BatterID = strVal;
            OnScoreboardDataChanged("BatterID", BatterID);
        //}
        strVal = scoreFields[index++];
        if (BatterName != strVal)
        {
            BatterName = strVal;
            OnScoreboardDataChanged("BatterName", BatterName);
        }
        float.TryParse(scoreFields[index++], out fVal);
        if (BatterBattingAverage != fVal)
        {
            BatterBattingAverage = fVal;
            OnScoreboardDataChanged("BatterBattingAverage", BatterBattingAverage.ToString());
        }
        string HitsAndBats = scoreFields[index++];
        int.TryParse(HitsAndBats.Substring(0, HitsAndBats.IndexOf('-')), out nVal);
        int.TryParse(HitsAndBats.Substring(HitsAndBats.IndexOf('-') + 1), out nVal2);
        if (BatterHitsCount != nVal || BatterAtBatsCount != nVal2)
        {
            BatterHitsCount = nVal;
            BatterAtBatsCount = nVal2;
            OnScoreboardDataChanged("BatterHitsAtBatsCount", string.Format("{0}-{1}", BatterHitsCount, BatterAtBatsCount));
        }
        int.TryParse(scoreFields[index++], out nVal);
        if (BatterHomerunCount != nVal)
        {
            BatterHomerunCount = nVal;
            OnScoreboardDataChanged("BatterHomerunCount", BatterHomerunCount.ToString());
        }
        int.TryParse(scoreFields[index++], out nVal);
        if (BatterRunsBattedIn != nVal)
        {
            BatterRunsBattedIn = nVal;
            OnScoreboardDataChanged("BatterRunsBattedIn", BatterRunsBattedIn.ToString());
        }

        // Pitcher ID/Name/ERA/PC/IP
        strVal = scoreFields[index++];
        //if (PitcherID != strVal)
        //{
            PitcherID = strVal;
            OnScoreboardDataChanged("PitcherID", PitcherID);
        //}
        strVal = scoreFields[index++];
        if (PitcherName != strVal)
        {
            PitcherName = strVal;
            OnScoreboardDataChanged("PitcherName", PitcherName);
        }
        float.TryParse(scoreFields[index++], out fVal);
        if (PitcherEarnedRunAverage != fVal)
        {
            PitcherEarnedRunAverage = fVal;
            OnScoreboardDataChanged("PitcherEarnedRunAverage", PitcherEarnedRunAverage.ToString());
        }
        int.TryParse(scoreFields[index++], out nVal);
        if (PitcherPitchCount != nVal)
        {
            PitcherPitchCount = nVal;
            OnScoreboardDataChanged("PitcherPitchCount", PitcherPitchCount.ToString());
        }
        float.TryParse(scoreFields[index++], out fVal);
        if (PitcherInningsPitched != fVal)
        {
            PitcherInningsPitched = fVal;
            OnScoreboardDataChanged("PitcherInningsPitched", PitcherInningsPitched.ToString());
        }

        return (_isDataModelUpdated = true);
    }

}
