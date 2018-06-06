using UnityEngine;
using System.Collections;
using System;
using VOKE.VokeApp.DataModel;
using System.Collections.Generic;

public class GamedayScoresController : MonoBehaviour {
	
	public ButtonController GamedayScoresButton;
	public GameObject LoadCircle;
	public List<GamedaySinglePanel> GamedaySinglePanels;
	public GameObject GamedayScoresPanel;
	public ButtonController CloseBtn;
	public RectTransform PanelBackground;
	public RectTransform Panel;
	public GameObject PagingHolder;
	public GameObject LeftButton;
	public GameObject RightButton;

	public GameObject Paging;
	public GameObject PageIndicator;

	private string _BaseballGamedayUrl = "http://54.68.21.16/vokemlbgamestore/api/1.3/games/getMlbGamedayScores";
	private bool _IsScoreOpen = false;
	private int _CurPage = 0;
	private int _TotalPages = 0;
	private JSONObject _JsonRoot;
	private int _ItemsPerPage = 6;
	private AudioController _AudioController;
	private Interactible _LeftButtonInteractible;
	private Interactible _RightButtonInteractible;
	private float _PagingHeight = 70.0f;	//how much spacing we'll give to bump the paging items down
	private List<PageIndicatorController> _pageIndicators = new List<PageIndicatorController>();
	//public ScoreboardData _scoreboardData;

	void Awake()
	{
		if (GamedayScoresButton == null)
			throw new Exception ("You must define a GamedayScoresButton for GamedayScoresController.");

		if (GamedayScoresPanel == null)
			throw new Exception ("You must define a GamedayScoresPanel for GamedayScoresController.");

		if (PanelBackground == null)
			throw new Exception ("You must define a PanelBackground for GamedayScoresController.");

		if (Panel == null)
			throw new Exception ("You must define a Panel for GamedayScoresController.");

		if (PagingHolder == null)
			throw new Exception ("You must define a Paging for GamedayScoresController.");

		if (LeftButton == null)
			throw new Exception ("You must define a LeftButton for GamedayScoresController.");

		if (RightButton == null)
			throw new Exception ("You must define a RightButton for GamedayScoresController.");

		HideLoadCircle ();
		GamedayScoresButton.gameObject.SetActive (false);
		GamedayScoresPanel.SetActive (false);
		PagingHolder.SetActive (false);

		_AudioController = Extensions.GetRequired<AudioController>();
		_LeftButtonInteractible = LeftButton.GetComponent<Interactible> ();
		_RightButtonInteractible = RightButton.GetComponent<Interactible> ();
	}

	private IEnumerator Start()
	{
		VokeAppConfiguration config = null;
		yield return AppConfigurationLoader.Instance.GetDataAsync(_ => config = _);

		if (config != null)
		{
			_BaseballGamedayUrl = config.GetResourceValueByID("BaseballGamedayUrl") ?? _BaseballGamedayUrl;
		}
	}

	public void Init(string statType)
	{
		if (statType.ToLower() == "baseball")
		{
			//if we're being inited, we should most likely show. Check to see if there are actually scores to display
			StartCoroutine(DownloadScores(true));
		}
	}


	IEnumerator DownloadScores(bool IsInit = false)
	{
		WWW www = new WWW(_BaseballGamedayUrl);
		yield return www;

		if (!String.IsNullOrEmpty(www.error))
		{
			Debug.LogErrorFormat("Error downloading '{0}': {1}", _BaseballGamedayUrl, www.error);
			yield break;
		}
	
		if (IsInit)
		{
			SetButtonVisibility (www.text);
		} else
		{
			ProcessJSON(www.text);
		}
		yield return null;
	}

	/// <summary>
	/// Check to see if we should allow the gameday button to show (if api is returning empty array, should not display
	/// </summary>
	/// <param name="jsonString">Json string.</param>
	void SetButtonVisibility(string jsonString)
	{
		_JsonRoot = new JSONObject (jsonString);
		if (_JsonRoot.type != JSONObject.Type.ARRAY)
		{
			Debug.Log ("JSON FORMAT ERROR: Gameday Scores root node not an array.");
			return;
		}

		//API returns empty array currently if there are no games; check to make sure there's a game before displaying
		if (_JsonRoot.Count > 0)
			GamedayScoresButton.gameObject.SetActive (true);
	}

	/// <summary>
	/// Process the JSON returned by the 
	/// </summary>
	/// <param name="jsonString">Json string.</param>
	void ProcessJSON(string jsonString)
	{
		_JsonRoot = new JSONObject(jsonString);
		if (_JsonRoot.type != JSONObject.Type.ARRAY)
		{
			Debug.Log ("JSON FORMAT ERROR: Gameday Scores root node not an array.");
			return;
		}

		HideLoadCircle ();
		GetPageScores ();
	}
		
	/// <summary>
	/// Set the scores for our page
	/// </summary>
	/// <param name="IsInitial">If set to <c>true</c> resize the panel when changing scores.</param>
	void GetPageScores(bool isInitial = true)
	{
		//check to see there are actual values we should populate our gameday scores with
		if (_JsonRoot.Count > 0)
		{
			int startPosition = _CurPage * _ItemsPerPage;
			int endPosition = startPosition + _ItemsPerPage;
			int totalItems = _JsonRoot.Count;

			Debug.Log (totalItems);

			_TotalPages = Mathf.CeilToInt((float)totalItems / (float)_ItemsPerPage);
			int counter = 0;
			GamedaySinglePanel curPanel;
			if(isInitial)
				InitPaging ();
			
			//do we need to show Paging
			SetPaging();

			for (int i = startPosition; i < endPosition; i++)
			{
				curPanel = GamedaySinglePanels [counter];
				CanvasGroup curPanelCanvas = curPanel.GetComponent<CanvasGroup> ();
				if (curPanel != null)
				{
					//if we have less than the full amount of items
					if (i >= totalItems)
					{
						curPanelCanvas.alpha = 0;
						if (isInitial)
						{
							curPanel.gameObject.SetActive (false);
						} 
					} else
					{
						curPanel.gameObject.SetActive (true);
						SetTeamScore (_JsonRoot [i], curPanel);
						curPanelCanvas.alpha = 1;
					}
					counter++;
				}
			}
			if(isInitial)
				StartCoroutine (SetBackgroundSize ());
		}
	}

	void SetPaging()
	{
		if (_JsonRoot.Count > _ItemsPerPage)
		{
			PagingHolder.SetActive (true);

			for (int j = 0; j < _pageIndicators.Count; j++)
			{
				if (_pageIndicators [j] != null)
				{
					_pageIndicators [j].TurnPageOff ();
					if (j == _CurPage)
						_pageIndicators [j].TurnPageOn ();
				}
			}
		} else
		{
			PagingHolder.SetActive (false);
		}
			
	}

	IEnumerator SetBackgroundSize()
	{
		yield return new WaitForEndOfFrame();
		PanelBackground.sizeDelta = new Vector2 (640, Panel.sizeDelta.y - 12);
		PagingHolder.GetComponent<RectTransform> ().anchoredPosition = new Vector2(PagingHolder.GetComponent<RectTransform> ().anchoredPosition.x, Panel.GetComponent<RectTransform> ().anchoredPosition.y - _PagingHeight);
	}

	/// <summary>
	/// populate each team score as data is passed in
	/// </summary>
	/// <param name="scoreJSON">Score JSO.</param>
	void SetTeamScore(JSONObject scoreJSON, GamedaySinglePanel curPanel)
	{
		//get the game status
		string gameStatus = GetGameStatusText(scoreJSON);
		//get our teams
		JSONObject teamsJSON = scoreJSON.GetField ("teams");
		//make sure we have two teams and game is an object (otherwise break)
		if (teamsJSON.type != JSONObject.Type.OBJECT || teamsJSON.Count != 2)
		{
			Debug.Log ("JSON FORMAT ERROR: Gameday Scores teams node not an object.");
			return;
		}	

		string team1Name = teamsJSON.keys [0];
		string team2Name = teamsJSON.keys [1];

		JSONObject team1JSON = teamsJSON [0];
		JSONObject team2JSON = teamsJSON [1];

        //default to showing record
        string team1Score = "-"; //team1JSON.GetField ("record_wins").str + "-" +  team1JSON.GetField ("record_losses").str;
		string team2Score = "-"; //= team2JSON.GetField ("record_wins").str + "-" +  team2JSON.GetField ("record_losses").str;;


		if (scoreJSON.GetField ("status") != null)
		{
			if (scoreJSON.GetField ("status").str.ToLower () == "pre-game" || scoreJSON.GetField ("status").str.ToLower () == "scheduled")
			{

			} else
			{
				team1Score = team1JSON.GetField ("game_runs").str;
				team2Score = team2JSON.GetField ("game_runs").str;
			}
		}

        if (string.IsNullOrEmpty(team1Score))
            team1Score = "0";
        if (string.IsNullOrEmpty(team2Score))
            team2Score = "0";

        curPanel.Team1Name.text = team1Name;
		curPanel.Team2Name.text = team2Name;
		curPanel.Team1Score.text = team1Score;
		curPanel.Team2Score.text = team2Score;
		curPanel.TimeText.text = gameStatus;

		//string teamTexture1 = null;
		//string teamTexture2 = null;

        //if(_scoreboardData != null)
        //{
        //	teamTexture1 = _scoreboardData.getTeamLogoById(team1JSON.GetField ("abbreviation").str);
        //	teamTexture2 = _scoreboardData.getTeamLogoById(team2JSON.GetField ("abbreviation").str);
        //}
        //else
        //{
        //	Debug.Log("ScoreboardData missing in GamedayScoresController, team images wil be blank");
        //}

		if(team1JSON.GetField("abbreviation") != null)
        	MLBLogosContainer.Instance.SetLogo(team1JSON.GetField("abbreviation").str,curPanel.Team1Image);
        
		if(team2JSON.GetField("abbreviation") != null)
			MLBLogosContainer.Instance.SetLogo(team2JSON.GetField("abbreviation").str, curPanel.Team2Image);

        //if (teamTexture1 != null)
        //{
        //	curPanel.SetCanvasInitialAlpha (curPanel.Team1Image);
        //	curPanel.Team1Image.SetTexture (teamTexture1, true);
        //}

        //if (teamTexture2 != null)
        //{
        //	curPanel.SetCanvasInitialAlpha (curPanel.Team2Image);
        //	curPanel.Team2Image.SetTexture (teamTexture2, true);
        //}

    }

	/// <summary>
	/// Return the status text to display in the top of the score pane. (TODO: if game hasn't started, does this return time?)
	/// </summary>
	/// <returns>The game status text as a strong.</returns>
	/// <param name="scoreJSON">Score JSON.</param>
	string GetGameStatusText(JSONObject scoreJSON)
	{
		string gameStatus = "";
		int curInning = 0;
		//make sure the API is returning an inning
		if (scoreJSON.GetField ("inning") != null)
		{
			curInning = int.Parse (scoreJSON.GetField ("inning").str);
		}

		//make sure the API is returning a status
		if (scoreJSON.GetField ("status") != null)
		{
			if (scoreJSON.GetField ("status").str.ToLower () == "final")
			{
				gameStatus = "Final";
				//if we went into extra innings, include that text
				if (curInning > 9)
				{
					gameStatus += "/" + scoreJSON.GetField ("inning").str;
				}
			} else if (scoreJSON.GetField ("status").str.ToLower () == "pre-game")
			{
				gameStatus = "";
			} else
			{
				//parse our inning string int, and make sure the int actually makes sense
				if (curInning >= 1 && curInning < 30)
				{
					gameStatus = CreateOrdinal (curInning);
				}
			}
		}
		return gameStatus;
	}

	string CreateOrdinal(int num)
	{
		if( num <= 0 ) return num.ToString();

		switch(num % 100)
		{
		case 11:
		case 12:
		case 13:
			return num + "th";
		}

		switch(num % 10)
		{
		case 1:
			return num + "st";
		case 2:
			return num + "nd";
		case 3:
			return num + "rd";
		default:
			return num + "th";
		}
	}

	void ShowLoadCircle()
	{
		if (LoadCircle != null)
		{
			LoadCircle.SetActive (true);
		}
	}

	void HideLoadCircle()
	{
		if (LoadCircle != null)
		{
			LoadCircle.SetActive (false);
		}
	}

	void ShowGamedayPanel()
	{
		GamedayScoresPanel.SetActive (true);
	}

	void HideGamedayPanel()
	{
		GamedayScoresPanel.SetActive (false);
	}

	/// <summary>
	/// Close our scores/kill any prefabs on the stage
	/// </summary>
	void CloseScores()
	{
		HideLoadCircle ();
		HideGamedayPanel ();
		_AudioController.PlayAudio(AudioController.AudioClips.SmallClick);
		//remove any scores from our container
//		foreach(Transform child in GamedayScoresPanelHolder.transform)
//		{
//			GameObject.Destroy(child.gameObject);
//		}
		_IsScoreOpen = false;
	}


	void InitPaging()
	{
		foreach (PageIndicatorController pageIndicator in _pageIndicators)
		{
			Destroy(pageIndicator.gameObject);
		}
		_pageIndicators.Clear();


		for (int i = 0; i < _TotalPages; i++)
		{
			GameObject pageIndicator = (GameObject)Instantiate (PageIndicator);
			pageIndicator.transform.SetParent (Paging.transform, false);
			_pageIndicators.Add(pageIndicator.GetComponent<PageIndicatorController>());
		}
	}

	/// <summary>
	/// Display our scores if there are scores to display
	/// </summary>
	void OpenScores()
	{
		if (!String.IsNullOrEmpty (_BaseballGamedayUrl))
		{
			_CurPage = 0;
			ShowLoadCircle ();
			ShowGamedayPanel ();
			StartCoroutine(DownloadScores());
			_AudioController.PlayAudio(AudioController.AudioClips.GenericClick);
            EventManager.Instance.GamedayScoresOpenedEvent();
		}
		_IsScoreOpen = true;
	}

	void HandleLeftClick()
	{
		if(_CurPage <= 0){
			_CurPage = _TotalPages - 1;
		}else{
			_CurPage--;
		}
		GetPageScores (false);
	}

	void HandleRightClick()
	{
		if(_CurPage >= _TotalPages - 1){
			_CurPage = 0;
		}else{
			_CurPage = _CurPage + 1;
		}
		GetPageScores (false);
	}

	/// <summary>
	/// Gameday scores button clicked, trigger scores
	/// </summary>
	/// <param name="sender">Sender.</param>
	void OnGamedayScoresClicked(object sender)
	{
		if (_IsScoreOpen)
		{
			CloseScores ();
		} else
		{
			OpenScores ();
		}
	}

	void OnCloseBtnClicked(object sender)
	{
		if (_IsScoreOpen)
		{
			CloseScores ();
		}
	}

	void OnEnable()
	{
		GamedayScoresButton.OnButtonClicked += OnGamedayScoresClicked;
		CloseBtn.OnButtonClicked += OnCloseBtnClicked;
		_LeftButtonInteractible.OnClick += HandleLeftClick;
		_RightButtonInteractible.OnClick += HandleRightClick;
        EventManager.OnPanelOpenComplete += OnPanelOpened;
	}

	void OnDisable()
	{
		GamedayScoresButton.OnButtonClicked -= OnGamedayScoresClicked;
		CloseBtn.OnButtonClicked -= OnCloseBtnClicked;
		_LeftButtonInteractible.OnClick += HandleLeftClick;
		_RightButtonInteractible.OnClick += HandleRightClick;
        EventManager.OnPanelOpenComplete -= OnPanelOpened;
    }

    /// <summary>
    /// EventHandler for Panel opened event
    /// </summary>
    void OnPanelOpened()
    {
        //When Panels like Related Content or watch next are opened, we need to close Gameday if its open
        if (_IsScoreOpen)
            CloseScores();
    }
}
