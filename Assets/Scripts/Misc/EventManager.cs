using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class EventManager : MonoBehaviour
{
	public delegate void BlackoutFadeComplete();
	public static event BlackoutFadeComplete OnBlackoutFadeComplete;

	public delegate void BlackoutFadeInAction();
	public static event BlackoutFadeInAction OnBlackoutFadeInEvent;
	public static event BlackoutFadeInAction OnLightsOffEvent;
	public static event BlackoutFadeInAction OnLightsOnEvent;

	public delegate void BackBtnClickAction();
	public static event BackBtnClickAction OnBackBtnClickEvent;

	public delegate void HomeBtnClickAction();
	public static event HomeBtnClickAction OnHomeBtnClickEvent;

	public delegate void FeedbackBtnClickAction();
	public static event FeedbackBtnClickAction OnFeedbackBtnClickEvent;

	public delegate void ContentTileMenuPagingBtnClickAction();
	public static event ContentTileMenuPagingBtnClickAction OnContentTileMenuPagingBtnClickEvent;

	public delegate void SwitchCameraAction(string mediaUrl);
	public static event SwitchCameraAction OnSwitchCameraEvent;

	public delegate void VideoBufferingAction();
	public static event VideoBufferingAction OnVideoBufferingEvent;

	public delegate void VideoReadyToPlayAction();
	public static event VideoReadyToPlayAction OnVideoReadyToPlayEvent;

	public delegate void VideoPlayAction();
	public static event VideoPlayAction OnVideoPlayEvent;

	public delegate void VideoPositionChangeAction();
	public static event VideoPositionChangeAction OnVideoPositionChangedEvent;

	public delegate void VideoErrorAction(string message);
	public static event VideoErrorAction OnVideoErrorEvent;

	public delegate void VideoEndAction();
	public static event VideoEndAction OnVideoEndEvent;

	public delegate void AnalyticsStatGazeAction(string message);
	public static event AnalyticsStatGazeAction OnAnalyticsStatsGazeEvent;

	public delegate void DrawTilesCompleteAction(bool isHome);
	public static event DrawTilesCompleteAction OnDrawTilesComplete;

	public delegate void DrawContentMenuTilesAction(ContentViewModel curContentViewModel);
	public static event DrawContentMenuTilesAction OnDrawContentMenuTilesComplete;

	public delegate void BackgroundAudioTriggerAction(string backgroundAudioStr, bool isHome);
	public static event BackgroundAudioTriggerAction OnBackgroundAudioTrigger;

	public delegate void BackgroundImageTriggerAction(string backgroundImgStr);
	public static event BackgroundImageTriggerAction OnBackgroundImageTrigger;

	public delegate void TileClickAction();
	public static event TileClickAction OnTileClickAction;

	public delegate void SwitchToConsumptionAction();
	public static event SwitchToConsumptionAction OnSwitchToConsumption;

	public delegate void SwitchToShoulderContentAction();
	public static event SwitchToShoulderContentAction OnSwitchToShoulderContent;

	public delegate void HardwareAudioMuteAction(bool isMute);
	public static event HardwareAudioMuteAction OnHardwareAudioIsMute;

	public delegate void DisableUserClickAction();
	public static event DisableUserClickAction OnDisableUserClick;

	public delegate void EnableUserClickAction();
	public static event EnableUserClickAction OnEnableUserClick;

	public delegate void PanelOpenAction();
	public static event PanelOpenAction OnPanelOpenComplete;

	public delegate void PanelCloseAction();
	public static event PanelCloseAction OnPanelCloseComplete;

	public delegate void GamedayScoresOpenedAction();
	public static event GamedayScoresOpenedAction OnGamedayScoresOpened;

	public delegate void EnvironmentLoadCompleteAction();
	public static event EnvironmentLoadCompleteAction OnEnvironmentLoadComplete;

	public delegate void StatsToggledAction(string toggleState);
	public static event StatsToggledAction OnStatsToggled;

	public delegate void ControllerSwipeAction(string swipeDirection);
	public static event ControllerSwipeAction OnControllerSwipe;

	public delegate void ChannelTriggerAction();
	public static event ChannelTriggerAction OnChannelTrigger;

	public delegate void PasswordAction();
	public static event PasswordAction OnPasswordCheck;
	public static event PasswordAction OnPasswordCorrect;
	public static event PasswordAction OnPasswordInCorrect;

	public delegate void PinpadAction();
	public static event PinpadAction OnPinpadOpen;
	public static event PinpadAction OnPinpadClose;

	public delegate void PitcherBatterResetAction();
	public static event PitcherBatterResetAction OnPitcherBatterReset;

	public delegate void PitcherBattterChanged(string playerID, string inning, GridStats.IconType iconType);
	public static event PitcherBattterChanged OnPitcherBattterChanged;

	public delegate void TeamNameReceived(bool isHome, string teamCode);
	public static event TeamNameReceived OnTeamNameReceived;

	public delegate void BannerAdStart();
	public static event BannerAdStart OnBannerAdStart;

	public delegate void BannerAdStop();
	public static event BannerAdStop OnBannerAdStop;

	public delegate void PopupShown();
	public static event PopupShown OnPopupShown;

	public delegate void CameraPivotAction();
	public static event CameraPivotAction OnAllowCameraPivot;

	public delegate void VideoTriggerAction(ContentViewModel contentViewModel);
	public static event VideoTriggerAction OnVideoTrigger;

	public delegate void LogStringAction(string objectToLog);
	public static event LogStringAction OnLogStringEvent;

	private static EventManager _Instance;

	public static EventManager Instance
	{
		get
		{
			if (!_Instance)
			{
				_Instance = FindObjectOfType(typeof(EventManager)) as EventManager;

				if (!_Instance)
				{
					Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
				}
			}

			return _Instance;
		}
	}

	public void BlackoutFadeCompleteEvent()
	{
		if (OnBlackoutFadeComplete != null)
			OnBlackoutFadeComplete();
	}

	public void BackBtnClickEvent()
	{
		if (OnBackBtnClickEvent != null)
			OnBackBtnClickEvent();
	}

	public void HomeBtnClickEvent()
	{
		if (OnHomeBtnClickEvent != null)
			OnHomeBtnClickEvent();
	}

	public void FeedbackBtnClickEvent()
	{
		if (OnFeedbackBtnClickEvent != null)
			OnFeedbackBtnClickEvent();
	}

	public void ContentTileMenuPagingBtnClickEvent()
	{
		if (OnContentTileMenuPagingBtnClickEvent != null)
			OnContentTileMenuPagingBtnClickEvent();
	}

	public void CameraSwitchEvent(string mediaUrl)
	{
		if (OnSwitchCameraEvent != null)
		{
			OnSwitchCameraEvent (mediaUrl);
		}
	}

	public void VideoBufferingEvent()
	{
		if (OnVideoBufferingEvent != null)
			OnVideoBufferingEvent();
	}

	public void VideoReadyToPlayEvent()
	{
		if (OnVideoReadyToPlayEvent != null)
			OnVideoReadyToPlayEvent();    
	}

	public void VideoPlayEvent()
	{
		if (OnVideoPlayEvent != null)
			OnVideoPlayEvent();
	}

	public void VideoPositionChangedEvent()
	{
		if (OnVideoPositionChangedEvent != null)
			OnVideoPositionChangedEvent();
	}

	public void VideoEndEvent()
	{
		if (OnVideoEndEvent != null)
			OnVideoEndEvent ();

	}

	public void VideoErrorEvent(string message)
	{
		if (OnVideoErrorEvent != null)
			OnVideoErrorEvent(message);

	}

	public void AnalyticsStatsGazeEvent(string message)
	{
		if (OnAnalyticsStatsGazeEvent != null)
			OnAnalyticsStatsGazeEvent(message);

	}

	public void DrawTilesCompleteEvent(bool isHome)
	{
		if (OnDrawTilesComplete != null)
			OnDrawTilesComplete(isHome);
	}

	public void DrawContentMenuTilesEvent(ContentViewModel curContentViewModel)
	{
		if (OnDrawContentMenuTilesComplete != null)
			OnDrawContentMenuTilesComplete (curContentViewModel);
	}

	public void BackgroundAudioTriggerEvent(string backgroundAudioStr, bool isHome)
	{
		if (OnBackgroundAudioTrigger != null)
			OnBackgroundAudioTrigger (backgroundAudioStr, isHome);

	}

	public void BackgroundImageTriggerEvent(string backgroundImgStr)
	{
		if (OnBackgroundImageTrigger != null)
			OnBackgroundImageTrigger (backgroundImgStr);
	}

	public void TileClickEvent()
	{
		if (OnTileClickAction != null)
			OnTileClickAction();

	}

	public void SwitchToConsumptionEvent()
	{
		if (OnSwitchToConsumption != null)
			OnSwitchToConsumption();

	}

	public void SwitchToShoulderContentEvent()
	{
		if (OnSwitchToShoulderContent != null)
			OnSwitchToShoulderContent();

	}

	public void HardwareAudioMuteEvent(bool isMute)
	{
		if (OnHardwareAudioIsMute != null)
			OnHardwareAudioIsMute(isMute);

	}

	public void DisableUserClickEvent()
	{
		if (OnDisableUserClick != null)
			OnDisableUserClick();

	}

	public void EnvironmentLoadComplete()
	{
		if (OnEnvironmentLoadComplete != null)
			OnEnvironmentLoadComplete();

	}

	public void EnableUserClickEvent()
	{
		if (OnEnableUserClick != null)
			OnEnableUserClick();

	}

	public void PanelOpenEvent()
	{
		if (OnPanelOpenComplete != null)
			OnPanelOpenComplete();

	}

	public void PanelCloseEvent()
	{
		if (OnPanelCloseComplete != null)
			OnPanelCloseComplete();

	}

	public void PasswordCheckEvent()
	{
		if (OnPasswordCheck != null)
			OnPasswordCheck ();
	}

	public void PasswordCorrectEvent()
	{
		if (OnPasswordCorrect != null)
			OnPasswordCorrect ();
	}

	public void PasswordInCorrectEvent()
	{
		if (OnPasswordInCorrect != null)
			OnPasswordInCorrect ();
	}

	public void PinpadOpenEvent()
	{
		if (OnPinpadOpen != null)
			OnPinpadOpen ();
	}

	public void PinpadCloseEvent()
	{
		if (OnPinpadClose != null)
			OnPinpadClose ();
	}

	public void AllowCameraPivotEvent()
	{
		if (OnAllowCameraPivot != null)
			OnAllowCameraPivot ();
	}

	public void GamedayScoresOpenedEvent()
	{
		if (OnGamedayScoresOpened != null)
			OnGamedayScoresOpened();
	}

	public void BlackoutFadeInEvent()
	{
		if (OnBlackoutFadeInEvent != null)
			OnBlackoutFadeInEvent();
	}

	public void LightsOffEvent()
	{
		if (OnLightsOffEvent != null)
			OnLightsOffEvent ();
	}

	public void LightsOnEvent()
	{
		if (OnLightsOnEvent != null)
			OnLightsOnEvent ();
	}

	public void StatsToggledEvent(string toggleState)
	{
		if (OnStatsToggled != null)
			OnStatsToggled(toggleState);
	}

	public void ControllerSwipeEvent(string swipeDirection)
	{
		if (OnControllerSwipe != null)
			OnControllerSwipe(swipeDirection);
	}

	public void PitcherBatterResetEvent()
	{
		if (OnPitcherBatterReset != null)
			OnPitcherBatterReset();
	}

	public void BannerAdStartEvent()
	{
		if (OnBannerAdStart != null)
			OnBannerAdStart();
	}

	public void BannerAdStopEvent() 
	{
		if (OnBannerAdStop != null)
			OnBannerAdStop();
	}

	public void PopupShownEvent()
	{
		if (OnPopupShown != null)
			OnPopupShown();
	}

	public void PitcherBatterChanged(string playerID, string inning, GridStats.IconType iconType)
	{
		if (OnPitcherBattterChanged != null)
			OnPitcherBattterChanged(playerID, inning, iconType);
	}

	public void VideoTriggerEvent(ContentViewModel contentViewModel)
	{
		if (OnVideoTrigger != null)
			OnVideoTrigger (contentViewModel);
	}

	public void LogStringEvent(string objectToLog)
	{
		if(OnLogStringEvent != null)
			OnLogStringEvent(objectToLog);
	}

	public void TeamNameReceivedEvent(bool isHome, string teamCode)
	{
		if (OnTeamNameReceived != null)
			OnTeamNameReceived(isHome, teamCode);
	}

	/// <summary>
	/// Rather than duplicate this event to be passed on to multiple states, track analytics here
	/// </summary>
	public void ChannelTriggerEvent()
	{
		string channelTitle = PlayerPrefs.GetString ("channelTitle");
		string channelID = PlayerPrefs.GetString ("channelID");
		float channelStartTime = PlayerPrefs.GetFloat ("channelStartTime");
		bool isChannelTracked =  PlayerPrefs.GetInt ("isChannelToTrack") == 1 ? true : false;

		if (!String.IsNullOrEmpty (channelTitle) && !String.IsNullOrEmpty (channelID) && channelStartTime != null)
		{
			//based on our time, track this channel
			float channelTotalTime = Time.time - PlayerPrefs.GetFloat("channelStartTime");
			//			Debug.Log("Analytics: Track channel: " + channelTitle + " watched for " + channelTotalTime + " seconds.");
			//if(isChannelTracked)
			//{
			//	// Debug.Log("Kochava: Tracking channel:"+channelTitle.ToLower());
			//	Tracker.SendEvent("trackingChannel-" + channelTitle.ToLower(), channelTotalTime.ToString());
			//}

			#region Analytics Call
			Analytics.CustomEvent("TimeInChannel", new Dictionary<string, object>
			{
				{ channelTitle, channelTotalTime}
			});
			#endregion
		}

		PlayerPrefs.DeleteKey ("channelTitle");
		PlayerPrefs.DeleteKey ("channelID");
		PlayerPrefs.DeleteKey ("channelStartTime");
	}
}