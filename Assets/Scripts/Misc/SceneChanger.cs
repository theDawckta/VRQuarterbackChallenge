using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRStandardAssets.Utils;

public class SceneChanger : Singleton<SceneChanger>
{

	private AsyncOperation _curScene;
	private string _sceneName = "";

	public void SceneInitializer(string sceneName)
	{
		_sceneName = sceneName;
		_curScene = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
		_curScene.allowSceneActivation = false;
	}

    public Coroutine FadeToConsumptionAsync()
    {
		EventManager.Instance.SwitchToConsumptionEvent();
        return FadeToSceneAsync("ConsumptionScene");
    }

	public Coroutine FadeToShoulderContentAsync()
	{
		EventManager.Instance.SwitchToShoulderContentEvent();
		return FadeToSceneAsync("ShoulderContentScene");
	}

    public Coroutine FadeToSceneAsync(string sceneName)
    {
        return StartCoroutine(FadeToScene(sceneName));
    }

    private IEnumerator FadeToScene(string sceneName)
    {
        if (Camera.main != null)
        {
            VRCameraFade cameraFade = Camera.main.gameObject.GetComponent<VRCameraFade>();
            if (cameraFade != null)
            {
                yield return StartCoroutine(cameraFade.BeginFadeOut(false));
            }
        }
		//check to see if we were the scene that we preloaded
		if (!string.IsNullOrEmpty(_sceneName))
		{
			if (_sceneName == sceneName)
			{
				while (_curScene.progress <= 0.89f)
					yield return null;
			
				_curScene.allowSceneActivation = true;
			} else
			{
				LoadSceneNormal (sceneName);
			}
		}else{
			LoadSceneNormal (sceneName);
		}
    }

	void LoadSceneNormal(string sceneName)
	{
		SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
	}

	public Coroutine FadeToSceneAsync(string sceneName, GameObject underlayVideo, GameObject underlayEnvironment)
	{
		return StartCoroutine(FadeToScene(sceneName, underlayVideo, underlayEnvironment));
	}

	private IEnumerator FadeToScene(string sceneName, GameObject underlayVideo, GameObject underlayEnvironment)
	{
		if (Camera.main != null)
		{
			VRCameraFade cameraFade = Camera.main.gameObject.GetComponent<VRCameraFade>();
			if (cameraFade != null)
			{
				yield return StartCoroutine(cameraFade.BeginFadeOut(false));
			}
		}
		underlayVideo.gameObject.SetActive (false);
		underlayEnvironment.gameObject.SetActive (false);

		SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
	}

    public Coroutine ReloadSceneAsync()
    {
        Scene active = SceneManager.GetActiveScene();
        return FadeToSceneAsync(active.name);
    }
}
