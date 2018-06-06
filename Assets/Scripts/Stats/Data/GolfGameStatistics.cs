using System.Collections.Generic;
using UnityEngine;

public class GolfGameStatistics : GameStatistics
{
    public GolfGameStatistics()
    {
        Type = GameType.Golf;
        PGAPlayers = new List<GolfPlayerStatistics>();
    }

    public IList<GolfPlayerStatistics> PGAPlayers { get; private set; }

    private const int MAX_PLAYERS_TO_DISPLAY = 10;

    public override GameStatistics Parse(string json, string statType = "")
    {
        //Debug.Log("****************************** Golf GameStatistics Parse method ****************************************");

        JSONObject obj = new JSONObject(json);
        GolfGameStatistics game = new GolfGameStatistics();

        var playerObj = obj.GetField("players");

        if (playerObj != null)
        {
            int numOfplayers = (playerObj.keys.Count > MAX_PLAYERS_TO_DISPLAY) ? MAX_PLAYERS_TO_DISPLAY : playerObj.keys.Count;
            //Debug.Log("PLAYER NAME : " + numOfplayers);
            for (int i = 0; i < numOfplayers; i++)
            {
                string playerName = playerObj.keys[i];
                //Debug.Log("PLAYER NAME : " + playerName);
                GolfPlayerStatistics pgaPlayer = BuildPGAPlayer(playerObj[playerName], GolfPlayerStatistics.StatisticDefinitions);
                game.PGAPlayers.Add(pgaPlayer);
            }
        }

        //Debug.Log("Players count: " + game.PGAPlayers.Count);

        return game;
    }

    private static GolfPlayerStatistics BuildPGAPlayer(JSONObject playerData, StatisticDefinitionCollection definitions)
    {
        if (playerData == null)
            Debug.LogError("null data found!");

        string playerName = "";
        string playerCountry = "";
        string playerTotal = "";
        string playerPosition = "";
        string playerRound = "";
        string playerThru = "";
        string playerID = "";

        if (playerData.HasField("Name"))
            playerName = playerData.GetField("Name").str;

        if (playerData.HasField("Country"))
            playerCountry = playerData.GetField("Country").str;

        if (playerData.HasField("TournParRel"))
            playerTotal = playerData.GetField("TournParRel").str;

        if (playerData.HasField("CurPos"))
            playerPosition = playerData.GetField("CurPos").str;

        if (playerData.HasField("CurParRel"))
            playerRound = playerData.GetField("CurParRel").str;

        if (playerData.HasField("Thru"))
            playerThru = playerData.GetField("Thru").str;

        if (playerData.HasField("PlayerID"))
            playerID = playerData.GetField("PlayerID").str;

        var player = new GolfPlayerStatistics
        {
            Position = playerPosition,
            Name = playerName.ToUpper(),
            Country = playerCountry,
            Total = playerTotal,
            Round = playerRound,
            Thru = playerThru,
            PlayerID = playerID
        };

        return player;
    }
}
