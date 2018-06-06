using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class BasketballScoreboardView : BallGameScoreboardView {

    private int mMaxNumOfTimeouts = 0;
	private int mMaxNumFoulsBeforeBonus = 5;

    public GameObject BasketballDataContainer;

    private TextMeshProUGUI HomeTeamFoulsText;
    private TextMeshProUGUI AwayTeamFoulsText;
    private TextMeshProUGUI HomeTeamBonusText;
    private TextMeshProUGUI AwayTeamBonusText;

    public GameObject AwayTeamOffenseIndicator;      // JK DEBUG, Can we change the type to DynamicTexture and the name;
    public GameObject HomeTeamOffenseIndicator;

	public List<GameObject> HomeTeamTimeouts = new List<GameObject>();
	public List<GameObject> AwayTeamTimeouts = new List<GameObject>();

    private string mLeagueType;

    public BasketballScoreboardView() : base()
    {
        mMaxNumOfTimeouts = BasketballScoreboardDataModel.MAX_TIMEOUTS_OF_BASKETBALL;
    }

    public override void LoadRequiredComponents(GameObject parentComponent)
    {
        Debug.Log("BasketballScoreboardView::LoadRequiredComponents()");
        try
        {
            base.LoadRequiredComponents(parentComponent);
            BasketballDataContainer = Extensions.GetRequired<Component>(parentComponent, Extensions.GetVariableName(() => BasketballDataContainer)).gameObject;
            //HomeTeamFoulsText = Extensions.GetRequired<TextMeshProUGUI>(parentComponent, Extensions.GetVariableName(() => HomeTeamFoulsText));
            //AwayTeamFoulsText = Extensions.GetRequired<TextMeshProUGUI>(parentComponent, Extensions.GetVariableName(() => AwayTeamFoulsText));
            HomeTeamBonusText = Extensions.GetRequired<TextMeshProUGUI>(BasketballDataContainer, Extensions.GetVariableName(() => HomeTeamBonusText));
            AwayTeamBonusText = Extensions.GetRequired<TextMeshProUGUI>(BasketballDataContainer, Extensions.GetVariableName(() => AwayTeamBonusText));
            AwayTeamOffenseIndicator = Extensions.GetRequired<Component>(BasketballDataContainer, Extensions.GetVariableName(() => AwayTeamOffenseIndicator)).gameObject;
            HomeTeamOffenseIndicator = Extensions.GetRequired<Component>(BasketballDataContainer, Extensions.GetVariableName(() => HomeTeamOffenseIndicator)).gameObject;
            HomeTeamTimeouts.Clear();
            AwayTeamTimeouts.Clear();

            for (int i = 1; i <= mMaxNumOfTimeouts ; i++)
            {
				HomeTeamTimeouts.Add(Extensions.GetRequired<Component>(ScoreboardCanvas, "Timeout" + i + "OnHome").gameObject);
				AwayTeamTimeouts.Add(Extensions.GetRequired<Component>(ScoreboardCanvas, "Timeout" + i + "OnAway").gameObject);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message); // TODO
        }

        ShowScoreboardUI();
    }

    public override void UpdateScoreboardUI(ScoreboardDataModel dataModel)
    {

    }

    public override void ResetScoreboardData()
    {
//        try 
//        {
//            base.UpdateScoreboardUI(dataModel);
//
//            Debug.Log("BasketballScoreboardView:: UpdateScoreboardUI()");
//
//            BasketballScoreboardDataModel bkbDataModel = dataModel as BasketballScoreboardDataModel;
//
//            // Timeouts
//            if (AwayTeamTimeouts.Count == mMaxNumOfTimeouts && HomeTeamTimeouts.Count == mMaxNumOfTimeouts)
//            {
//                for (int i = 0; i < mMaxNumOfTimeouts; i++)
//                {
//                    AwayTeamTimeouts[i].SetActive(i < bkbDataModel.AwayTeamTimeouts);
//                    HomeTeamTimeouts[i].SetActive(i < bkbDataModel.HomeTeamTimeouts);
//                }
//            }
//
//            // Possession
//            if (HomeTeamOffenseIndicator)
//                HomeTeamOffenseIndicator.SetActive(!bkbDataModel.IsOffendedByHome);
//
//            if (AwayTeamOffenseIndicator)
//                AwayTeamOffenseIndicator.SetActive(bkbDataModel.IsOffendedByHome);
//            
//            // Foul/Bonus // TODO: NEED TO CHECK
//            if (AwayTeamBonusText)
//                AwayTeamBonusText.gameObject.SetActive(bkbDataModel.AwayTeamFouls >= 10);
//
//            if  (HomeTeamBonusText)
//                HomeTeamBonusText.gameObject.SetActive(bkbDataModel.HomeTeamFouls >= 10);
//
//            // Period
//            int quarter = bkbDataModel.Period;
//            if (TimeHeaderText != null && TimeSubheaderText != null)
//            {
//                if (quarter <= 0)
//                {
//                    TimeHeaderText.text = "";
//                    TimeSubheaderText.text = "";
//                }
//                else if (quarter <= ((bkbDataModel.LeagueType == "NCAA")? 2 : 4))
//                {
//                    TimeHeaderText.text = quarter.ToString();
//                    TimeSubheaderText.text = (bkbDataModel.LeagueType == "NCAA")? "Half" : "Quarter";
//                }
//                else
//                {
//                    TimeHeaderText.text = "OT";
//                    TimeSubheaderText.text = "";
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError(ex.Message); // TODO
//        }
    }

    public override void UpdateScoreboardData(string propName, string propValue)
    {
        //Debug.LogFormat("BasketballScoreboardView::UpdateScoreboardData(): name={0}, value={1}", propName, propValue);

        try 
        {
            if (propName == "LeagueType")
            {
                mLeagueType = propValue;
            }
            else if (propName == "AwayTeamTimeouts")
            {
                int nVal = int.Parse(propValue);
//                if (AwayTeamTimeouts.Count == mMaxNumOfTimeouts && HomeTeamTimeouts.Count == mMaxNumOfTimeouts)
//                {
//				for (int i = 0; i < AwayTeamTimeouts.Count; i++)
//                    {
//                        AwayTeamTimeouts[i].SetActive(i < nVal);
//                    }
//                }
            }
            else if (propName == "HomeTeamTimeouts")
            {
                int nVal = int.Parse(propValue);
//                if (AwayTeamTimeouts.Count == mMaxNumOfTimeouts && HomeTeamTimeouts.Count == mMaxNumOfTimeouts)
//                {
//				for (int i = 0; i < HomeTeamTimeouts.Count; i++)
//                    {
//                        HomeTeamTimeouts[i].SetActive(i < nVal);
//                    }
//                }
            }
            else if (propName == "IsOffendedByHome")
            {
                bool bVal = bool.Parse(propValue);

                if (HomeTeamOffenseIndicator)
                    HomeTeamOffenseIndicator.SetActive(!bVal);

                if (AwayTeamOffenseIndicator)
                    AwayTeamOffenseIndicator.SetActive(bVal);
            }
            else if (propName == "AwayTeamFouls")
            {
                int nVal = int.Parse(propValue);

				//currently using timeouts for fouls...
				for (int i = 0; i < AwayTeamTimeouts.Count; i++)
				{
					AwayTeamTimeouts[i].SetActive(i < nVal);
				}

                if (AwayTeamBonusText)
				{
					if(nVal >= mMaxNumFoulsBeforeBonus)
					{
						AwayTeamBonusText.gameObject.SetActive(true);
						AwayTeamBonusText.alpha = 1;
					}

				}
            }
            else if (propName == "HomeTeamFouls")
            {
                int nVal = int.Parse(propValue);
				//currently using timeouts for fouls
				for (int i = 0; i < HomeTeamTimeouts.Count; i++)
				{
					HomeTeamTimeouts[i].SetActive(i < nVal);
				}

                if  (HomeTeamBonusText)
				{
					if(nVal >= mMaxNumFoulsBeforeBonus)
					{
                    	HomeTeamBonusText.gameObject.SetActive(true);
						HomeTeamBonusText.alpha = 1;
					}

				}
            }
            else if (propName == "Period")
            {
				int quarter;
				bool isInt = int.TryParse(propValue, out quarter);

				if(!isInt)
				{
					TimeHeaderText.text = propValue;
					TimeSubheaderText.text = "";
				}else{
	                if (TimeHeaderText != null && TimeSubheaderText != null)
	                {
	                    if (quarter <= 0)
	                    {
	                        TimeHeaderText.text = "";
	                        TimeSubheaderText.text = "";
	                    }
	                    else if (quarter <= ((mLeagueType == "NCAA")? 2 : 4))
	                    {
							TimeSubheaderText.text = "";
							string nbaSubTxt = "";
							switch(quarter){
							case 1:
								nbaSubTxt = "st";
								break;
							case 2:
								nbaSubTxt = "nd";
								break;
							case 3:
								nbaSubTxt = "rd";
								break;
							case 4:
								nbaSubTxt = "th";
								break;
							}

							TimeHeaderText.text = quarter.ToString() + nbaSubTxt;

	                    }
	                    else
	                    {
	                        TimeHeaderText.text = "OT";
	                        TimeSubheaderText.text = "";
	                    }
	                }
				}
            }
            else 
            {
                base.UpdateScoreboardData(propName, propValue);
            }
         }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message); // TODO
        }
    }

    public override void ShowScoreboardUI()
    {
        base.ShowScoreboardUI();

        if (BasketballDataContainer)
            BasketballDataContainer.gameObject.SetActive(true);
    }

    public override void HideScoreboardUI()
    {
        base.HideScoreboardUI();

        if (BasketballDataContainer)
            BasketballDataContainer.gameObject.SetActive(false);
   }
}
