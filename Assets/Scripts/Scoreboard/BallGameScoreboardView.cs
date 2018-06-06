using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class BallGameScoreboardView : ScoreboardView {

    protected GameObject BallGameScoreboard;
    //protected GameObject ScoreboardMesh;
    protected GameObject ScoreboardCanvas;

    protected DynamicTexture AwayTeamLogo;
    protected DynamicTexture HomeTeamLogo;
    protected DynamicTexture SponsorshipCenter;

    protected TextMeshProUGUI AwayTeamScoreText;
    protected TextMeshProUGUI HomeTeamScoreText;

    protected TextMeshProUGUI TimeHeaderText;
    protected TextMeshProUGUI TimeSubheaderText;

    protected TextMeshProUGUI AwayTeamHeaderText;
    protected TextMeshProUGUI AwayTeamSubheaderText;
    protected TextMeshProUGUI HomeTeamHeaderText;
    protected TextMeshProUGUI HomeTeamSubheaderText;

    protected BallGameScoreboardView()
    {
    }

    public virtual void LoadRequiredComponents(GameObject parentComponent)
    {
        BallGameScoreboard = Extensions.GetRequired<Component>(parentComponent, Extensions.GetVariableName(() => BallGameScoreboard)).gameObject;
 
        //ScoreboardMesh = Extensions.GetRequired<Component>(BallGameScoreboard, Extensions.GetVariableName(() => ScoreboardMesh)).gameObject;


        //SponsorshipCenter = Extensions.GetRequired<DynamicTexture>(ScoreboardMesh, Extensions.GetVariableName(() => SponsorshipCenter));

        ScoreboardCanvas = Extensions.GetRequired<Component>(BallGameScoreboard, Extensions.GetVariableName(() => ScoreboardCanvas)).gameObject;

		AwayTeamLogo = Extensions.GetRequired<DynamicTexture>(ScoreboardCanvas, Extensions.GetVariableName(() => AwayTeamLogo));
		HomeTeamLogo = Extensions.GetRequired<DynamicTexture>(ScoreboardCanvas, Extensions.GetVariableName(() => HomeTeamLogo));
        AwayTeamHeaderText = Extensions.GetRequired<TextMeshProUGUI>(ScoreboardCanvas, Extensions.GetVariableName(() => AwayTeamHeaderText));
        HomeTeamHeaderText = Extensions.GetRequired<TextMeshProUGUI>(ScoreboardCanvas, Extensions.GetVariableName(() => HomeTeamHeaderText));
        AwayTeamSubheaderText = Extensions.GetRequired<TextMeshProUGUI>(ScoreboardCanvas, Extensions.GetVariableName(() => AwayTeamSubheaderText));
        HomeTeamSubheaderText = Extensions.GetRequired<TextMeshProUGUI>(ScoreboardCanvas, Extensions.GetVariableName(() => HomeTeamSubheaderText));
        AwayTeamScoreText = Extensions.GetRequired<TextMeshProUGUI>(ScoreboardCanvas, Extensions.GetVariableName(() => AwayTeamScoreText));
        HomeTeamScoreText = Extensions.GetRequired<TextMeshProUGUI>(ScoreboardCanvas, Extensions.GetVariableName(() => HomeTeamScoreText));
        TimeHeaderText = Extensions.GetRequired<TextMeshProUGUI>(ScoreboardCanvas, Extensions.GetVariableName(() => TimeHeaderText));
        TimeSubheaderText = Extensions.GetRequired<TextMeshProUGUI>(ScoreboardCanvas, Extensions.GetVariableName(() => TimeSubheaderText));
    }

    public virtual void UpdateScoreboardUI(ScoreboardDataModel dataModel)
    {
        /*
        BallGameScoreboardDataModel bgDataModel = dataModel as BallGameScoreboardDataModel;

        // Team Name
        if (AwayTeamHeadlineText != null)
            AwayTeamHeadlineText.text = bgDataModel.AwayTeamName;
        if (HomeTeamHeadlineText != null)
            HomeTeamHeadlineText.text = bgDataModel.HomeTeamName;

        // Team Nickname
        if (AwayTeamSubheadText != null)
            AwayTeamSubheadText.text = bgDataModel.AwayTeamNickname;
        if (HomeTeamSubheadText != null)
            HomeTeamSubheadText.text = bgDataModel.HomeTeamNickname;

        // Team Logo
        if (AwayTeamLogo != null && mAwayTeamLogoUrl != bgDataModel.AwayTeamLogoUrl)
        {
            AwayTeamLogo.SetTexture(bgDataModel.AwayTeamLogoUrl);
            mAwayTeamLogoUrl = bgDataModel.AwayTeamLogoUrl;
        }
        if (HomeTeamLogo != null && mHomeTeamLogoUrl != bgDataModel.HomeTeamLogoUrl)
        {
            HomeTeamLogo.SetTexture(bgDataModel.HomeTeamLogoUrl);
            mHomeTeamLogoUrl = bgDataModel.HomeTeamLogoUrl;
        }

        // Team Color

         // Game Score        
        if (AwayTeamScoreText != null)
            AwayTeamScoreText.text = bgDataModel.AwayTeamScore.ToString();
        if (HomeTeamScoreText != null)
            HomeTeamScoreText.text = bgDataModel.HomeTeamScore.ToString();

        // Sponsorship Logo
        if (SponsorshipCenter != null && mSponsorshipLogoUrl != bgDataModel.LeagueLogoUrl)
        {
            SponsorshipCenter.SetTexture(bgDataModel.LeagueLogoUrl);
            mSponsorshipLogoUrl = bgDataModel.LeagueLogoUrl;
        }
        */
    }

    public virtual void UpdateScoreboardData(string propName, string propValue)
    {
        try
        {
            // Team Name
			if (propName == "AwayTeamNickname")
            {
                if (AwayTeamHeaderText != null)
                    AwayTeamHeaderText.text = propValue;
            }
			else if (propName == "HomeTeamNickname")
            {
                if (HomeTeamHeaderText != null)
                    HomeTeamHeaderText.text = propValue;
            }
            // Team Nickname
			else if (propName == "AwayTeamName")
            {
                if (AwayTeamSubheaderText != null)
                    AwayTeamSubheaderText.text = propValue;
            }
			else if (propName == "HomeTeamName")
            {
                if (HomeTeamSubheaderText != null)
                    HomeTeamSubheaderText.text = propValue;
            }
            // Team Logo
            else if (propName == "AwayTeamLogoUrl")
            {
                if (AwayTeamLogo != null)
				{
                    AwayTeamLogo.SetTexture(propValue, true);
				}
            }
            else if (propName == "HomeTeamLogoUrl")
            {
                if (HomeTeamLogo != null)
				{
                    HomeTeamLogo.SetTexture(propValue, true);
				}
            }
            // Game Score 
            else if (propName == "AwayTeamScore")
            {
                if (AwayTeamScoreText != null)
                    AwayTeamScoreText.text = propValue;
            }
            else if (propName == "HomeTeamScore")
            {
                if (HomeTeamScoreText != null)
                    HomeTeamScoreText.text = propValue;
            }
            // Sponsorship Logo
//            else if (propName == "LeagueLogoUrl")
//            {
//                if (SponsorshipCenter != null)
//                    SponsorshipCenter.SetTexture(propValue);
//            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message); // TODO
        }
    }

    public virtual void ResetScoreboardData()
    {
    }

    public virtual void ShowScoreboardUI()
    {
        if(BallGameScoreboard)
            BallGameScoreboard.gameObject.SetActive (true);

//        if(ScoreboardMesh)
//            ScoreboardMesh.gameObject.SetActive (true);        

        if(ScoreboardCanvas)
            ScoreboardCanvas.gameObject.SetActive (true);        
    }

    public virtual void HideScoreboardUI()
    {
        if(BallGameScoreboard)
            BallGameScoreboard.gameObject.SetActive (false);

//        if(ScoreboardMesh)
//            ScoreboardMesh.gameObject.SetActive (false);
    
        if(ScoreboardCanvas)
            ScoreboardCanvas.gameObject.SetActive (true);        
    }

    /// <summary>
    /// Set the 'color bar' color for a team - if no color available, hide color bar
    /// </summary>
    /// <returns>The team color.</returns>
    /// <param name="teamGameObject">Team game object.</param>
    /// <param name="hexColor">Hex color.</param>
    private void SetTeamColor(GameObject teamGameObject, string hexColor)
    {
        if (teamGameObject == null)
            return;

        teamGameObject.SetActive(false);
        if (!string.IsNullOrEmpty(hexColor))
        {
            Color newCol;
            if (ColorUtility.TryParseHtmlString(hexColor, out newCol))
            {
                teamGameObject.SetActive(true);
                Renderer rend = teamGameObject.GetComponent<Renderer>();
                rend.material.color = newCol;
            }
        }
    }
}
