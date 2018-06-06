using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class BasketballGameStatistics : GameStatistics {

    public IList<BasketballTeamStatistics> Teams { get; set; }

    public BasketballGameStatistics()
    {
        Type = GameType.Basketball;
        Teams = new Collection<BasketballTeamStatistics>();
    }

    public override GameStatistics Parse(string json, string statType = "")
    {
        //Debug.Log("****************************** Basketball GameStatistics Parse method ****************************************");

        var obj = new JSONObject(json);
        //replace hypen for valid enum naming
        string gameStatusStr = obj.GetField("status").str.Replace("-", "");

        var game = new BasketballGameStatistics
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
                BasketballTeamStatistics team = BuildTeam(teamName, teamData, BasketballTeamStatistics.StatisticDefinitions, statType);
                game.Teams.Add(team);

                //not baseball
                var playerObj = teamData.GetField("players");
                foreach (var playerName in playerObj.keys)
                {
                    BasketballPlayerStatistics player = BuildPlayer(playerName, playerObj[playerName], BasketballPlayerStatistics.StatisticDefinitions);
                    team.Players.Add(player);
                }
            }
        }

        return game;
    }

    protected BasketballPlayerStatistics BuildPlayer(string playerName, JSONObject playerData, StatisticDefinitionCollection definitions)
    {
        string playerPosition = "";
        try
        {
            playerPosition = "  <size=16><color=#0071c5>" + playerData.GetField("position").str + "</color></size>";
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("Error parsing player position data '{0}'", ex.ToString());
        }

        var player = new BasketballPlayerStatistics
        {
            Name = playerName + playerPosition,
            Played = Boolean.Parse(playerData.GetField("played").str),
            Active = Boolean.Parse(playerData.GetField("active").str),
            Starter = Boolean.Parse(playerData.GetField("starter").str),
            MinutesPlayed = ParseTime(playerData.GetField("minutes").str),
            Updated = ParseDate(playerData.GetField("updated_time").str)
        };

        AddGenericStats(player, playerData, definitions);
        return player;
    }

    protected BasketballTeamStatistics BuildTeam(string teamName, JSONObject teamData, StatisticDefinitionCollection definitions, string statType = "")
    {
        string logoURL = "";
        string teamLocation = "";
        string abbreviation = "";

        try
        {
            logoURL = teamData.GetField("logo_url").str;
            logoURL = logoURL.Replace("\\", "");
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("Error parsing logo_url data '{0}'", ex.ToString());
        }

        var team = new BasketballTeamStatistics
        {
            Name = teamName,
            Abbreviation = abbreviation,
            Location = teamLocation,
            LogoURL = logoURL
        };

        AddGenericStats(team, teamData, definitions);
        return team;
    }
}
