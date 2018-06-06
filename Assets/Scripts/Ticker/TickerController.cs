using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TickerController : MonoBehaviour {

    [Serializable]
    public class PlayerObject
    {
        public GameObject tickerHolder;
        public CanvasGroup canvasGroup;
        public TextMeshProUGUI playerStanding;
        public Image countryImage;
        public TextMeshProUGUI playerCountry;
        public TextMeshProUGUI playerName;
        public TextMeshProUGUI playerScore; 
    }

    public List<PlayerObject> players;

    public FlagsContainer _FlagsContainer;

    private string player1_ID="",player2_ID="",player3_ID="";
    private string[] receivedData;

    private string playerLookupURL;

    private void Awake()
    {
        StartCoroutine(GetPlayerLookupUrl());
    }

    IEnumerator GetPlayerLookupUrl()
    {
        yield return AppConfigurationLoader.Instance.GetDataAsync(data =>
        {
            playerLookupURL = data.GetResourceValueByID("PGAPlayerUrlFormat");
        });
    }

    private void OnEnable()
    {
        EventManager.OnBackBtnClickEvent += HideTicker;
    }

    private void OnDisable()
    {
        EventManager.OnBackBtnClickEvent -= HideTicker;
    }

    public void HideTicker()
    {
        gameObject.SetActive(false);
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.U))
    //        UpdateTickerInfo(new string[] { "R", "V1.0", "GLF", "PGAT", "PGAT", "R2017041", "23983", "T1", "-12", "29975", "T1", "-12", "48084", "T1", "-12", "2017 - 04 - 20 16:47:33" });
    //    if (Input.GetKeyDown(KeyCode.E))
    //        UpdateTickerInfo(new string[] { "R", "V1.0", "GLF", "PGAT", "PGAT", "R2017041", "23983", "T1", "-12", "29975", "T1", "-12", "", "", "", "2017 - 04 - 20 16:47:33" });
    //}

    /*
        R\V1.0\GLF\PGAT\PGAT\R2017041\29974\T1\-12\29975\T1\-12\29976\T1\-12\2017-04-20 16:47:33
        FIELD
        Field 0:  Response packet type (always “R”)
        Field 1: Version Number of scoreboard client
        Field 2:  Sport identifier (always “GLF” for Golf)
        Field 3: Source for data,
        Field 4:  League identifier ( PGAT- PGA Tour)
        Field 5:  Tournament ID
        Field 6:  Rank 1 Player ID
        Field 7:  Player Rank
        Field 8:  Player Score
        Field 9:  Rank 2 Player ID
        Field 10:  Player Rank
        Field 11:  Player Score
        Field 12:  Rank 3 Player ID
        Field 13:  Player Rank
        Field 14:  Player Score
        Field 15: UTC Time stamp
     */
    public void UpdateTickerInfo(string[] playerData)
    {
        receivedData = playerData;

        CheckPlayerStatus(0, 6, ref player1_ID);
        CheckPlayerStatus(1, 9, ref player2_ID);
        CheckPlayerStatus(2,12, ref player3_ID);

        //if (playerData[6] =="")
        //{
        //    FadeOutData(0);
        //}
        //else
        //{
        //    if (playerData[6] != player1_ID)
        //    {
        //        //Fade player 1 out and update with new player
        //        UpdateData(0,6);
        //    }
        //    else
        //    {
        //        //Only update the player score wothout fading out
        //        players[0].playerStanding.text = playerData[7];
        //        players[0].playerScore.text = playerData[8];
        //    }
        //}

        //if (playerData[9] =="")
        //{
        //    FadeOutData(1);
        //}
        //else
        //{
        //    if (playerData[9] != player2_ID)
        //    {
        //        //Fade player 2 out and update with new player
        //        UpdateData(1, 9);
        //    }
        //    else
        //    {
        //        //Only update the player score wothout fading out
        //        players[1].playerStanding.text = playerData[10];
        //        players[1].playerScore.text = playerData[11];
        //    }
        //}

        //if (playerData[12] == "")
        //{
        //    FadeOutData(2);
        //}
        //else
        //{
        //    if (playerData[12] != player3_ID)
        //    {
        //        //Fade player 3 out and update with new player
        //        UpdateData(2,12);
        //    }
        //    else
        //    {
        //        //Only update the player score wothout fading out
        //        players[2].playerStanding.text = playerData[13];
        //        players[2].playerScore.text = playerData[14];
        //    }
        //}     
    }

    void CheckPlayerStatus(int playerNumber, int playerDataIndex, ref string playerIDtoCheck)
    {
        if (receivedData[playerDataIndex] == "")
            players[playerNumber].tickerHolder.SetActive(false);
        else
        {
            players[playerNumber].tickerHolder.SetActive(true);

            if (receivedData[playerDataIndex] != playerIDtoCheck)
            {
                //Fade player out and update with new player Data
                UpdateData(playerNumber, playerDataIndex);
            }
            else
            {
                //Only update the player score/standing without fading out
                players[playerNumber].playerStanding.text = receivedData[playerDataIndex + 1];
                players[playerNumber].playerScore.text = receivedData[playerDataIndex + 2];
            }
        }

        playerIDtoCheck = receivedData[playerDataIndex];
    }
    
    /// <summary>
    /// Starts the Update coroutine that Handles updating the data
    /// </summary>
    /// <param name="playerNumber">Player Number in the Ticker</param>
    /// <param name="playerDataIndex">Start index in receved data for the player</param>
    public void UpdateData(int playerNumber,int playerDataIndex)
    {
        StartCoroutine(UpdateDataCoroutine(playerNumber,playerDataIndex));
    }

    IEnumerator UpdateDataCoroutine(int playerNumber,int playerDataIndex)
    {
        //FadeOut-->UpdateData-->FadeIn
        yield return StartCoroutine(FadeOutCanvas(players[playerNumber].canvasGroup));

        players[playerNumber].playerStanding.text = receivedData[playerDataIndex + 1];
        players[playerNumber].playerScore.text = receivedData[playerDataIndex + 2];

        string[] playerDetails;
        using (var www = new WWW(string.Format(playerLookupURL,receivedData[playerDataIndex])))
        {
            yield return www;

            if (!String.IsNullOrEmpty(www.error))
            {
                Debug.LogErrorFormat("Error downloading '{0}': {1}", playerLookupURL, www.error);
                yield break;
            }

            playerDetails = www.text.Split(',');
        }

        if (playerDetails.Length > 1)
        {
            //If player Details were not fetched then dont Fade In
            players[playerNumber].playerName.text = playerDetails[1].ToUpper();
            //players[playerNumber].playerCountry.text = playerDetails[2];    // This might change to fetching country image
            //Debug.Log(playerDetails[2].Substring(0,playerDetails[2].IndexOf('<')));
            _FlagsContainer.SetFlag(playerDetails[2].Substring(0, 3), players[playerNumber].countryImage);
            FadeInData(playerNumber);
        }
        else
        {
            players[playerNumber].tickerHolder.SetActive(false);
            Debug.Log("Empty Data returned by Player Lookup API: for playerID" + receivedData[playerDataIndex]);
        }
    }

    /// <summary>
    /// Fades out the specific player in the Ticker
    /// </summary>
    /// <param name="playerNumber"></param>
    public void FadeOutData(int playerNumber)
    {
        StartCoroutine(FadeOutCanvas(players[playerNumber].canvasGroup));

        //for (int i = 0; i < players.Count; i++)
        //{
        //    StartCoroutine(FadeOutCanvas(players[i].canvasGroup,i));
        //}
    }

    IEnumerator FadeOutCanvas(CanvasGroup toFade)
    {
        while (toFade.alpha > 0f)
        {
            toFade.alpha -= 0.1f;
            yield return new WaitForSeconds(0.05f);
        };
    }

    /// <summary>
    /// Fades In the specific player in the ticker
    /// </summary>
    /// <param name="playerNumber"></param>
    public void FadeInData(int playerNumber)
    {
        StartCoroutine(FadeInCanvas(players[playerNumber].canvasGroup));

        //for (int i = 0; i < players.Count; i++)
        //{
        //    StartCoroutine(FadeInCanvas(players[i].canvasGroup));
        //}
    }

    IEnumerator FadeInCanvas(CanvasGroup toFade)
    {
        while (toFade.alpha < 1f)
        {
            toFade.alpha += 0.1f;
            yield return new WaitForSeconds(0.05f);
        }
    }

}
