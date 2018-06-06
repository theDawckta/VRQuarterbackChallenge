using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class NBAPlayoffRoundController : MonoBehaviour {


	[Serializable]
	public struct SeriesTarget
	{
		public GameObject SeriesHolder;
		public NBAPlayoffSeriesController SeriesController;
	}

	public SeriesTarget[] PlayoffSeries;
	public TextMeshProUGUI TitleTxt;
	//how much do we need to shift our text over if we are the east
	public float ShiftEastTxt = 5.0f;

	private NBAPlayoffSeriesController _seriesController;


	public void SetContent(Round curRound, bool isEast = false)
	{
		if (curRound != null)
		{
			if (TitleTxt != null)
			{
				if (curRound.title != null)
					TitleTxt.text = curRound.title;
			}

			for (int i = 0; i < curRound.teams.Count; i++)
			{
				if (PlayoffSeries.Length > i)
				{
					if (PlayoffSeries [i].SeriesHolder != null && PlayoffSeries [i].SeriesController != null)
					{
						Team curTeam = curRound.teams [i];
						GameObject seriesHolder = PlayoffSeries [i].SeriesHolder;
						NBAPlayoffSeriesController seriesController = PlayoffSeries [i].SeriesController;
						_seriesController = Instantiate (seriesController, seriesHolder.transform) as NBAPlayoffSeriesController;

						string team1Name = curTeam.team1Name;
						string team2Name = curTeam.team2Name;

						if (_seriesController.Team1CityTxt == null)
							team1Name = curTeam.team1City + " " + curTeam.team1Name;
						else
							SetText (_seriesController.Team1CityTxt, curTeam.team1City);

						if (_seriesController.Team2CityTxt == null)
							team2Name = curTeam.team2City + " " + curTeam.team2Name;
						else
							SetText (_seriesController.Team2CityTxt, curTeam.team2City);

						//east needs shifted slightly to the right to accomodate for design/icons now moving left
						//design is different for the last round, only shift for the first three rounds
						if (isEast && curRound.roundNum < 4)
						{
							Vector3 localPos = PlayoffSeries [i].SeriesHolder.transform.localPosition;
							localPos.x += ShiftEastTxt;
							PlayoffSeries [i].SeriesHolder.transform.localPosition = localPos;


							if (_seriesController.SeriesSummaryTxt != null)
							{
								GameObject seriesSummaryGameObject = _seriesController.SeriesSummaryTxt.gameObject;
								if (seriesSummaryGameObject != null)
								{
									Vector3 seriesSummaryPos = seriesSummaryGameObject.transform.localPosition;
									seriesSummaryPos.x -= ShiftEastTxt / 2;
									seriesSummaryGameObject.transform.localPosition = seriesSummaryPos;
								}
							}
						}

						SetText (_seriesController.Team1NameTxt, team1Name);
						SetText (_seriesController.Team1RankTxt, curTeam.team1Rank.ToString());
						SetText (_seriesController.Team2NameTxt, team2Name);
						SetText (_seriesController.Team2RankTxt, curTeam.team2Rank.ToString());
						SetText (_seriesController.SeriesSummaryTxt, curTeam.seriesSummary);
						SetText (_seriesController.Column1Txt, curTeam.column1);
						SetText (_seriesController.Column2Txt, curTeam.column2);
					}
				}
			}
		}
	}

	void SetText(TextMeshProUGUI textItem, string textStr)
	{
		if (textItem != null)
		{
			if (textStr != null)
                textItem.text = textStr.ToUpper();
		}
	}

}
