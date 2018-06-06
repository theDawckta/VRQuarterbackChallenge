using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.VR;
using VRStandardAssets.Utils;
using VOKE.VokeApp.DataModel;
using DG.Tweening;

public class HomeController : MonoBehaviour
{
	public ReticleController Reticle;
	public ThreeDButtonController AdSceneButton;
	public CanvasGroup SplashCanvasGroup;
	public AudioSource HomeAudioSource;

    private bool _isInitialLoad = true;
    private bool _isApplicationQuitting;  

    private void Awake()
    {      
		if(PlayerPrefs.HasKey("ComingFromTransition"))
		{
			PlayerPrefs.DeleteKey("ComingFromTransition");
			SplashCanvasGroup.DOFade(0.0f, 0.0f);
		}
    }

    void Start()
    {
		//SceneChanger.Instance.SceneInitializer ("AdScene");
		//SceneManager.activeSceneChanged += SceneChanged;
		HomeAudioSource.Play();
		HomeAudioSource.DOFade(0.5f, 1.0f);
		AdSceneButton.ShowButton(0.5f);
		SplashCanvasGroup.DOFade(0.0f, 0.5f);
    }

    private void Instance_PresenceChanged(object sender, UserPresenceEventArgs e)
    {
        if (e.IsPresent)
        {
            Debug.Log("Is Present: " + e.IsPresent);
            UnityEngine.XR.InputTracking.Recenter();
        }
    }

    public void OpenAdScene()
    {      
		SplashCanvasGroup.DOFade(1.0f, 0.5f);
		HomeAudioSource.DOFade(0.0f, 0.8f);
		StartCoroutine(DelayedSceneLoad("AdScene"));
    }

	/// <summary>
	/// Asynchronously start the main scene load.
	/// </summary>
	IEnumerator DelayedSceneLoad(string sceneToLoad)
	{
		// delay one frame to make sure everything has initialized
		yield return new WaitForEndOfFrame ();
		if(Reticle != null)
			Reticle.AnimReticleOut ();
		//yield return new WaitForSeconds(0.1f);
		yield return new WaitForEndOfFrame ();  
		SceneChanger.Instance.FadeToSceneAsync(sceneToLoad);
	}

	public void SceneChanged(Scene current, Scene next)
	{
		Destroy(GameObject.Find("PhoneRemoteConfig"));
		Destroy(GameObject.Find("PhoneRemote"));
		Destroy(GameObject.Find("(singleton) UserPresenceController"));
		Destroy(GameObject.Find("(singleton) GlobalVars"));
		Destroy(GameObject.Find("(singleton) AppConfigurationLoader"));
		Destroy(GameObject.Find("(singleton) DataCursorComponent"));
		Destroy(GameObject.Find("(singleton) FeedbackSystemController"));
		Destroy(GameObject.Find("(singleton) SceneChanger"));
		Destroy(GameObject.Find("(singleton) TextureUnwrapperQueue"));

		SceneManager.activeSceneChanged -= SceneChanged;
	}

	private void OnEnable()
    {
        UserPresenceController.Instance.PresenceChanged += Instance_PresenceChanged;
		AdSceneButton.OnButtonClicked += OpenAdScene;
    }

    private void OnDisable()
    {
        if (_isApplicationQuitting)
            return;

        UserPresenceController.Instance.PresenceChanged -= Instance_PresenceChanged;
		AdSceneButton.OnButtonClicked -= OpenAdScene;
    }
}