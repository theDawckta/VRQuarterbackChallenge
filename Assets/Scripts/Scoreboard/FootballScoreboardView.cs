using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class FootballScoreboardView : BallGameScoreboardView {

    public GameObject FootballDataContainer;
    public GameObject GameStatusHolder;

    public TextMeshProUGUI DownText;
    public TextMeshProUGUI DownNumber;
    public TextMeshProUGUI BallOnText;
    public TextMeshProUGUI BallOnNumber;
    public TextMeshProUGUI ToGoText;
    public TextMeshProUGUI ToGoNumber;

    public GameObject HomeTeamOffenseIndicator;
    public GameObject AwayTeamOffenseIndicator;

    public List<GameObject> HomeTeamTimeouts;
    public List<GameObject> AwayTeamTimeouts;

    private int mMaxNumOfTimeouts = 0;

    public FootballScoreboardView() : base()
    {
        HomeTeamTimeouts = new List<GameObject>();
        AwayTeamTimeouts = new List<GameObject>();
        mMaxNumOfTimeouts = FootballScoreboardDataModel.MAX_TIMEOUTS_OF_FOOTBALL;
    }

    public override void LoadRequiredComponents(GameObject parentComponent)
    {
        Debug.Log("FootballScoreboardView::LoadRequiredComponents()");
        try
        {
            base.LoadRequiredComponents(parentComponent);

            GameStatusHolder = Extensions.GetRequired<Component>(ScoreboardCanvas, Extensions.GetVariableName(() => GameStatusHolder)).gameObject;

            FootballDataContainer = Extensions.GetRequired<Component>(parentComponent, Extensions.GetVariableName(() => FootballDataContainer)).gameObject;

            DownText = Extensions.GetRequired<TextMeshProUGUI>(FootballDataContainer, Extensions.GetVariableName(() => DownText));
            DownNumber = Extensions.GetRequired<TextMeshProUGUI>(FootballDataContainer, Extensions.GetVariableName(() => DownNumber));
            ToGoText = Extensions.GetRequired<TextMeshProUGUI>(FootballDataContainer, Extensions.GetVariableName(() => ToGoText));
            ToGoNumber = Extensions.GetRequired<TextMeshProUGUI>(FootballDataContainer, Extensions.GetVariableName(() => ToGoNumber));
            BallOnText = Extensions.GetRequired<TextMeshProUGUI>(FootballDataContainer, Extensions.GetVariableName(() => BallOnText));
            BallOnNumber = Extensions.GetRequired<TextMeshProUGUI>(FootballDataContainer, Extensions.GetVariableName(() => BallOnNumber));

            HomeTeamOffenseIndicator = Extensions.GetRequired<Component>(FootballDataContainer, Extensions.GetVariableName(() => HomeTeamOffenseIndicator)).gameObject;
            AwayTeamOffenseIndicator = Extensions.GetRequired<Component>(FootballDataContainer, Extensions.GetVariableName(() => AwayTeamOffenseIndicator)).gameObject;
           
            HomeTeamTimeouts.Clear();
            AwayTeamTimeouts.Clear();
            for (int i = 1; i <= mMaxNumOfTimeouts ; i++)
            {
                HomeTeamTimeouts.Add(Extensions.GetRequired<Component>(parentComponent, "scoreboard_timeout_R_geo" + i).gameObject);
                AwayTeamTimeouts.Add(Extensions.GetRequired<Component>(parentComponent, "scoreboard_timeout_L_geo" + i).gameObject);
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
//        try 
//        {
//            base.UpdateScoreboardUI(dataModel);
//
//            Debug.Log("JK DEBUG:: FootballScoreboardView:: UpdateScoreboardUI()");
//
//            FootballScoreboardDataModel ftbDataModel = dataModel as FootballScoreboardDataModel;
//
//            if (DownNumber)
//                DownNumber.text = ftbDataModel.NumberOfDowns.ToString();
//
//            if (ToGoNumber)
//                ToGoNumber.text = ftbDataModel.ToGoInYard.ToString();
//
//            if (BallOnNumber)
//                BallOnNumber.text = ftbDataModel.BallOnInYard.ToString();
//
//            // Possession
//            if (AwayTeamOffenseIndicator)
//                AwayTeamOffenseIndicator.SetActive(!ftbDataModel.IsOffendedByHome);
//            if (HomeTeamOffenseIndicator)
//                HomeTeamOffenseIndicator.SetActive(ftbDataModel.IsOffendedByHome);
// 
//            // Timeouts
//            if (AwayTeamTimeouts.Count == mMaxNumOfTimeouts && HomeTeamTimeouts.Count == mMaxNumOfTimeouts)
//            {
//                for (int i = 0; i < mMaxNumOfTimeouts; i++)
//                {
//                    AwayTeamTimeouts[i].SetActive(i < ftbDataModel.AwayTeamTimeouts);
//                    HomeTeamTimeouts[i].SetActive(i < ftbDataModel.HomeTeamTimeouts);
//                }
//            }
//
//            // Period
//            int quarter = ftbDataModel.Period;
//            if (TimeHeaderText != null && TimeSubheaderText != null)
//            {
//                if (quarter <= 0)
//                {
//                    TimeHeaderText.text = "";
//                    TimeSubheaderText.text = "";
//                }
//                else if (quarter <= 4)
//                {
//                    TimeHeaderText.text = quarter.ToString();
//                    TimeSubheaderText.text = "Quarter";
//                }
//                else
//                {
//                    TimeHeaderText.text = "OT";
//                    TimeSubheaderText.text = "";
//                }
//            }
//
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError(ex.Message); // TODO
//        }
    }

    public override void UpdateScoreboardData(string propName, string propValue)
    {
        Debug.LogFormat("FootballScoreboardView::UpdateScoreboardData(): name={0}, value={1}", propName, propValue);

        try 
        {
            if (propName == "NumberOfDowns")
            {
                if (DownNumber)
                    DownNumber.text = propValue;
            }
            else if (propName =="ToGoInYard")
            {
                if (ToGoNumber)
                    ToGoNumber.text = propValue;
            }
            else if (propName == "BallOnInYard")
            {
                if (BallOnNumber)
                    BallOnNumber.text = propValue;
            }
            else if (propName == "IsOffendedByHome") // Possession
            {
                bool bVal = bool.Parse(propValue);
                if (HomeTeamOffenseIndicator)
                    HomeTeamOffenseIndicator.SetActive(bVal);
                if (AwayTeamOffenseIndicator)
                    AwayTeamOffenseIndicator.SetActive(!bVal);
            }
            else if (propName == "AwayTeamTimeouts") // Timeouts
            {
                int timeouts = int.Parse(propValue);
                if (AwayTeamTimeouts.Count == mMaxNumOfTimeouts)
                {
                    for (int i = 0; i < mMaxNumOfTimeouts; i++)
                    {
                        AwayTeamTimeouts[i].SetActive(i < timeouts);
                    }
                }
            }
            else if (propName == "HomeTeamTimeouts")
            {
                int timeouts = int.Parse(propValue);
                if (AwayTeamTimeouts.Count == mMaxNumOfTimeouts && HomeTeamTimeouts.Count == mMaxNumOfTimeouts)
                {
                    for (int i = 0; i < mMaxNumOfTimeouts; i++)
                    {
                        HomeTeamTimeouts[i].SetActive(i < timeouts);
                    }
                }
            }
            else if (propName == "Period")
            {
                // Period
                int quarter = int.Parse(propValue);
                if (TimeHeaderText != null && TimeSubheaderText != null)
                {
                    if (quarter <= 0)
                    {
                        TimeHeaderText.text = "";
                        TimeSubheaderText.text = "";
                    }
                    else if (quarter <= 4)
                    {
                        TimeHeaderText.text = quarter.ToString();
                        TimeSubheaderText.text = "Quarter";
                    }
                    else
                    {
                        TimeHeaderText.text = "OT";
                        TimeSubheaderText.text = "";
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

    public override void ResetScoreboardData()
    {
    }

    public override void ShowScoreboardUI()
    {
        base.ShowScoreboardUI();

        if (FootballDataContainer)
            FootballDataContainer.gameObject.SetActive(true);

        if (GameStatusHolder)
            GameStatusHolder.gameObject.SetActive(true);
        
    }

    public override void HideScoreboardUI()
    {
        base.HideScoreboardUI();

        if (FootballDataContainer)
            FootballDataContainer.gameObject.SetActive(false);
    }
}
