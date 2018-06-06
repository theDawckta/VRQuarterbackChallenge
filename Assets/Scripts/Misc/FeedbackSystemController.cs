using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class FeedbackSystemController : Singleton<FeedbackSystemController>
{

    private bool throughBackButton;

    private void Awake()
    {
        //DontDestroyOnLoad(this);
    }

    /// <summary>
    /// Checks the logic as to whether the Feedback form Popup should be shown
    /// </summary>
    /// <param name="fromBackButtonPress">Whether the check is from a back button press</param>
    public void CheckFeedbackFormState(bool fromBackButtonPress)
    {
#if HideFeedbackButton
        return;
#endif
        //Debug.Log("Through back or Home Button :;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;; " + fromBackButtonPress);

        throughBackButton = fromBackButtonPress;

        if (PlayerPrefs.GetInt("LastVideoDuration") >= 3 && PlayerPrefs.GetString("FeedbackIncrementDate", "") != DateTime.Now.Date.ToShortDateString())
        {
            PlayerPrefs.SetString("FeedbackIncrementDate", DateTime.Now.Date.ToShortDateString());
            PlayerPrefs.SetInt("FeedbackVideosWatched", PlayerPrefs.GetInt("FeedbackVideosWatched", 0) + 1);
            PlayerPrefs.SetInt("LastVideoDuration", 0);
            PlayerPrefs.SetString("FeedbackVideos", PlayerPrefs.GetString("FeedbackVideos", "") + "," + PlayerPrefs.GetString("LastVideoSeen"));
            //Debug.Log("Incremented FeedbackVideosWatched ----------------------------------------");
        }

        if (PlayerPrefs.GetInt("FeedbackVideosWatched") > 1)    //Recurring playthrough on the second day
        {
            //Show Feedback Popup
            //Debug.Log("Showing Feedback Popup ---------------------------------------------------------------------------------------");
            PopupController.Instance.Setup("Help us improve! Would you like to give us feedback?")
                .WithButton("YES", OnFeedbackPopupYes, false)
                .WithButton("LATER", OnFeedbackPopupLater)
                .WithButton("NO", OnFeedbackPopupNo)
                .Show();
        }
    }

    /// <summary>
    /// Shows the feedback Popup irrespective of conditions
    /// </summary>
    public void TriggerGlobalFeedback()
    {
        PopupController.Instance.Setup("Help us improve! Would you like to give us feedback?")
            .WithButton("YES", OnFeedbackPopupYes, false)
            .WithButton("NO")
            .Show();
    }

    private void OnFeedbackPopupNo()
    {
        //The feedback popup will never show again
        PlayerPrefs.SetInt("FeedbackSystemDisabled", 1);
    }

    private void OnFeedbackPopupLater()
    {
        PlayerPrefs.SetInt("FeedbackVideosWatched", 0);
    }

    private void OnFeedbackPopupYes()
    {
        GlobalVars.Instance.feedbackPopupShown = true;
        PlayerPrefs.SetInt("FeedbackSystemDisabled", 1);
        PopupController.Instance.HidePopup(false);

        //Show dismissable popup to remove headset
        PopupController.Instance.ShowPopup("Please remove your headset to fill out the survey. The application will exit.");

        Analytics.CustomEvent("OnFeedbackYes", new Dictionary<string, object> {
                { "FromBackButton", throughBackButton },
                { "VideoDetails", PlayerPrefs.GetString("FeedbackVideos") }
            });
    }
}
