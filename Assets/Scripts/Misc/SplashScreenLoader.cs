using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreenLoader : MonoBehaviour {

	public float delayBeforeLoad = 3.0f;
	public string sceneToLoad = string.Empty;
	public BlackoutCube BlackoutCubeAnim;

	private AsyncOperation _Async;
	private bool _IsDone = false;
	private float _timeDelta;

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
		if (BlackoutCubeAnim == null)
		{
			throw new Exception("A BlackoutCubeAnim must exist.");
		}
	}

	// Use this for initialization
	void Start()
	{
		Application.backgroundLoadingPriority = ThreadPriority.Low;
		BlackoutCubeAnim.FadeOut();
        Cursor.visible = false;
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
		yield return new WaitForSeconds(delayBeforeLoad);

		_Async = SceneManager.LoadSceneAsync(sceneToLoad);
		_Async.allowSceneActivation = false;
		yield return _Async;
	}

	void Update()
	{
		//if (!_IsDone)
		//{
//			if (_Async != null)
//			{
//				if (_Async.progress >= _DoneLevel)
//				{
//					_IsDone = true;
//					ChangeScene();
//				}
//			}
		//}
		_timeDelta += Time.deltaTime;
		if(_timeDelta > 4.0f)
		{
			_timeDelta = 0.0f;
			ChangeScene();
		}
	}

	void ChangeScene()
	{
		BlackoutCubeAnim.FadeIn();
	}

	void OnFadeInComplete()
	{
		_Async.allowSceneActivation = true;
	}
}
