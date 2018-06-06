using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class BaseballScoreboardView : BallGameScoreboardView {

    public GameObject BaseballDataContainer;
    public GameObject PitcherBatterDataContainer;
    public GameObject GameStatusHolder;
 
    public GameObject PBDataGroupLeft;
    public GameObject PBDataGroupRight;
    public GameObject BatterPanel;
    public GameObject PitcherPanel;

    public GameObject HeadshotLeft;
    public GameObject HeadshotRight;

    public TextMeshProUGUI BallText;
    public TextMeshProUGUI StrikeText;
    public TextMeshProUGUI OutText;
    public TextMeshProUGUI InningsText;

    public GameObject InningArrowTop;
    public GameObject InningArrowBottom;

    public TextMeshProUGUI AwayTeamWinLossText;
    public TextMeshProUGUI HomeTeamWinLossText;

    public GameObject FirstBase;
    public GameObject SecondBase;
    public GameObject ThirdBase;

    public GameObject TeamLogoLeft;
    public GameObject TeamLogoRight;

    public TextMeshProUGUI BatterName;
    public TextMeshProUGUI BatterAverage;
    public TextMeshProUGUI BatterHCAtBat;
    public TextMeshProUGUI BatterHR;
    public TextMeshProUGUI BatterRBI;

    public TextMeshProUGUI PitcherName;
    public TextMeshProUGUI PitcherERA;
    public TextMeshProUGUI PitchCount;
    public TextMeshProUGUI InningsPitched;

    private string mHeadshotUrl;
    private Dictionary<string, Texture2D> mPlayerHeadshots;
    private Texture2D mSilhouetteHeadshot; 

    public BaseballScoreboardView() : base ()
    {
        mPlayerHeadshots = new Dictionary<string, Texture2D>();

        // TODO: Load the default headshot image
        mSilhouetteHeadshot = null;
    }

    public override void LoadRequiredComponents(GameObject parentComponent)
    {
        Debug.Log("BaseballScoreboardView::LoadRequiredComponents()");

        try
        {
            base.LoadRequiredComponents(parentComponent);
//            PBDataGroupLeft = Extensions.GetRequired<Component>(ScoreboardMesh, Extensions.GetVariableName(() => PBDataGroupLeft)).gameObject;
            HeadshotLeft = Extensions.GetRequired<Component>(PBDataGroupLeft, Extensions.GetVariableName(() => HeadshotLeft)).gameObject;

//            PBDataGroupRight = Extensions.GetRequired<Component>(ScoreboardMesh, Extensions.GetVariableName(() => PBDataGroupRight)).gameObject;
            HeadshotRight = Extensions.GetRequired<Component>(PBDataGroupRight, Extensions.GetVariableName(() => HeadshotRight)).gameObject;

            GameStatusHolder = Extensions.GetRequired<Component>(ScoreboardCanvas, Extensions.GetVariableName(() => GameStatusHolder)).gameObject;
            AwayTeamWinLossText = Extensions.GetRequired<TextMeshProUGUI>(ScoreboardCanvas, Extensions.GetVariableName(() => AwayTeamWinLossText));
            HomeTeamWinLossText = Extensions.GetRequired<TextMeshProUGUI>(ScoreboardCanvas, Extensions.GetVariableName(() => HomeTeamWinLossText));

            BaseballDataContainer = Extensions.GetRequired<Component>(parentComponent, Extensions.GetVariableName(() => BaseballDataContainer)).gameObject;
            BallText = Extensions.GetRequired<TextMeshProUGUI>(BaseballDataContainer, Extensions.GetVariableName(() => BallText));
            StrikeText = Extensions.GetRequired<TextMeshProUGUI>(BaseballDataContainer, Extensions.GetVariableName(() => StrikeText));
            OutText = Extensions.GetRequired<TextMeshProUGUI>(BaseballDataContainer, Extensions.GetVariableName(() => OutText));
            InningsText = Extensions.GetRequired<TextMeshProUGUI>(BaseballDataContainer, Extensions.GetVariableName(() => InningsText));
            InningArrowTop = Extensions.GetRequired<Component>(BaseballDataContainer, Extensions.GetVariableName(() => InningArrowTop)).gameObject;
            InningArrowBottom = Extensions.GetRequired<Component>(BaseballDataContainer, Extensions.GetVariableName(() => InningArrowBottom)).gameObject;
            FirstBase = Extensions.GetRequired<Component>(BaseballDataContainer, Extensions.GetVariableName(() => FirstBase)).gameObject;
            SecondBase = Extensions.GetRequired<Component>(BaseballDataContainer, Extensions.GetVariableName(() => SecondBase)).gameObject;
            ThirdBase = Extensions.GetRequired<Component>(BaseballDataContainer, Extensions.GetVariableName(() => ThirdBase)).gameObject;
            TeamLogoLeft = Extensions.GetRequired<Component>(BaseballDataContainer, Extensions.GetVariableName(() => TeamLogoLeft)).gameObject;
            TeamLogoRight = Extensions.GetRequired<Component>(BaseballDataContainer, Extensions.GetVariableName(() => TeamLogoRight)).gameObject;

            PitcherBatterDataContainer = Extensions.GetRequired<Component>(parentComponent, Extensions.GetVariableName(() => PitcherBatterDataContainer)).gameObject;
            BatterPanel = Extensions.GetRequired<Component>(PitcherBatterDataContainer, Extensions.GetVariableName(() => BatterPanel)).gameObject;
            BatterName = Extensions.GetRequired<TextMeshProUGUI>(BatterPanel, Extensions.GetVariableName(() => BatterName));
            BatterAverage = Extensions.GetRequired<TextMeshProUGUI>(BatterPanel, Extensions.GetVariableName(() => BatterAverage));
            BatterHCAtBat = Extensions.GetRequired<TextMeshProUGUI>(BatterPanel, Extensions.GetVariableName(() => BatterHCAtBat));
            BatterHR = Extensions.GetRequired<TextMeshProUGUI>(BatterPanel, Extensions.GetVariableName(() => BatterHR));
            BatterRBI = Extensions.GetRequired<TextMeshProUGUI>(BatterPanel, Extensions.GetVariableName(() => BatterRBI));

            PitcherPanel = Extensions.GetRequired<Component>(PitcherBatterDataContainer, Extensions.GetVariableName(() => PitcherPanel)).gameObject;
            PitcherName = Extensions.GetRequired<TextMeshProUGUI>(PitcherPanel, Extensions.GetVariableName(() => PitcherName));
            PitcherERA = Extensions.GetRequired<TextMeshProUGUI>(PitcherPanel, Extensions.GetVariableName(() => PitcherERA));
            PitchCount = Extensions.GetRequired<TextMeshProUGUI>(PitcherPanel, Extensions.GetVariableName(() => PitchCount));
            InningsPitched = Extensions.GetRequired<TextMeshProUGUI>(PitcherPanel, Extensions.GetVariableName(() => InningsPitched));
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message); // TODO
        }

        ShowScoreboardUI();
    }

    public override void UpdateScoreboardUI(ScoreboardDataModel dataModel)
    {
        Debug.Log("BaseballScoreboardView:: UpdateScoreboardUI()");

        try 
        {
            base.UpdateScoreboardUI(dataModel);
 
//            BaseballScoreboardDataModel bsbDataModel = dataModel as BaseballScoreboardDataModel;
//
//            if (BallText)
//                BallText.text = bsbDataModel.BallCount.ToString();
//
//            if (StrikeText)
//                StrikeText.text = bsbDataModel.StrikeCount.ToString();
//
//            if (OutText)
//                OutText.text = bsbDataModel.OutCount.ToString();
//
//            if (InningsText)
//                InningsText.text = bsbDataModel.Inning.ToString();
//
//            if (AwayTeamWinLossText)
//                AwayTeamWinLossText.text = string.Format("{0}-{1}", bsbDataModel.AwayTeamWins, bsbDataModel.AwayTeamLosses);
//
//            if (HomeTeamWinLossText)
//                HomeTeamWinLossText.text = string.Format("{0}-{1}", bsbDataModel.HomeTeamWins, bsbDataModel.HomeTeamLosses);
//
//            if (FirstBase)
//                FirstBase.SwitchActiveState(bsbDataModel.PlayerOn1Base);
// 
//            if (SecondBase)
//                SecondBase.SwitchActiveState(bsbDataModel.PlayerOn2Base);
// 
//            if (ThirdBase)
//                ThirdBase.SwitchActiveState(bsbDataModel.PlayerOn3Base);
// 
//            if (InningArrowTop)
//                InningArrowTop.SwitchActiveState(bsbDataModel.IsTopOfInning);
//
//            if (InningArrowBottom)
//                InningArrowBottom.SwitchActiveState(!bsbDataModel.IsTopOfInning);
//
//            if (TeamLogoLeft)
//                MLBLogosContainer.Instance.SetLogo(bsbDataModel.AwayTeamID, TeamLogoLeft.GetComponent<UnityEngine.UI.Image>());
//
//            if (TeamLogoRight)
//                MLBLogosContainer.Instance.SetLogo(bsbDataModel.HomeTeamID, TeamLogoRight.GetComponent<UnityEngine.UI.Image>());
//
//            if (BatterName)
//                BatterName.text = bsbDataModel.BatterName;
//
//            if (BatterAverage)
//                BatterAverage.text = bsbDataModel.BatterBattingAverage.ToString();
//
//            if (BatterHCAtBat)
//                BatterHCAtBat.text = bsbDataModel.BatterHitsCount.ToString() + "-" + bsbDataModel.BatterAtBatsCount.ToString();
//
//            if (BatterHR)
//                BatterHR.text = bsbDataModel.BatterHomerunCount.ToString();
//
//            if (BatterRBI)
//                BatterRBI.text = bsbDataModel.BatterRunsBattedIn.ToString();
//
//            if (PitcherName)
//                PitcherName.text = bsbDataModel.PitcherName;
//
//            if (PitcherERA)
//                PitcherERA.text = bsbDataModel.PitcherEarnedRunAverage.ToString();
//
//            if (PitchCount)
//                PitchCount.text = bsbDataModel.PitcherPitchCount.ToString();
//
//            if (InningsPitched)
//                InningsPitched.text = bsbDataModel.PitcherInningsPitched.ToString();
//
//            // Batter data will be on the left when we are in the top inning and right when we are in the bottom
//            if ((bsbDataModel.IsTopOfInning && (PitcherPanel.transform.localPosition.x > BatterPanel.transform.localPosition.x)) ||
//                (!bsbDataModel.IsTopOfInning && (PitcherPanel.transform.localPosition.x < BatterPanel.transform.localPosition.x)))
//            {
//                SwapPitcherAndBatterPanels();
//            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message); // TODO
        }
    }

    public override void UpdateScoreboardData(string propName, string propValue)
    {
        Debug.LogFormat("BaseballScoreboardView::UpdateScoreboardData(): name={0}, value={1}", propName, propValue);

        try
        {
            if (propName == "BallCount")
            {
                if (BallText)
                    BallText.text = propValue;
            }
            else if (propName == "StrikeCount")
            {
                if (StrikeText)
                    StrikeText.text = propValue;
            }
            else if (propName == "OutCount")
            {
                if (OutText)
                    OutText.text = propValue;
            }
            else if (propName == "Inning")
            {
                if (InningsText)
                    InningsText.text = propValue;
            }
            else if (propName == "AwayTeamWinsLosses")
            {
                if (AwayTeamWinLossText)
                    AwayTeamWinLossText.text = propValue;
            }
            else if (propName == "HomeTeamWinsLosses")
            {
                if (HomeTeamWinLossText)
                    HomeTeamWinLossText.text = propValue;
            }
            else if (propName == "PlayerOn1Base")
            {
                if (FirstBase)
                    FirstBase.SwitchActiveState(bool.Parse(propValue));
            }
            else if (propName == "PlayerOn2Base")
            {
                if (SecondBase)
                    SecondBase.SwitchActiveState(bool.Parse(propValue));
            }
            else if (propName == "PlayerOn3Base")
            {
                if (ThirdBase)
                    ThirdBase.SwitchActiveState(bool.Parse(propValue));
            }
            else if (propName == "IsTopOfInning")
            {
                bool bVal = bool.Parse(propValue);
                if (InningArrowTop)
                    InningArrowTop.SwitchActiveState(bVal);
                if (InningArrowBottom)
                    InningArrowBottom.SwitchActiveState(!bVal);

                // Batter data will be on the left when we are in the top inning and right when we are in the bottom
                if ((bVal && (PitcherPanel.transform.localPosition.x > BatterPanel.transform.localPosition.x)) ||
                    (!bVal && (PitcherPanel.transform.localPosition.x < BatterPanel.transform.localPosition.x)))
                {
                    swapPitcherAndBatterPanels();
                }
            }
            else if (propName == "AwayTeamID")
            {
                if (TeamLogoLeft)
                    MLBLogosContainer.Instance.SetLogo(propValue, TeamLogoLeft.GetComponent<UnityEngine.UI.Image>());

                EventManager.Instance.TeamNameReceivedEvent(false, propValue);
            }
            else if (propName == "HomeTeamID")
            {
                if (TeamLogoRight)
                    MLBLogosContainer.Instance.SetLogo(propValue, TeamLogoRight.GetComponent<UnityEngine.UI.Image>());

                EventManager.Instance.TeamNameReceivedEvent(true, propValue);
            }
            //else if (propName == "BatterID")
            //{
            //    // TODO: JK DEBUG, Update the headshot
            //    //if (HeadshotLeft)
            //    //    setHeadshotImage(HeadshotLeft.GetComponent<Renderer>(), propValue);
            //    EventManager.Instance.PitcherBatterChanged(propValue, inningHalf, GridStats.IconType.Batter);
            //}
            else if (propName == "BatterName")
            {
                if (BatterName)
                    BatterName.text = propValue;
            }
            else if (propName == "BatterBattingAverage")
            {
                if (BatterAverage)
                    BatterAverage.text = propValue;
            }
            else if (propName == "BatterHitsAtBatsCount")
            {
                if (BatterHCAtBat)
                    BatterHCAtBat.text = propValue;
            }
            else if (propName == "BatterHomerunCount")
            {
                if (BatterHR)
                    BatterHR.text = propValue;
            }
            else if (propName == "BatterRunsBattedIn")
            {
                if (BatterRBI)
                    BatterRBI.text = propValue;
            }
            //else if (propName == "PitcherID")
            //{
            //    // TODO: JK DEBUG, Update the headshot
            //    //if (HeadshotRight)
            //    //    setHeadshotImage(HeadshotRight.GetComponent<Renderer>(), propValue);
            //    EventManager.Instance.PitcherBatterChanged(propValue, inningHalf, GridStats.IconType.Pitcher);
            //}
            else if (propName == "PitcherName")
            {
                if (PitcherName)
                    PitcherName.text = propValue;
            }
            else if (propName == "PitcherEarnedRunAverage")
            {
                if (PitcherERA)
                    PitcherERA.text = propValue;
            }
            else if (propName == "PitcherPitchCount")
            {
                if (PitchCount)
                    PitchCount.text = propValue;
            }
            else if (propName == "PitcherInningsPitched")
            {
                if (InningsPitched)
                    InningsPitched.text = propValue;
            }
            else if (propName == "PlayerInfoUrl")
            {
                mHeadshotUrl = propValue;
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

        if (BaseballDataContainer)
            BaseballDataContainer.gameObject.SetActive(true);

        if (AwayTeamWinLossText)
            AwayTeamWinLossText.gameObject.SetActive (true);

        if (HomeTeamWinLossText)
            HomeTeamWinLossText.gameObject.SetActive (true);

        if (GameStatusHolder)
            GameStatusHolder.gameObject.SetActive(false);
        
        if (FirstBase)
            FirstBase.SwitchActiveState(false);

        if (SecondBase)
            SecondBase.SwitchActiveState(false);

        if (ThirdBase)
            ThirdBase.SwitchActiveState(false);

        if (PitcherBatterDataContainer)
            PitcherBatterDataContainer.gameObject.SetActive(true);

        if (PBDataGroupLeft)
            PBDataGroupLeft.gameObject.SetActive(true);

        if (PBDataGroupRight)
            PBDataGroupRight.gameObject.SetActive(true);
    }

    public override void HideScoreboardUI()
    {
        base.HideScoreboardUI();

        if (BaseballDataContainer)
            BaseballDataContainer.gameObject.SetActive(false);

        if (AwayTeamWinLossText)
            AwayTeamWinLossText.gameObject.SetActive (false);

        if (HomeTeamWinLossText)
            HomeTeamWinLossText.gameObject.SetActive (false);

        if (GameStatusHolder)
            GameStatusHolder.gameObject.SetActive(true);

        if (PitcherBatterDataContainer)
            PitcherBatterDataContainer.gameObject.SetActive(false);

        if (PBDataGroupLeft)
            PBDataGroupLeft.gameObject.SetActive(false);
 
        if (PBDataGroupRight)
            PBDataGroupRight.gameObject.SetActive(false);
    }

    private void swapPitcherAndBatterPanels()
    {
        Vector3 tempPosition = PitcherPanel.transform.localPosition;
        Quaternion tempRotation = PitcherPanel.transform.localRotation;

        PitcherPanel.transform.localPosition = BatterPanel.transform.localPosition;
        BatterPanel.transform.localPosition = tempPosition;

        PitcherPanel.transform.localRotation = BatterPanel.transform.localRotation;
        BatterPanel.transform.localRotation = tempRotation;
    }

    private void setHeadshotImage(Renderer rend, string playerID)
    {
        Texture2D headshotImage;
        if (mPlayerHeadshots.TryGetValue(playerID, out headshotImage))
        {
            rend.material.mainTexture = headshotImage;
        }
        else
        {
            rend.material.mainTexture = downloadPlayerHeadshot(mHeadshotUrl, playerID);
        }
    }

    private Texture2D downloadPlayerHeadshot(string url, string playerID)
    {
        // Verify the input parameters
        if (string.IsNullOrEmpty(url + playerID + ".png"))
        {
            return mSilhouetteHeadshot;
        }

        Debug.Log("BaseballScoreboardDataModel::downloadPlayerHeadshot(): Downloading a JSON file from " + url);

        // Start a download of the given URL
        WWW www = new WWW(url);

        while (!www.isDone)
        {
        }     // TODO: JK COMMENT, Is there any good way to download the JSON fils synchronously?
        if (string.IsNullOrEmpty(www.error))
        {
            // Create a texture in DXT1 format
            Texture2D texture = new Texture2D(www.texture.width, www.texture.height, TextureFormat.DXT1, true);
            texture.filterMode = FilterMode.Trilinear;
            //texture.wrapMode = TextureWrapMode.Clamp;

            // assign the downloaded image to sprite
            www.LoadImageIntoTexture(texture);

            //Adding to local dictionary of headshots
            mPlayerHeadshots.Add(playerID, texture);

            return texture;
        }
        else
        {
            Debug.Log("aseballScoreboardDataModel::downloadPlayerHeadshot(): Failed to download a headshot for " + playerID);
            return mSilhouetteHeadshot;
        }
    }
}
