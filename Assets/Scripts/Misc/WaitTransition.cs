using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaitTransition : MonoBehaviour
{
	public float delayBeforeLoad = 0.1f;
	public string sceneToLoad = string.Empty;
	public BlackoutCube BlackoutCubeAnim;

	private AsyncOperation _Async;
	private bool _IsDone = false;
	private float _DoneLevel = 0.9f;    //when it hits this level, it is loaded (defined by Unity)

	private void OnEnable()
	{
		EventManager.OnBlackoutFadeInEvent += OnFadeInComplete;
		EventManager.OnBlackoutFadeComplete += OnFadeComplete;
	}

	private void OnDisable()
	{
		EventManager.OnBlackoutFadeInEvent -= OnFadeInComplete;
		EventManager.OnBlackoutFadeComplete -= OnFadeComplete;
	}

	void Awake()
	{
		PlayerPrefs.SetInt("ComingFromTransition", 1);
		if (BlackoutCubeAnim == null)
		{
			throw new Exception("A BlackoutCubeAnim must exist.");
		}
	}

	// Use this for initialization
	void Start()
	{
		BlackoutCubeAnim.FadeOut();
		Application.backgroundLoadingPriority = ThreadPriority.Low;      
	}

    void OnFadeComplete()
    {
		StartCoroutine(DelayedSceneLoad());
    }

    /// <summary>
    /// Asynchronously start the main scene load.
    /// </summary>
    IEnumerator DelayedSceneLoad()
	{
		// delay one frame to make sure everything has initialized
		yield return 0;

		yield return new WaitForSeconds(delayBeforeLoad);
		//SceneManager.LoadSceneAsync(sceneToLoad);
		_Async = SceneManager.LoadSceneAsync(sceneToLoad);
		_Async.allowSceneActivation = false;
		yield return _Async;
	}

	void Update()
	{
		if (!_IsDone)
		{
			if (_Async != null)
			{
				if (_Async.progress >= _DoneLevel)
				{
					_IsDone = true;
					ChangeScene();
				}
			}
		}
	}

	void ChangeScene()
	{
		BlackoutCubeAnim.FadeIn();
	}

	void OnFadeInComplete()
	{
		//if(dependencyStatus != Firebase.DependencyStatus.Available)
		//{
		//	string message = "Kindly remove the phone from the headset and update Google Play Services";
		//	PopupController.Instance.ShowImagePopupDelayed(0.5f,message, false, true);
		//	_isServicesPopup = true;
		//}
		//else
        if(_Async != null)
		    _Async.allowSceneActivation = true;
	}
}