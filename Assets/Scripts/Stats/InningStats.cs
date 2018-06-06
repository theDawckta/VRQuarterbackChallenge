using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class InningStats : MonoBehaviour 
{
	public TextMeshProUGUI GameStatusTxt;
	public TextMeshProUGUI AwayTeamNameTxt;
	public TextMeshProUGUI HomeTeamNameTxt;
	public GameObject InningInfo;
	public GameObject InningInfoExt;
	public GameObject GameInfo;
	[Header("Extra Innings")]
	public GameObject InningHolder;
	public ScoreboardInning ScoreboardInningPrefab;
    public GameObject CenterFrame;
    public Vector3 extraInningPosition;

	[HideInInspector]
	public List<InningColumnData> InningColumnDataList;
	[HideInInspector]
	public List<InningColumnData> InningExtColumnDataList;
	[HideInInspector]
	public List<InningColumnData> GameColumnDataList;

	//there are 4 extra columns of data inside scorecolumns currently
	private int extraColumns = 4;
	private int totalInnings = 9;

	void Awake()
	{
		InningHolder.SetActive (false);
		InningInfoExt.SetActive(false);
	}

	public void StartMakeInningStats(GameStatistics team)
	{
        InningColumnDataList = new List<InningColumnData>();
		InningExtColumnDataList = new List<InningColumnData>();
		GameColumnDataList = new List<InningColumnData>();

		foreach(Transform gameInfoChildren in InningInfo.transform)
		{
            if (gameInfoChildren.gameObject != InningHolder)
			{
				TextMeshProUGUI[] txtArray = gameInfoChildren.GetComponentsInChildren<TextMeshProUGUI> ();

				if (txtArray.Length > 0)
				{
					InningColumnDataList.Add (new InningColumnData (txtArray [0], txtArray [1], txtArray [2]));
				}
			}
		}
//		foreach(Transform gameInfoChildren in InningInfoExt.transform)
//		{
//			Text[] txtArray = gameInfoChildren.GetComponentsInChildren<Text>();
//			InningExtColumnDataList.Add(new InningColumnData(txtArray[0], txtArray[1], txtArray[2]));
//		}
		foreach(Transform gameInfoChildren in GameInfo.transform)
		{
			TextMeshProUGUI[] txtArray = gameInfoChildren.GetComponentsInChildren<TextMeshProUGUI>();
			GameColumnDataList.Add(new InningColumnData(txtArray[0], txtArray[1], txtArray[2]));
		}
		StartCoroutine(MakeInningStats(team));
	}

	IEnumerator MakeInningStats(GameStatistics teamData) 
	{

		List<ColumnData> scoreColumns = ParseTeamStats(teamData);

		if (teamData.Status.ToString ().ToLower () == "final")
		{
			GameStatusTxt.text = "Final";
		} else
		{
			GameStatusTxt.text = "";
		}

		if(scoreColumns.Count > 0)
		{
			AwayTeamNameTxt.text = scoreColumns[0].Row1;
			HomeTeamNameTxt.text = scoreColumns[0].Row2;
		}

		CreateExtraInnings (scoreColumns);

		int extraInningCount = 0;
		for (int i = 1; i < scoreColumns.Count; i++)
		{
			if(scoreColumns[i].Headline.Contains("R"))
			{
				GameColumnDataList[0].InningNumber.text = scoreColumns[i].Headline;
				GameColumnDataList[0].AwayScoreTxt.text = scoreColumns[i].Row1;
				GameColumnDataList[0].HomeScoreTxt.text = scoreColumns[i].Row2;
			}
			else if(scoreColumns[i].Headline.Contains("H"))
			{
				GameColumnDataList[1].InningNumber.text = scoreColumns[i].Headline;
				GameColumnDataList[1].AwayScoreTxt.text = scoreColumns[i].Row1;
				GameColumnDataList[1].HomeScoreTxt.text = scoreColumns[i].Row2;
			}
			else if(scoreColumns[i].Headline.Contains("E"))
			{
				GameColumnDataList[2].InningNumber.text = scoreColumns[i].Headline;
				GameColumnDataList[2].AwayScoreTxt.text = scoreColumns[i].Row1;
				GameColumnDataList[2].HomeScoreTxt.text = scoreColumns[i].Row2;
			}
			else
			{
				if(i < 10)
				{
					InningColumnDataList[i - 1].InningNumber.text = i.ToString();
					if(!string.IsNullOrEmpty(scoreColumns[i].Row1))
					{
						InningColumnDataList[i - 1].AwayScoreTxt.text = scoreColumns[i].Row1;
					}
					if(!string.IsNullOrEmpty(scoreColumns[i].Row2))
					{
						InningColumnDataList[i - 1].HomeScoreTxt.text = scoreColumns[i].Row2;
					}
//					InningInfoExt.SetActive(false);
				}
				else
				{
					
					//there are 4 extra columns of data; any over that should belong to new innings
					string inningNum = i.ToString();
					string awayScore = "-";
					string homeScore = "-";

					awayScore = scoreColumns[i].Row1;
					homeScore = scoreColumns [i].Row2;

					UpdateExtraInning (extraInningCount, inningNum, awayScore, homeScore);
					extraInningCount++;

//                    if (!InningInfoExt.activeSelf)
//                    {
//                        InningInfoExt.SetActive(true);
//                        CenterFrameAnchor.transform.localPosition = extraInningPosition;
//                        if (GlobalVars.Instance.IsUISnappedOut)
//                            CenterFrame.transform.localPosition = extraInningPosition;
//                    }
//					InningExtColumnDataList[i - 10].InningNumber.text = i.ToString();
//					if(!string.IsNullOrEmpty(scoreColumns[i].Row1))
//					{
//						InningExtColumnDataList[i - 10].AwayScoreTxt.text = scoreColumns[i].Row1;
//					}
//					if(!string.IsNullOrEmpty(scoreColumns[i].Row2))
//					{
//						InningExtColumnDataList[i - 10].HomeScoreTxt.text = scoreColumns[i].Row2;
//					}
				}
			}
		}

		yield return null;
	}

	private bool _creatingInnings = false;

	/// <summary>
	/// Create extra innings if need be - hide extra innings if this should not exist
	/// </summary>
	/// <param name="scoreColumns">Score columns.</param>
	void CreateExtraInnings(List<ColumnData> scoreColumns)
	{
		//create extra innings if need be
		if (scoreColumns.Count > totalInnings + extraColumns)
		{
			InningHolder.SetActive (true);
			//how many extra items should we create
			int newInnings = scoreColumns.Count - (totalInnings + extraColumns);
			//check to see if we've already been created
			if (InningHolder.transform.childCount < newInnings && !_creatingInnings)
			{
				_creatingInnings = true;
				int totalNewInnings = newInnings - InningHolder.transform.childCount;
				for (int j = 0; j < totalNewInnings; j++)
				{
					ScoreboardInning scoreboardInning = Instantiate (ScoreboardInningPrefab) as ScoreboardInning;
					scoreboardInning.gameObject.transform.SetParent (InningHolder.transform, false);
				}
				_creatingInnings = false;
			}
		} else
		{
			InningHolder.SetActive (false);
		}
	}

	void UpdateExtraInning(int extraInningCount, string inningNum, string awayScore, string homeScore)
	{
		//make sure we have children to accomodate the innings
		if (InningHolder.transform.childCount > extraInningCount)
		{
			GameObject extraInningGO = InningHolder.transform.GetChild (extraInningCount).gameObject;
			if (extraInningGO != null)
			{
				ScoreboardInning extraInning = extraInningGO.GetComponent<ScoreboardInning> ();
				if (extraInning != null)
				{
					extraInning.InningTxt.text = inningNum;
					extraInning.AwayScoreTxt.text = awayScore;
					extraInning.HomeScoreTxt.text = homeScore;
				}
			}
		}
	}

	public class InningColumnData
	{
		public TextMeshProUGUI InningNumber;
		public TextMeshProUGUI AwayScoreTxt;
		public TextMeshProUGUI HomeScoreTxt;
		
		public InningColumnData(TextMeshProUGUI inningNum, TextMeshProUGUI awayScore, TextMeshProUGUI homeScore)
		{
			InningNumber = inningNum;
			AwayScoreTxt = awayScore;
			HomeScoreTxt = homeScore;
		}
	}

	private class TeamStatsData
	{
		public string TeamAwayName { get; set; }
		public string TeamHomeName { get; set; }
//		public string Team1ImageUrl { get; set; }
//		public string Team2ImageUrl { get; set; }
//		public IList<TeamStatsStatLine> StatLines { get; private set; }
//
//		public TeamStatsData()
//		{
//			StatLines = new List<TeamStatsStatLine>();
//		}
	}

	private class ColumnData
	{
		public string Headline { get; set;}
		public string Row1 { get; set;}
		public string Row2 { get; set;}
	}

	private List<ColumnData> ParseTeamStats(GameStatistics game)
	{
        BaseballGameStatistics baseballGameStats = (BaseballGameStatistics)game;
		List<ColumnData> scoreColumns = new List<ColumnData>();
		var teams = baseballGameStats.Teams;
		if (baseballGameStats.Teams.Count != 2)
		{
			Debug.LogErrorFormat("Must have at least 2 teams, found {0}.", teams.Count);
			return scoreColumns;
		}
		BaseballTeamStatistics homeTeam;
        BaseballTeamStatistics awayTeam;
		if(teams[0].Location.Contains("home"))
		{
			homeTeam = teams[0];
			awayTeam = teams[1];
		}
		else
		{
			homeTeam = teams[1];
			awayTeam = teams[0];
		}

		ColumnData colTeams = new ColumnData ();
		colTeams.Headline = "";
		colTeams.Row1 = awayTeam.Abbreviation.ToUpper();
		colTeams.Row2 = homeTeam.Abbreviation.ToUpper();

		scoreColumns.Add (colTeams);

		//need to add innings data to list
		if (awayTeam.Innings != null)
		{
			for (int i = 0; i < awayTeam.Innings.Count; i++)
			{
				ColumnData colInning = new ColumnData ();
				colInning.Headline = awayTeam.Innings [i].Inning;
				if(awayTeam.Innings [i] != null)
					colInning.Row1 = awayTeam.Innings [i].Score;
				if(homeTeam.Innings.Count > i)
					colInning.Row2 = homeTeam.Innings [i].Score;
				else
					colInning.Row2 = "0";
				scoreColumns.Add (colInning);
			}
		}

		//add runs/hits/errors
		if (awayTeam.Runs != null)
		{
			ColumnData colRuns = new ColumnData ();
			colRuns.Headline = "R";
			colRuns.Row1 = awayTeam.Runs;
			colRuns.Row2 = homeTeam.Runs;
			scoreColumns.Add (colRuns);
		}

		if (awayTeam.Hits != null)
		{
			ColumnData colHits = new ColumnData ();
			colHits.Headline = "H";
			colHits.Row1 = awayTeam.Hits;
			colHits.Row2 = homeTeam.Hits;
			scoreColumns.Add (colHits);
		}

		if (awayTeam.Errors != null)
		{
			ColumnData colErrors = new ColumnData ();
			colErrors.Headline = "E";
			colErrors.Row1 = awayTeam.Errors;
			colErrors.Row2 = homeTeam.Errors;
			scoreColumns.Add (colErrors);
		}

		return scoreColumns;
	}
}
