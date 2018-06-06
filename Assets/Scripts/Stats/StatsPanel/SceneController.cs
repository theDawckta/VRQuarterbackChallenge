using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour 
{
    public PlayerStatsController PlayerStats;

	private void Start()
	{
        PlayerStats.StartCoroutine(PlayerStats.GetPlayerStats("http://intelvoke-staging.popworldwide.com/nba/PlayerStatsExample.json"));
	}

    void PlayerStatsError(string errorText)
    {
        
    }

    void OnEnable()
    {
        PlayerStats.OnPlayerStatsDownloadError += PlayerStatsError;
    }

    void OnDisable()
    {
        PlayerStats.OnPlayerStatsDownloadError -= PlayerStatsError;
    }
}
