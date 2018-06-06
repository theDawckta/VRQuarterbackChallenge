using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class GolfScoreboardView : ScoreboardView {

    private const int NUM_OF_PLAYERS = 3;

    private class PlayerUIHolder
    {
        public GameObject       TickerHolder;
        public GameObject       TickerData;
        public TextMeshProUGUI  PlayerStandingText;
        public GameObject       PlayerCountryImage;
        public TextMeshProUGUI  PlayerCountryText;
        public TextMeshProUGUI  PlayerNameText;
        public TextMeshProUGUI  PlayerScoreText; 
    }

    private GameObject GolfScoreTicker;

    private List<PlayerUIHolder> PlayersUIHolder;

    private FlagsContainer CountryFlagsHolder;

    private int mMaxNumbOfPlayers;

    private bool fadeBackIn = false;

    public GolfScoreboardView()
    {
        PlayersUIHolder = new List<PlayerUIHolder>();
        mMaxNumbOfPlayers = GolfScoreboardDataModel.MAX_NUM_OF_PLAYERS;
    }

    public void LoadRequiredComponents(GameObject parentComponent)
    {
        Debug.Log("GolfScoreboardView::LoadRequiredComponents()");
        try
        {
            GolfScoreTicker = Extensions.GetRequired<Component>(parentComponent, Extensions.GetVariableName(() => GolfScoreTicker)).gameObject;

            for (int i = 0; i < GolfScoreboardDataModel.MAX_NUM_OF_PLAYERS; i++)
            {
                PlayerUIHolder player = new PlayerUIHolder();
                player.TickerHolder = Extensions.GetRequired<Component>(GolfScoreTicker, 
                    string.Format("{0}_{1}", Extensions.GetVariableName(() => player.TickerHolder), i + 1)).gameObject;
                player.TickerData = Extensions.GetRequired<Component>(player.TickerHolder, 
                    string.Format("{0}_{1}", Extensions.GetVariableName(() => player.TickerData), i + 1)).gameObject;
                player.PlayerStandingText = Extensions.GetRequired<TextMeshProUGUI>(player.TickerData, Extensions.GetVariableName(() => player.PlayerStandingText));
                player.PlayerCountryImage = Extensions.GetRequired<Component>(player.TickerData, Extensions.GetVariableName(() => player.PlayerCountryImage)).gameObject;
                player.PlayerCountryText = Extensions.GetRequired<TextMeshProUGUI>(player.TickerData, Extensions.GetVariableName(() => player.PlayerCountryText));
                player.PlayerNameText = Extensions.GetRequired<TextMeshProUGUI>(player.TickerData, Extensions.GetVariableName(() => player.PlayerNameText));
                player.PlayerScoreText = Extensions.GetRequired<TextMeshProUGUI>(player.TickerData, Extensions.GetVariableName(() => player.PlayerScoreText));

                PlayersUIHolder.Add(player);
            }

            // TODO: Any better way?
            CountryFlagsHolder = GameObject.FindObjectOfType<FlagsContainer>();
            if (CountryFlagsHolder == null)
            {
                GameObject holder = new GameObject("FlagsContainer");
                CountryFlagsHolder = parentComponent.AddComponent<FlagsContainer>();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message); // TODO
        }

        ShowScoreboardUI();

        ResetScoreboardData();
    }

    public void UpdateScoreboardUI(ScoreboardDataModel dataModel)
    {
        // Do nothing!
    }

    public void UpdateScoreboardData(string propName, string propValue)
    {
        Debug.LogFormat("GolfScoreboardView::UpdateScoreboardData(): name={0}, value={1}", propName, propValue);

        try 
        {
            if (propName.StartsWith("Player["))
            {
                int index;
                int.TryParse(propName.Substring(7, propName.IndexOf("]") - 7), out index);

                string propSubName = propName.Substring(propName.IndexOf(".") + 1);

                //Debug.LogFormat("JK DEBUG:: UpdateScoreboardData(): index={0}, propSubName={1}", index, propSubName);

                if (index > 0 && index <= mMaxNumbOfPlayers)
                {
                    PlayerUIHolder playerUI = PlayersUIHolder[index - 1];

                    if (playerUI.TickerHolder == null)
                        return;

                    if (propSubName == "ID")
                    {
                        if (string.IsNullOrEmpty(propValue))
                        {
                            playerUI.TickerHolder.SetActive(false);
                        }
                        else
                        {
                            //FadeOut
                            fadeBackIn = true;
                        }                           
                    }
                    else
                    {
                        if (propSubName == "Rank")
                        {
                            if (playerUI.PlayerStandingText)
                                playerUI.PlayerStandingText.text = propValue;
                        }
                        else if (propSubName == "Name")
                        {
                            if (playerUI.PlayerNameText)
                                playerUI.PlayerNameText.text = propValue;
                        }
                        else if (propSubName == "Score")
                        {
                            if (playerUI.PlayerScoreText)
                                playerUI.PlayerScoreText.text = propValue;
                        }
                        else if (propSubName == "CountryCode")
                        {
                            if (CountryFlagsHolder != null && playerUI.PlayerCountryImage != null)
                            {
                                CountryFlagsHolder.SetFlag(propValue, 
                                    playerUI.PlayerCountryImage.GetComponent<UnityEngine.UI.Image>());
                            }
                        }

                        if (fadeBackIn == true)
                        {
                            fadeBackIn = false;
                            //TODO FADE IN VIA DOTWEEN
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message); // TODO
        }
    }

    public virtual void ResetScoreboardData()
    {
        try 
        {
            Debug.Log("GolfScoreboardView:: ResetScoreboardData()");

            for (int i = 0; i < GolfScoreboardDataModel.MAX_NUM_OF_PLAYERS; i++)
            {
                PlayerUIHolder playerUI = PlayersUIHolder[i];

                if (playerUI.TickerHolder == null)
                    continue;
                
                 //playerUI.TickerHolder.SetActive(true);

                if (playerUI.PlayerStandingText)
                    playerUI.PlayerStandingText.text = "";

                if (playerUI.PlayerNameText)
                    PlayersUIHolder[i].PlayerNameText.text = "";

                if (playerUI.PlayerScoreText)
                    PlayersUIHolder[i].PlayerScoreText.text = "";

                if (playerUI.PlayerCountryImage)
                {
                    // TODO
                    //playerUI.PlayerCountryImage.GetComponent<UnityEngine.UI.Image>()
                }                        
            }
         }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message); // TODO
        }    
    }

    public void ShowScoreboardUI()
    {
        if (GolfScoreTicker)
            GolfScoreTicker.gameObject.SetActive(true);
    }

    public void HideScoreboardUI()
    {
        if (GolfScoreTicker)
            GolfScoreTicker.gameObject.SetActive(false);
    }

    /// <summary>
    /// Fades out a Canvas group
    /// </summary>
    /// <param name="toFade">Canvas Group</param>
    IEnumerator FadeOut(CanvasGroup toFade)
    {
        while (toFade.alpha > 0f)
        {
            Debug.Log("Fading out");
            toFade.alpha -= 0.1f;
            yield return new WaitForSeconds(0.05f);
        };
    }

    /// <summary>
    /// Fades in a Canvas group
    /// </summary>
    /// <param name="toFade">Canvas Group</param>
    IEnumerator FadeIn(CanvasGroup toFade)
    {
        while (toFade.alpha < 1f)
        {
            Debug.Log("Fading In");
            toFade.alpha += 0.1f;
            yield return new WaitForSeconds(0.05f);
        }
    }
}
