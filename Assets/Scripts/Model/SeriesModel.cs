using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class RoundModel 
{
	public int westernRound;
	public int easternRound;
	public List<Round> series;
}

[Serializable]
public class Round 
{
	public string title;
	public int roundNum;
	public string confName;
	public string backgroundImg;
	public List<Team> teams;
}


[Serializable]
public class Team
{
	public string team1City;
	public string team1Name;
	public int team1Rank;
	public string team2City;
	public string team2Name;
	public int team2Rank;
	public string seriesSummary;
	public string column1;
	public string column2;
}