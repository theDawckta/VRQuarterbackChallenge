using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class BaseballGameStatistics : GameStatistics {

    public IList<BaseballTeamStatistics> Teams { get; set; }

    public BaseballGameStatistics()
    {
        Type = GameType.Baseball;
        Teams = new Collection<BaseballTeamStatistics>();
    }

    public override GameStatistics Parse(string json, string statType = "")
    {
        //Debug.Log("****************************** Baseball GameStatistics Parse method ****************************************");

        var obj = new JSONObject(json);
        //replace hypen for valid enum naming
        string gameStatusStr = obj.GetField("status").str.Replace("-", "");

        var game = new BaseballGameStatistics
        {
            ID = obj.GetField("id").str,
            Name = obj.GetField("name").str,
            ApiID = obj.GetField("game_api_id").str,
            Scheduled = ParseDate(obj.GetField("scheduled_datetime").str),
            Status = Extensions.EnumParseOrNull<GameStatus>(gameStatusStr) ?? GameStatus.Unknown
        };

        var teamObj = obj.GetField("teams");

        if (teamObj != null)
        {
            foreach (var teamName in teamObj.keys)
            {
                var teamData = teamObj[teamName];
                BaseballTeamStatistics team = BuildTeam(teamName, teamData, BaseballTeamStatistics.StatisticDefinitions, statType);
                game.Teams.Add(team);

                if (teamData.HasField("hitters") && teamData.HasField("pitchers"))
                {
                    var playerObj = teamData.GetField("hitters").GetField("players");
                    foreach (var playerName in playerObj.keys)
                    {
                        BaseballPlayerStatistics hitter = BuildHitter(playerName, playerObj[playerName], BaseballPlayerStatistics.StatisticDefinitions);
                        team.Players.Add(hitter);
                    }

                    //pitchers
                    playerObj = teamData.GetField("pitchers").GetField("players");
                    foreach (var playerName in playerObj.keys)
                    {
                        BaseballPlayerStatistics pitcher = BuildPitcher(playerName, playerObj[playerName], BaseballPlayerStatistics.StatisticDefinitions);
                        team.Pitchers.Add(pitcher);
                    }
                }
            }
        }

        return game;
    }

    /// <summary>
    /// Builds the hitter.
    /// </summary>
    /// <returns>The hitter.</returns>
    /// <param name="playerName">Player name.</param>
    /// <param name="playerData">Player data.</param>
    /// <param name="definitions">Definitions.</param>
    private BaseballPlayerStatistics BuildHitter(string playerName, JSONObject playerData, StatisticDefinitionCollection definitions)
    {
        string playerPosition = "";
        try
        {
            playerPosition = "  <size=16><color=#0071c5>" + playerData.GetField("position_played").str + "</color></size>";
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("Error parsing player position data '{0}'", ex.ToString());
        }

        //		bool didPlay = false;
        //		if(int.Parse(playerData.GetField("atbats").str) > 0)
        //		{
        //			didPlay = true;
        //		}

        var player = new BaseballPlayerStatistics
        {
            Name = playerName + playerPosition,
            Played = true
        };

        AddGenericStats(player, playerData, definitions);
        return player;
    }

    private BaseballPlayerStatistics BuildPitcher(string playerName, JSONObject playerData, StatisticDefinitionCollection definitions)
    {

        //		bool didPlay = false;
        //		if(float.Parse(playerData.GetField("innings_pitched").str) > 0)
        //		{
        //			didPlay = true;
        //		}

        var player = new BaseballPlayerStatistics
        {
            Name = playerName,
            Played = true
        };

        AddGenericStats(player, playerData, definitions);
        return player;
    }

    protected BaseballTeamStatistics BuildTeam(string teamName, JSONObject teamData, StatisticDefinitionCollection definitions, string statType = "")
    {
        string logoURL = "";
        //baseball specific per team
        string gameRuns = "";
        string gameHits = "";
        string gameErrors = "";
        string teamLocation = "";
        string abbreviation = "";
        List<InningInfo> inningsList = new List<InningInfo>();

        try
        {
            logoURL = teamData.GetField("logo_url").str;
            logoURL = logoURL.Replace("\\", "");
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("Error parsing logo_url data '{0}'", ex.ToString());
        }

        if (statType == "Baseball")
        {
            try
            {
                teamLocation = teamData.GetField("team_location_type").str;
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Error parsing team_location_type data '{0}'", ex.ToString());
            }
            try
            {
                abbreviation = teamData.GetField("abbreviation").str;
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Error parsing abbreviation data '{0}'", ex.ToString());
            }
            try
            {
                gameRuns = teamData.GetField("game_runs").str;
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Error parsing game_runs data '{0}'", ex.ToString());
            }
            try
            {
                gameHits = teamData.GetField("game_hits").str;
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Error parsing game_hits data '{0}'", ex.ToString());
            }
            try
            {
                gameErrors = teamData.GetField("game_errors").str;
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Error parsing game_errors data '{0}'", ex.ToString());
            }

            JSONObject lineScoreArray = teamData.GetField("linescore");

            if (lineScoreArray != null)
            {
                //the data currently reports back two different ways; one way for the first two innings, a different way for all following
                //this should catch those errors, though the fix should most likely come on the data side
                if (lineScoreArray.GetField("linescore") != null)
                {
                    lineScoreArray = lineScoreArray.GetField("linescore");
                }

                if (lineScoreArray != null)
                {
                    //if "inning" is at the root, we are not formatted properly as an array
                    //this should be fixed on the data end, but we will catch here as well
                    if (lineScoreArray.GetField("inning"))
                    {
                        JSONObject tempObject = new JSONObject(JSONObject.Type.ARRAY);
                        tempObject.Add(lineScoreArray);
                        lineScoreArray = new JSONObject(JSONObject.Type.ARRAY);
                        lineScoreArray = tempObject;
                    }

                    for (int i = 0; i < lineScoreArray.Count; i++)
                    {

                        InningInfo curInningInfo = new InningInfo();
                        if (lineScoreArray.keys != null && lineScoreArray.keys[0].Contains("inning"))
                            curInningInfo.Inning = lineScoreArray.list[0].str;
                        else
                        {
                            if (lineScoreArray.list[i] != null && lineScoreArray.list[i].keys[0].Contains("inning"))
                                curInningInfo.Inning = lineScoreArray.list[i].list[0].str;
                        }
                        if (lineScoreArray.keys != null && lineScoreArray.keys[1].Contains("score"))
                            curInningInfo.Score = lineScoreArray.list[1].str;
                        else
                        {
                            if (lineScoreArray.list[i] != null && lineScoreArray.list[i].keys[1].Contains("score"))
                                curInningInfo.Score = lineScoreArray.list[i].list[1].str;
                        }
                        inningsList.Add(curInningInfo);

                    }
                }
            }
        }

        var team = new BaseballTeamStatistics
        {
            Name = teamName,
            Abbreviation = abbreviation,
            Location = teamLocation,
            LogoURL = logoURL,
            Innings = inningsList,
            Runs = gameRuns,
            Hits = gameHits,
            Errors = gameErrors
        };

        AddGenericStats(team, teamData, definitions);
        return team;
    }

}
