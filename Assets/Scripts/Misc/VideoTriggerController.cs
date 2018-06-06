using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using System;

public class VideoTriggerController : MonoBehaviour {

	public TwoDPlayerController TwoDPlayer;

	private DataCursor _Cursor;
	private float _SceneLoadDelay = 0.6f;
	private ContentViewModel _ContentViewModel;


	IEnumerator Start()
	{
		yield return DataCursorComponent.Instance.GetCursorAsync (cursor => _Cursor = cursor);
	}

	void AnimReticleOut()
	{
		if (ResourceManager.Instance.Reticle != null)
		{
			ReticleController reticleController = ResourceManager.Instance.Reticle.GetComponent<ReticleController>();
			if(reticleController != null)
				reticleController.AnimReticleOut();
		}
	}

	IEnumerator FadeWithDelay()
	{
		EventManager.Instance.DisableUserClickEvent ();
		AnimReticleOut ();
		yield return new WaitForSeconds(_SceneLoadDelay);
		yield return SceneChanger.Instance.FadeToConsumptionAsync();
	}
		
	/// <summary>
	/// Listen for when a new video is requested - trigger scene change to play
	/// </summary>
	/// <param name="contentViewModel">Content view model.</param>
	void EventManager_OnVideoTrigger (ContentViewModel contentViewModel)
	{
		_ContentViewModel = contentViewModel;
		EventManager.Instance.TileClickEvent();

		_Cursor.MoveTo(_ContentViewModel);
		string tileTitle = _ContentViewModel.CaptionLine1 + ": " + _ContentViewModel.CaptionLine2;

		#region Analytics Call
		Analytics.CustomEvent("TileClicked", new Dictionary<string, object> {
			{ "TileTitle", tileTitle },
		});
		#endregion

		if (_ContentViewModel.Type == ContentType.Clip || _ContentViewModel.Type == ContentType.TWO_D_CONSUMPTION)
		{
			#region Analytics Call
			Analytics.CustomEvent("VideoPlayed", new Dictionary<string, object> {
				{ "VideoTitle", tileTitle },
				{ "VideoId", _ContentViewModel.ID }
			});
			#endregion

			PlayerPrefs.SetString("LastVideoSeen", tileTitle);

			StartCoroutine(FadeWithDelay());
		}
		else if (_ContentViewModel.Type == ContentType.TWO_D)
		{
			Play2DContent();
		}
		else if (_ContentViewModel.Type == ContentType.THREE_SIXTY)
		{
			StartCoroutine(Play360Content());
		}
	}


	/// <summary>
	/// Play content via our 2d player directly
	/// </summary>
	void Play2DContent()
	{
		StartCoroutine (DisableHardwareBack ());
		if(ResourceManager.Instance.VRCameraFadeObj != null) 
			ResourceManager.Instance.VRCameraFadeObj.FadeCameraOut();
		if (TwoDPlayer != null)
		{
			TwoDPlayer.Show ();
			TwoDPlayer.SetMediaUrl (_ContentViewModel.Streams [0].Url);
			TwoDPlayer.Play ();
		}
	}

	/// <summary>
	/// Play content by moving our player to our 360 scene
	/// </summary>
	/// <returns>The content.</returns>
	IEnumerator Play360Content()
	{
		EventManager.Instance.DisableUserClickEvent ();
		AnimReticleOut ();
		yield return new WaitForSeconds(_SceneLoadDelay);
		yield return SceneChanger.Instance.FadeToShoulderContentAsync();
	}

	/// <summary>
	/// need to disable the users ability to click back via hardware for X amount of time
	/// </summary>
	/// <param name="delayTime">The amount of delay before hardware click is renabled.</param>
	private IEnumerator DisableHardwareBack(float delayTime = 1.0f)
	{
		EventManager.Instance.DisableUserClickEvent ();
		yield return new WaitForSeconds(delayTime);
		EventManager.Instance.EnableUserClickEvent ();
	}

	void OnEnable()
	{
		EventManager.OnVideoTrigger += EventManager_OnVideoTrigger;;
	}

	void OnDisable()
	{
		EventManager.OnVideoTrigger -= EventManager_OnVideoTrigger;
	}
}
