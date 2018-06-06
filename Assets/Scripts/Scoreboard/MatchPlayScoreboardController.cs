using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class MatchPlayScoreboardController : MonoBehaviour {

    public FlagsContainer _FlagsContainer;
    public DynamicTexture tournamentLogo;

    public Image team1Image;
    public TextMeshProUGUI team1Name;
    public TextMeshProUGUI team1Score;

    public Image team2Image;
    public TextMeshProUGUI team2Name;
    public TextMeshProUGUI team2Score;

    private bool firstRun = true;
    private string tournamentLogosPath;
    private string team1PreviousScore = "";
    private string team2PreviousScore = "";

    IEnumerator Start()
    {
        yield return AppConfigurationLoader.Instance.GetDataAsync(data =>
        {
            tournamentLogosPath = data.GetResourceValueByID("PGATournamentLogosURL");
        });
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.M))
    //        UpdateMatchPlayScoreboard(new string[] { "R", "V1.0", "GLF", "PGAT", "PGAT", "MatchPlay", "PresidentsCup", "1", "Thursday Foursomes", "United States", "USA", "14", "International", "Intl", "10", "2017 - 04 - 20 16:47:33" });
    //    if (Input.GetKeyDown(KeyCode.N))
    //        UpdateMatchPlayScoreboard(new string[] { "R", "V1.0", "GLF", "PGAT", "PGAT", "MatchPlay", "PresidentsCup", "1", "Thursday Foursomes", "United States", "USA", "16", "International", "Intl", "12", "2017 - 04 - 20 16:47:33" });
    //}

    /*
    R\V1.1\GLF\PGAT\PGAT\MatchPlay\PresidentsCup\1\Thursday Foursomes\United States\USA\14\International\INT\12\2017-04-20 16:47:33
    FIELD
    Field 0:  Response packet type (always “R”)
    Field 1: Version Number of scoreboard client
    Field 2:  Sport identifier (always “GLF” for Golf)
    Field 3: Source for data,
    Field 4:  League identifier ( PGAT- PGA Tour)
    Field 5:  Tournament ID
    Field 6:  Tournament Name
    Field 7:  Round Number
    Field 8:  Round Format
    Field 9:  Team 1 Name
    Field 10:  Team 1 Short Name
    Field 11:  Team 1 Score
    Field 12:  Team 2 Name
    Field 13:  Team 2 Short Name
    Field 14:  Team 2 Score
    Field 15: UTC Time stamp
    */
    public void UpdateMatchPlayScoreboard(string[] scoreData)
    {
        //Setting elements that wont update only once
        if (firstRun)
        {
            firstRun = true;

            //Team Names
            team1Name.text = scoreData[9];
            team2Name.text = scoreData[12];

            //Team Logos
            _FlagsContainer.SetFlag(scoreData[10].ToUpper(), team1Image);
            _FlagsContainer.SetFlag(scoreData[13].ToUpper(), team2Image);

            //TournamentLogo
            tournamentLogo.SetTexture(tournamentLogosPath + scoreData[6] + ".png", true, FadeInLogo);
        }

        if (scoreData[11] != team1PreviousScore)
        {
            team1Score.text = scoreData[11];
            team1PreviousScore = scoreData[11];
        }

        if (scoreData[14] != team2PreviousScore)
        {
            team2Score.text = scoreData[14];
            team2PreviousScore = scoreData[14];
        }
    }

    public void FadeInLogo()
    {
        tournamentLogo.GetComponent<RawImage>().DOFade(1f, 1f);
    }
}
