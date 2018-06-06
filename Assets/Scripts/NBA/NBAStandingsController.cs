using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VOKE.VokeApp.DataModel;
using System;
using DG.Tweening;

public class NBAStandingsController : MonoBehaviour {

	public NBAStandingsMasterController masterController;
	public TextMeshProUGUI heading;
	public string StandingsUrlKey = "NBAStandingsUrl";
	public GameObject standingsPrefab;
	public GameObject standingsPanel;
	public GameObject standingsRow;
	public float fadeDelay;
	public float InitDelay = 4.0f;
	public NBAPlayoffsPanelController playoffsPrefab;

	private GameObject rowHolder;
	private JSONObject _JsonRoot;
	private JSONObject _JsonStandings;
	private NBAPlayoffsPanelController _playoffsPanel;

	IEnumerator FadeIn()
	{
		yield return new WaitForEndOfFrame ();
		CanvasGroup canvas = standingsPanel.GetComponent<StandingsPanelController> ().standingsCanvas;
		if (canvas != null)
		{
			canvas.alpha = 0.0f;
			canvas.DOFade (1.0f, fadeDelay);
		}
	}


	/// <summary>
	/// Load in an image from
	/// </summary>
	public IEnumerator LoadImg(string imgUrl)
	{
		using (WWW www = new WWW(imgUrl))
		{
			yield return www;
			if (!String.IsNullOrEmpty(www.error))
			{
				Debug.LogErrorFormat("Error downloading '{0}': {1}", imgUrl, www.error);
				ShowError();
				yield break;
			}

			Debug.Log ("imgUrl: " + imgUrl);
			var unwrapper = TextureUnwrapperQueue.Instance.CreateTextureUnwrapper(www);
			yield return unwrapper.WaitForUnwrapCompletion();

			masterController.loaded++;
			while (masterController.loaded < 2)
			{
				yield return new WaitForSeconds(0f);
			}
			standingsPanel = Instantiate(standingsPrefab, gameObject.transform);
			if (standingsPanel.GetComponent<StandingsPanelController> ().image.GetComponent<RawImage> () != null)
			{
				standingsPanel.GetComponent<StandingsPanelController> ().image.GetComponent<RawImage> ().gameObject.SetActive (true);
				standingsPanel.GetComponent<StandingsPanelController> ().image.GetComponent<RawImage> ().texture = unwrapper.Texture;
			}
		}

		StartCoroutine (FadeIn ());
	}

	void ClearPanel()
	{
		//clear out the old panels
		foreach (Transform child in this.gameObject.transform)
		{
			GameObject.Destroy(child.gameObject);
		}
	}

	/// <summary>
	/// Instantiate prefabs for the playoff panels
	/// </summary>
	/// <param name="conferenceSeries">Conference series.</param>
	/// <param name="curRound">Current round to display, not zero indexed.</param>
	public void SetPlayoffData(List<Round> conferenceSeries, int curRound)
	{
		//clean up the current panels if needed
		ClearPanel();

		bool IsEast = false;
		if (StandingsUrlKey == "NBAEasternStandingsUrl")
			IsEast = true;

		if(playoffsPrefab != null)
			_playoffsPanel = Instantiate(playoffsPrefab, gameObject.transform) as NBAPlayoffsPanelController;
		
		_playoffsPanel.InitPanel (conferenceSeries, curRound, IsEast);
	}


	/// <summary>
	/// Team JSON is loaded, display
	/// </summary>
	public void SetTeamStandings(JSONObject standingsJSON, int division)
	{
		standingsPanel = Instantiate (standingsPrefab, gameObject.transform);
		standingsPanel.GetComponent<StandingsPanelController>().image.SetActive (false);
		if (division == 1) {
			standingsPanel.GetComponent<StandingsPanelController>().heading.text = "WESTERN CONFERENCE";
			standingsPanel.GetComponent<StandingsPanelController>().westBackground.SetActive (true);
		} else {
			standingsPanel.GetComponent<StandingsPanelController>().heading.text = "EASTERN CONFERENCE";
			standingsPanel.GetComponent<StandingsPanelController>().eastBackground.SetActive (true);
		}
		JSONObject divisionJSON = standingsJSON [division];

		int count = 0;

		foreach(JSONObject teamJSON in divisionJSON)
		{
			count++;
			string teamID = teamJSON.GetField ("teamId").str;
			string gamesBehind = teamJSON.GetField ("gamesBehind").str;
			string teamName;
			standingsPanel.GetComponent<StandingsPanelController> ().teamIDTable.TryGetValue (teamID, out teamName);
			int confWins = Int32.Parse(teamJSON.GetField ("confWin").str);
			int confLoss = Int32.Parse(teamJSON.GetField ("confLoss").str);
			int overallWins = Int32.Parse(teamJSON.GetField ("win").str);
			int overallLoss = Int32.Parse(teamJSON.GetField ("loss").str);
			string confRecord = confWins.ToString() + "-" + confLoss.ToString();
			string overallRecord = overallWins.ToString() + "-" + overallLoss.ToString();
			bool clinched = false;
			if (teamJSON.GetField("clinchedPlayoffsCode").str == "P" ||
				(masterController.debug && count == 1))
				clinched = true;
			GameObject row = Instantiate (standingsRow, standingsPanel.GetComponent<StandingsPanelController>().rowHolder.transform);
			row.GetComponent<NBAStandingsRowController> ().SetUpRow (teamName, gamesBehind, overallRecord, clinched);
		}
		StartCoroutine (FadeIn ());
	}


	/// <summary>
	/// Generic error for if loading URL fails for any reason
	/// </summary>
	void ShowError()
	{

	}

}
