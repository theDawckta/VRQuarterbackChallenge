using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VOKE.VokeApp.DataModel;

public class LeaderBoardController : MonoBehaviour
{

	public GameObject LeaderboardRowHolder;
	public LeaderboardRow LeaderBoardRowPrefab;
	public FlagsContainer _FlagsContainer;

    private bool isAnimating = false;
    private Vector3 originalScale;

	private string _playerURL = "http://34.209.116.203/vokegolfplayerinfo/api/1.0/GetPlayerInfo.php?plrNum=";


	private IEnumerator Start()
	{

		VokeAppConfiguration config = null;
		yield return AppConfigurationLoader.Instance.GetDataAsync(_ => config = _);

		if (config != null)
		{
			_playerURL = config.GetResourceValueByID("PGAPlayerUrlFormat") ?? _playerURL;
		}
	}

    private void OnEnable()
    {
        EventManager.OnBackBtnClickEvent += HideLeaderBoard;
    }

    private void OnDisable()
    {
        EventManager.OnBackBtnClickEvent -= HideLeaderBoard;
    }

    public void HideLeaderBoard()
    {
        gameObject.SetActive(false);
    }

    //if we want to show pitchers, we'd need to pass a boolean here
    public void StartMakePGAStats(GolfGameStatistics pgaStats)
	{
		StartCoroutine(MakePGAStats(pgaStats.PGAPlayers));
	}

	IEnumerator MakePGAStats(IList<GolfPlayerStatistics> players)
	{
        //clear our player rows here
        foreach (Transform child in LeaderboardRowHolder.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        //recreate our player rows
        for (int i=0;i<players.Count;i++)
		{
			CreatePGAPlayerRow(players[i]);
		}
		yield return null;
	}

	void CreatePGAPlayerRow(GolfPlayerStatistics player) {

        LeaderboardRow leaderboardRow = Instantiate (LeaderBoardRowPrefab) as LeaderboardRow;
		leaderboardRow.transform.SetParent (LeaderboardRowHolder.transform, false);

		_FlagsContainer.SetFlag(player.Country, leaderboardRow.FlagImage);

		leaderboardRow.TxtName.text = player.Name;
       // leaderboardRow.TxtCountry.text = player.Country;
		leaderboardRow.TxtRank.text = player.Position;
		leaderboardRow.TxtTotal.text = player.Total;
		leaderboardRow.TxtThru.text = player.Thru;
		leaderboardRow.TxtRd.text = player.Round;

		//get the player country by running through the player IDs. 
		//TODO: this call should not be something the front-end is responsible for
		//string playerID = player.PlayerID;
		//string url = String.Format (_playerURL, playerID);
	}
}
