using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VOKE.VokeApp.DataModel;
using VOKE.VokeApp.Util;
using TMPro;

public class ScoreboardController : MonoBehaviour
{
    private string mSportType;
    private string mLeagueType;

    private ScoreboardView mScoreboardView;
    private ScoreboardDataModel mScoreboardDataModel;

    private VokeAppConfiguration mAppConfig;

    private Queue<ScoreboardEventArgs> mScoreboardEventsQueue;

    public JackalopeMediaPlayer MediaPlayer;

    void Awake()
    {
         mScoreboardEventsQueue = new Queue<ScoreboardEventArgs>();
    }

    IEnumerator Start()
    {
        HideAllScoreboards();

        yield return AppConfigurationLoader.Instance.GetDataAsync(data =>  
        {
            mAppConfig = data;
        });


		DataCursorComponent.Instance.GetCursorAsync(cursor => {
			if(cursor.CurrentVideo != null)
			{
				if(cursor.CurrentVideo.ScoreboardPosition != null)
					SetScoreboardPosition(cursor.CurrentVideo.ScoreboardPosition);
			}
		});

//		EnvironmentConfigurationLoader.Instance.GetDataAsync(environmentData =>
//		{
//			environmentData.GetScoreboardPosition();
//			ScoreboardPosition scoreboardPos = environmentData.GetScoreboardPosition();
//			if(scoreboardPos != null)
//				SetScoreboardPosition(scoreboardPos);
//		});
			
        if (!MediaPlayer)
        {
            Debug.LogError("The Media Player should be assigned in order to get the scoreboard metadata.");
        }
     }

    void OnEnable()
    {
        if (MediaPlayer)
            MediaPlayer.OnUserDefinedTextMetadata += OnUserDefinedTextMetadata;
    }

    void OnDisable()
    {
        if (MediaPlayer)
            MediaPlayer.OnUserDefinedTextMetadata -= OnUserDefinedTextMetadata;
    }

    void Update()
    {
        // Check if there is any job to update the scoreboard UI
        // Only handle 5 events in a frame
        for (int i = 0; i < 5 && mScoreboardEventsQueue.Count > 0; i++)
        {
            // Get the latest message
            ScoreboardEventArgs sbe = mScoreboardEventsQueue.Dequeue();

            // Intercept some messages
            if (sbe.SportType == "BSB" && sbe.PropertyName == "BatterID")
            {
                EventManager.Instance.PitcherBatterChanged(sbe.PropertyValue, ((BaseballScoreboardDataModel)mScoreboardDataModel).IsTopOfInning?"T":"B", GridStats.IconType.Batter);
            }
            else if (sbe.SportType == "BSB" && sbe.PropertyName == "PitcherID")
            {
                // TODO: Update the stats board
                EventManager.Instance.PitcherBatterChanged(sbe.PropertyValue, ((BaseballScoreboardDataModel)mScoreboardDataModel).IsTopOfInning ? "T" : "B", GridStats.IconType.Pitcher);
            }

            // Update the Scoreboard UI according to the message type
            if (mScoreboardView != null)
            {
                // TODO: Check the sportType, leagueType, and matchId

                // Update the score data
                mScoreboardView.UpdateScoreboardData(sbe.PropertyName, sbe.PropertyValue);
            }
        }
    }

	/// <summary>
	/// Position the scoreboard based off of XML values if available
	/// </summary>
	void SetScoreboardPosition(ScoreboardPosition scoreboardPos)
	{

		Vector3 curLocalPosition = this.gameObject.transform.localPosition;

		float xPos = 0;
		float yPos = 0;
		float zPos = 0;

		if (scoreboardPos.OffsetX != null)
		{
			float.TryParse (scoreboardPos.OffsetX, out xPos);
			curLocalPosition.x += xPos;
		}
		if (scoreboardPos.OffsetY != null)
		{
			float.TryParse (scoreboardPos.OffsetY, out yPos);
			curLocalPosition.y += yPos;
		}
		if (scoreboardPos.OffsetZ != null)
		{
			float.TryParse (scoreboardPos.OffsetZ, out zPos);
			curLocalPosition.z += zPos;
		}

		this.gameObject.transform.localPosition = curLocalPosition;
	}

    public void HideAllScoreboards()
    {
        // Disable all the first-depth children
        foreach (Transform t in gameObject.transform)
        {
            //Debug.Log("ScoreboardController::HideAllScoreboards(): Child Name= " + t.name);
            t.gameObject.SetActive(false);
        }
    }

    private void OnUserDefinedTextMetadata(string id, string value, string description)
    {
        Debug.Log("OnUserDefinedTextMetadata: Scoreboard Data = " + description);
        string[] scoreFields = null;
        bool isValid = false;

        // Validate the score data       
        if (!string.IsNullOrEmpty(description))
        {
            scoreFields = description.Split('\\');
            if (scoreFields.Length >= 4)   // We are expecting more than 4 fields
                isValid = true;
        }

        if (!isValid)
        {
            Trace.Log("OnUserDefinedTextMetadata::The socre data is invalid. No parsed data available. Check metadata format");
            HideAllScoreboards(); 
            return;
        }

        int sportTypeIndex = 1;
        if (scoreFields[1].Contains(".") || scoreFields[1].StartsWith("V"))
        {
            sportTypeIndex = 2;
        }

        // Get the current sport type and league type. Also, need to handle the exception like SMT
        string sportType;
        string leagueType;
        if (scoreFields[sportTypeIndex] == "1")
        {
            sportType = "BKB";
            leagueType = "NCAA";
        }
        else
        {
            sportType = scoreFields[sportTypeIndex];
            leagueType = scoreFields[sportTypeIndex + 1];
        }

        // Instantiate View and Model objects for a sport-specific scoreboard
        if (mSportType != sportType)
        {
            mSportType = sportType;
            mLeagueType = leagueType;

            if (mScoreboardView != null)
            {
                mScoreboardView.HideScoreboardUI();
            }
            mScoreboardView = createScoreboardView(mSportType, mLeagueType);

            if (mScoreboardDataModel != null)
            {
                // TODO: Do something!!!
            }
            mScoreboardDataModel = createScoreboardDataModel(mSportType, mLeagueType);
        }

        // Update the scoreboard data model and the scoreboard view will be updated from the Update() function
        if (mScoreboardDataModel != null)
        {
            mScoreboardDataModel.UpdateScoreboardData(scoreFields);

            // The timestamp will be used by the stats controller
            GlobalVars.Instance.CurrentTimestamp = mScoreboardDataModel.UTCTime;
        }
    }

    private ScoreboardView createScoreboardView(string sportType, string leagueType)
    {
		ScoreboardView view = new BasketballScoreboardView();
        
        if (view != null)
            view.LoadRequiredComponents(gameObject);

        return view;
    }

    private ScoreboardDataModel createScoreboardDataModel(string sportType, string leagueType)
    {
		ScoreboardDataModel model = new BasketballScoreboardDataModel();

        // Set properties according to the sport and league type
        if (model != null)
        {
            model.ScoreboardDataChanged += new EventHandler(OnScoreboardDataChangedEventHandler);

            if (model is BallGameScoreboardDataModel)
            {
                ((BallGameScoreboardDataModel)model).TeamInfoUrl = mAppConfig.GetURLForTeamInfos(mSportType, mLeagueType);
                ((BallGameScoreboardDataModel)model).LeagueLogoUrl = mAppConfig.GetURLForLeagueLogo(mLeagueType);
                ((BallGameScoreboardDataModel)model).PlayerInfoUrl = mAppConfig.GetURLForPlayerInfos(mSportType, mLeagueType);
            }
        }
                
        return model;
    }
        
    private void OnScoreboardDataChangedEventHandler(System.Object sender, EventArgs e)
    {
        ScoreboardEventArgs sbe = e as ScoreboardEventArgs;
       
        // Insert the message into the queue which is dequeued in the Update() function
        mScoreboardEventsQueue.Enqueue(sbe);
    }
}
