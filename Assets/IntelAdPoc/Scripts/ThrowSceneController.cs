using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ThrowSceneController : MonoBehaviour
{
	public TargetSpawner TargetSpawner;
	public UIController UIControllerPOC;
	public GameAudioController GameAudio;
	public ProjectileController Projectile;
	public CanvasGroup SplashCanvasGroup;
	public GameObject SkyGeo;
	public ParticleSystem FlashParticles;
	public float RayCastDistance = 2000.0f;
	public string TargetLayer = "Target";
	public string EnvironmentLayer = "Environment";
	public string UILayer = "TopUI";
	public float GameTime = 40.0f;
	public int StreakMax = 5;
	public ReticleController Reticle;
	public AudioClip boo1;
	public AudioClip boo2;
	public AudioClip boo3;
	public AudioClip cheer1;
	public AudioClip cheer2;
	public AudioClip cheer3;
	public AudioClip cheer4;

	private bool _gameOn = false;
	private ProjectileController _projectile;
	private bool _ballInPlay = false;
	private TargetController _currentTargetController;
	private RaycastHit _targetHit;
	private TargetSpawner _targetSpawner = null;
	private float _gameTime;
	private int _streak = 1;
	private int _missStreak = 0;
	private int _currentScore = 0;
	private ParticleSystem.EmissionModule _flashParticlesEmission;

	void Start()
	{
		SkyGeo.transform.DOLocalRotate(new Vector3(0.0f, 0.0f, -360.0f), 600.0f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(-1);
		_flashParticlesEmission = FlashParticles.emission;
		_flashParticlesEmission.rateOverTime = 1100;
		FlashParticles.Play();
		SplashCanvasGroup.DOFade(0.0f, 0.5f);
    }

	private void Update()
	{
#if (UNITY_EDITOR)
		Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * RayCastDistance, Color.yellow);
#endif

		if (!_gameOn)
		{
			if (_currentTargetController != null)
				_currentTargetController.HideMarker(true);
			return;
		}

		if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out _targetHit, RayCastDistance, ~(1 << LayerMask.NameToLayer(UILayer))))
		{
#if (UNITY_EDITOR)
			Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * _targetHit.distance, Color.red);
#endif

			if (LayerMask.LayerToName(_targetHit.transform.gameObject.layer) == TargetLayer ||
			   LayerMask.LayerToName(_targetHit.transform.gameObject.layer) == EnvironmentLayer)
			{
				TargetController hitTargetController = _targetHit.transform.root.GetComponent<TargetController>();
                           
				// check to see if we have switched to a new TargetController, if not, turn everything off
				if ((_currentTargetController != hitTargetController) && hitTargetController != null)
				{
					// hide the old TargetController marker if it is not Frozen
					if (_currentTargetController != null && !_currentTargetController.IsFrozen)
					{
						_currentTargetController.HideMarker();
					}
					// if the old TargetController is frozen we need to unfreeze the new TargetController and show it
					if (_currentTargetController != null && _currentTargetController.IsFrozen)
					{
						hitTargetController.IsFrozen = false;
						hitTargetController.ShowMarker();
					}
					// if the new TargetController is Frozen we just wanna show that
					if (!hitTargetController.IsFrozen)
					{
						hitTargetController.ShowMarker();
					}

					if (_currentTargetController != null)
					{
						_currentTargetController.IsCurrent = false;
					}

					_currentTargetController = hitTargetController;
					_currentTargetController.IsCurrent = true;
				}
				else if (hitTargetController == null && _currentTargetController != null && !_currentTargetController.IsFrozen)
				{
					_currentTargetController.HideMarker();
					_currentTargetController.IsCurrent = false;
					_currentTargetController = null;
				}

				if (_currentTargetController != null && !_currentTargetController.IsFrozen)
				{
					_currentTargetController.PositionMarker(_targetHit.point);
				}

				if (!_gameOn && _currentTargetController != null)
					_currentTargetController.HideMarker();
			}
		}

		if (Input.GetMouseButtonDown(0))
		{
			//remove  && _currentTargetController != null below to enable shooting without looking at a target
			//if (!_ballInPlay && _currentTargetController != null)
			if (_currentTargetController != null)
			{
				LoadFootball();
				_projectile.gameObject.SetActive(true);

				if (_currentTargetController != null)
					_projectile.SetTarget(_currentTargetController.Marker.transform.position);
				else
					_projectile.SetTarget(_targetHit.point);

				if (_currentTargetController != null)
				{
					//_currentTargetController.IsFrozen = true;
				}

				_ballInPlay = true;
				GameAudio.PlayThrowSound();
				_projectile.Launch(_streak);
			}
		}
	}

	void StartCountdown()
	{
		UIControllerPOC.HideStartScreen();
		UIControllerPOC.ResetScoreboard(GameTime);
		UIControllerPOC.HideButtons();
		UIControllerPOC.HideGameOverScreen();
		UIControllerPOC.Countdown.StartGameCoundown();
		GameAudio.StopUIAudioSource();
	}

	void StartGame()
	{
		_streak = 1;
		_missStreak = 0;
		_currentScore = 0;
		_gameOn = true;
		LoadFootball();
		if (_targetSpawner != null)
			_targetSpawner.DestroyTargetsAndTargetSpawner();
		_targetSpawner = Instantiate(TargetSpawner) as TargetSpawner;
		_targetSpawner.OnTargetHit += TargetHit;
		_targetSpawner.OnTargetMissed += TargetMissed;
		_targetSpawner.StartTargets();
		_gameTime = GameTime;
		StartCoroutine(StartTimer());
	}

	void StopGame()
	{
		_gameOn = false;
		GameAudio.PlayEndMusic();
		GameAudio.StopLast10Seconds();
		UIControllerPOC.ShowGameOverScreen(_currentScore);
	}

	IEnumerator StartTimer()
	{
		while (_gameTime > 0.0f)
		{
			_gameTime -= Time.deltaTime;
			if (_gameTime < 0.0f)
				_gameTime = 0.0f;
			UIControllerPOC.UpdateTimerText(_gameTime);
			yield return null;
		}
		_gameTime = 0.0f;
		_targetSpawner.StopTargets();
		GameAudio.PlayEndHorn();
		UIControllerPOC.UpdateTimerText(0);
		if (_ballInPlay)
			StartCoroutine(WaitForBall());
		else
			StopGame();

		yield return null;
	}

	IEnumerator WaitForBall()
	{
		while (_ballInPlay)
			yield return null;
		StopGame();
	}

	void TenSecondsLeft()
	{
		GameAudio.PlayLast10Seconds();
	}

	void GoBack()
	{
		SplashCanvasGroup.DOFade(1.0f, 0.5f);
		StartCoroutine(DelayedSceneLoad("HomeScene"));
	}

	/// <summary>
	/// Asynchronously start the main scene load.
	/// </summary>
	IEnumerator DelayedSceneLoad(string sceneToLoad)
	{
		//disable our ability to click hardware buttons
		EventManager.Instance.DisableUserClickEvent();
		// delay one frame to make sure everything has initialized
		yield return 0;
		if (Reticle != null)
			Reticle.AnimReticleOut();
		yield return new WaitForSeconds(0.1f);

		SceneChanger.Instance.FadeToSceneAsync(sceneToLoad);
	}

	void ShowHighScoreScreen(object sender)
	{
		UIControllerPOC.HideGameOverScreen();
		UIControllerPOC.ShowHighScoreScreen(_currentScore, 0.3f);
	}

	void LoadFootball()
	{
		Vector3 SpawnLocation = new Vector3(Camera.main.transform.position.x + 0.3f,
											Camera.main.transform.position.y - 0.2f,
											Camera.main.transform.position.z);
		_projectile = Instantiate(Projectile, SpawnLocation, Projectile.transform.rotation) as ProjectileController;
		_projectile.OnProjectileHit += ProjectileHit;
	}

	void ProjectileHit(string colliderLayerName, ProjectileController projectile)
	{
		if (colliderLayerName == "Environment" && projectile.HitSomeThing == false)
		{
			//Debug.Log("Projectile Missed And Hit The Field");
			projectile.HitSomeThing = true;
			_ballInPlay = false;
			_streak = 1;
			_missStreak++;
			PlayStreakAudio();
		}
	}

	void TargetHit(TargetController targetHit, int baseScore)
	{
		if (targetHit != null)
		{
			int currentStreak = _streak;
			_missStreak = 0;
			_currentScore = _currentScore + (10 * baseScore * _streak);
			UIControllerPOC.UpdateScore(_currentScore);
			targetHit.SetScore(10 * baseScore * _streak, _streak);
			PlayStreakAudio();

			if (_streak < StreakMax)
				_streak = _streak + 1;
			if (_streak == StreakMax && currentStreak != _streak)
				StartCoroutine(PlayCanSound());         
		}

		_ballInPlay = false;
	}

	IEnumerator PlayCanSound()
	{
		yield return new WaitForSeconds(1.0f);
		GameAudio.PlayOpenCanSound();
	}

	void TargetBottleHit()
	{
		_streak = StreakMax;
	}

    void TargetMissed()
    {
        //Debug.Log("Target Missed");
        _streak = 1;  
		_missStreak++;
        _ballInPlay = false;
		PlayStreakAudio ();
    }

	void PlayStreakAudio()
	{
		//only play bad audio if we've missed more than twice in a row
		if (_missStreak == 2)
		{
			GameAudio.PlayAudioClip(boo1);
		} else if (_missStreak == 3)
		{
			GameAudio.PlayAudioClip(boo2);
		} else if (_missStreak > 3)
		{
			GameAudio.PlayAudioClip(boo3);
		}

		if (_streak == 2)
		{
			GameAudio.PlayAudioClip(cheer1);
			_flashParticlesEmission.rateOverTime = 200;
			FlashParticles.Play();
		} else if (_streak == 3)
		{
			GameAudio.PlayAudioClip(cheer2);
			_flashParticlesEmission.rateOverTime = 400;
            FlashParticles.Play();
		} else if (_streak == 4)
		{
			GameAudio.PlayAudioClip(cheer3);
			_flashParticlesEmission.rateOverTime = 700;
            FlashParticles.Play();
		} else if (_streak > 4)
		{
			GameAudio.PlayAudioClip(cheer4);
			_flashParticlesEmission.rateOverTime = 1000;
            FlashParticles.Play();
		}
	}

	void OnEnable()
	{
		UIControllerPOC.OnStartButtonClicked += StartCountdown;
		UIControllerPOC.OnBackButtonClicked += GoBack;
		UIControllerPOC.OnCountdownOver += StartGame;
		UIControllerPOC.OnTenSecondsLeft += TenSecondsLeft;
	}

    void OnDisable()
    {
		UIControllerPOC.OnStartButtonClicked -= StartCountdown;
		UIControllerPOC.OnBackButtonClicked -= GoBack;
		UIControllerPOC.OnCountdownOver -= StartGame;
		UIControllerPOC.OnTenSecondsLeft -= TenSecondsLeft;
    }
}
