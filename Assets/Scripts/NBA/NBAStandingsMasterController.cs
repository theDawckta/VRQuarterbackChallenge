using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using VOKE.VokeApp.DataModel;
using UnityEngine.Networking;



public class NBAStandingsMasterController : MonoBehaviour {

	public int loaded = 0;
	public float InitDelay = 2f;
	public bool debug = false;

	public GameObject standingsPrefab;
	public List<GameObject> panelContainers = new List<GameObject> ();
	public NBAStandingsController WesternController;
	public NBAStandingsController EasternController;

	private string _standingsUrl = "";
	private string _playoffsUrl = "";
	private JSONObject _JsonStandings;
	private VokeAppConfiguration _config;
	private RoundModel _seriesModel;


	// Use this for initialization
	void Start () {
		StartCoroutine (Init ());
	}

	IEnumerator Init()
	{
		//home is doing a lot of stuff, delay these coming in

		yield return new WaitForSeconds(InitDelay);
		_config = null;
		yield return AppConfigurationLoader.Instance.GetDataAsync(_ => _config = _);
		if (_config != null)
		{
			_standingsUrl = _config.GetResourceValueByID("NBAWesternStandingsUrl");
			_playoffsUrl = _config.GetResourceValueByID ("NBAPlayoffsUrl");

            if (_config.IsPlayoffs && _playoffsUrl != null && _playoffsUrl != "")
			{
				StartCoroutine (LoadPlayoffsUrl ());
			} else
			{
				if (_standingsUrl.IndexOf (".png") == -1 && _standingsUrl.IndexOf (".jpg") == -1)
				{
					StartCoroutine (LoadJSON ());
				} else
				{
					SetControllerImgs ();
				}
			}
		}
	}

	IEnumerator LoadPlayoffsUrl()
	{
		using (UnityWebRequest www = UnityWebRequest.Get (_playoffsUrl))
		{
			yield return www.SendWebRequest();
			if (www.isNetworkError || www.isHttpError)
			{
				Debug.Log ("Error in master controller");
			} else
			{
				_seriesModel = JsonUtility.FromJson<RoundModel>(www.downloadHandler.text);
				List<Round> westernSeries = new List<Round>();
				List<Round> easternSeries = new List<Round> ();
				for (int i = 0; i < _seriesModel.series.Count; i++)
				{
					Round curSeries = _seriesModel.series [i];
					if (curSeries.confName.ToLower () == "west" && !string.IsNullOrEmpty(curSeries.backgroundImg))
					{
						westernSeries.Add (curSeries);
					} else if (curSeries.confName.ToLower () == "east" && !string.IsNullOrEmpty(curSeries.backgroundImg))
					{
						easternSeries.Add (curSeries);
					}
				}
				if(WesternController != null)
					WesternController.SetPlayoffData(westernSeries, _seriesModel.westernRound);
				if(EasternController != null)
					EasternController.SetPlayoffData (easternSeries, _seriesModel.easternRound);
			}
		}
	}

	/// <summary>
	/// Attempt to set images on the controllers
	/// </summary>
	void SetControllerImgs()
	{
		//make sure we're a png or jpg
		string westernURL = _config.GetResourceValueByID("NBAWesternStandingsUrl");
		string easternURL = _config.GetResourceValueByID ("NBAEasternStandingsUrl");

		PopulateControllerImg (WesternController, westernURL);
		PopulateControllerImg (EasternController, easternURL);
	}

	void PopulateControllerImg(NBAStandingsController controller, string imgUrl)
	{
		if (controller != null)
		{
			if (imgUrl.IndexOf (".png") != -1 || imgUrl.IndexOf (".jpg") != -1)
			{
				Debug.Log ("PopulateControllerImg: " + controller + ", " + imgUrl);
				StartCoroutine (controller.LoadImg (imgUrl));
			}
		}
	}



	public IEnumerator LoadJSON()
	{
		WWW www = new WWW(_standingsUrl);
		yield return www;

		if (!String.IsNullOrEmpty(www.error))
		{
			Debug.LogErrorFormat("Error downloading '{0}': {1}", _standingsUrl, www.error);
			ShowError ();
			yield break;
		}
		string tempJsonStandings = www.text;
		_JsonStandings = JSONObject.Create (tempJsonStandings);
		_JsonStandings = _JsonStandings [1] [0] [2];
		foreach (GameObject container in panelContainers)
		{
			if (_standingsUrl.IndexOf (".png") != -1 || _standingsUrl.IndexOf (".jpg") != -1) {
				StartCoroutine (container.GetComponent<NBAStandingsController> ().LoadImg (_standingsUrl));
			} else {
				int division = 0;
				if (container.GetComponent<NBAStandingsController> ().StandingsUrlKey == "NBAWesternStandingsUrl")
				{
					division = 1;
				}
				container.GetComponent<NBAStandingsController> ().SetTeamStandings(_JsonStandings, division);
			}
		}
		yield return null;
	}


	/// <summary>
	/// Generic error for if loading URL fails for any reason
	/// </summary>
	void ShowError()
	{

	}

}
