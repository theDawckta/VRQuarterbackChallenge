using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VOKE.VokeApp.DataModel;
using System;

public class FPOScoreboardTest : MonoBehaviour {

	public DynamicTexture AwayLogo;
	public DynamicTexture HomeLogo;
	public float InitDelay = 8.0f;

	private string _NBATeamsURL = "";
	private JSONObject _NBATeamsRoot;
	private int _TotalTeams;
	private int _CurTeam = 0;

	// Use this for initialization
	void Start () {
		if (this.gameObject.activeInHierarchy)
			StartCoroutine (Init ());
		
	}

	IEnumerator Init()
	{
		yield return new WaitForSeconds(InitDelay);
		VokeAppConfiguration config = null;
		yield return AppConfigurationLoader.Instance.GetDataAsync(_ => config = _);
		if (config != null)
		{
			_NBATeamsURL = config.GetResourceValueByID("NBATeamsUrl");
			StartCoroutine (LoadJSON ());
		}
	}

	public IEnumerator LoadJSON()
	{
		WWW www = new WWW(_NBATeamsURL);
		yield return www;

		if (!String.IsNullOrEmpty(www.error))
		{
			Debug.LogErrorFormat("Error downloading '{0}': {1}", _NBATeamsURL, www.error);
			ShowError ();
			yield break;
		}
		string tempJsonStandings = www.text;
		_NBATeamsRoot = JSONObject.Create (tempJsonStandings);
		_NBATeamsRoot = _NBATeamsRoot.GetField ("team_info");
		_TotalTeams = _NBATeamsRoot.Count;

		if (_CurTeam < _TotalTeams - 1)
			PopulateTeam ();


//		_JsonStandings = JSONObject.Create (tempJsonStandings);
//		_JsonStandings = _JsonStandings [1] [0] [2];
//		foreach (GameObject container in panelContainers)
//		{
//			if (_standingsUrl.IndexOf (".png") != -1 || _standingsUrl.IndexOf (".jpg") != -1) {
//				StartCoroutine (container.GetComponent<NBAStandingsController> ().LoadImg ());
//			} else {
//				int division = 0;
//				if (container.GetComponent<NBAStandingsController> ().StandingsUrlKey == "NBAWesternStandingsUrl")
//				{
//					division = 1;
//				}
//				container.GetComponent<NBAStandingsController> ().SetTeamStandings(_JsonStandings, division);
//			}
//		}
//		yield return null;
	}

	void PopulateTeam()
	{

		string awayTeamLogo = _NBATeamsRoot [_CurTeam].GetField ("logoAway").str;
		string homeTeamLogo = _NBATeamsRoot [_CurTeam].GetField ("logoHome").str;

		if (!string.IsNullOrEmpty (awayTeamLogo))
			AwayLogo.SetTexture (awayTeamLogo, true);

		if (!string.IsNullOrEmpty (homeTeamLogo))
			HomeLogo.SetTexture (homeTeamLogo, true);

		Debug.Log ("awayTeamLogo: " + awayTeamLogo);
		Debug.Log ("homeTeamLogo: " + homeTeamLogo);

		StartCoroutine (ReloadTeams ());
	}

	IEnumerator ReloadTeams()
	{
		yield return new WaitForSeconds (InitDelay);
		_CurTeam++;
		if (_CurTeam < _TotalTeams)
			PopulateTeam ();
	}

	void ShowError()
	{

	}

}
