using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIController : MonoBehaviour 
{
	public delegate void OnStartButtonClickedEvent();
    public event OnStartButtonClickedEvent OnStartButtonClicked;
	public delegate void OnBackButtonClickedEvent();
    public event OnBackButtonClickedEvent OnBackButtonClicked;
	public delegate void OnTenSecondsLeftEvent();
	public event OnTenSecondsLeftEvent OnTenSecondsLeft;
	public delegate void OnCountdownOverEvent();
	public event OnCountdownOverEvent OnCountdownOver;

    public TextMeshPro Timer;
    public TextMeshPro Score;
	public StartScreenController StartScreen;
	public GameObject ButtonHolder;
	public ThreeDButtonController RightButton;
	public ThreeDButtonController LeftButton;
	public ButtonControllerReplacement BackToSuite;
    public HighScoresController HighScores;
	public GameOverController GameOver;
	public CountdownController Countdown;
	public float HighScoreWaitTime = 5.0f;
	public float GameOverWaitTime = 7.0f;
	public Color FlashingTimerColor;

	private bool _timerRed = false;
	private bool _tenSeconds = false;

	void Start()
	{
		StartCoroutine(StartDelay());
	}

	IEnumerator StartDelay()
	{
		yield return new WaitForSeconds(0.5f);      
        ShowStartScreen(0.3f);
		ShowButtons();
	}

	public void UpdateTimerText(float gameTime)
    {
		if (gameTime < 10.0f && _tenSeconds == false)
        {
			_tenSeconds = true;
			OnTenSecondsLeft();
        }
		if(gameTime < 5.0f && _timerRed == false)
		{
			_timerRed = true;
			Timer.faceColor = FlashingTimerColor;
		}
        TimeSpan t = TimeSpan.FromSeconds(gameTime);
        Timer.text = string.Format("{0:0#}.{1:0#}", t.Seconds, t.Milliseconds / 10);
    }  

    public void UpdateScore(int newScore)
    {
        Score.text = string.Format("{0:n0}", newScore);
    }

    public void ResetScoreboard(float seconds)
    {
        UpdateTimerText(seconds);
        Score.text = "0";
    }

    public void HideStartScreen()
    {
		StartScreen.HideStartScreen();
    }

    public void ShowStartScreen(float delay = 0.0f)
    {
		StartScreen.ShowStartScreen(delay);
		ShowButtons(delay);
    }

    public void ShowGameOverScreen(int score)
	{
		bool highScore;
		_timerRed = false;
		_tenSeconds = false;
		StopCoroutine("_timerFlashCoroutine");
		Timer.faceColor = Color.white;
		highScore = HighScores.CheckForTopScore(score);
		GameOver.ShowGameOverScreen(score, highScore);
		StartCoroutine(GameOverScreenWait(score));
	}

	IEnumerator GameOverScreenWait(int score)
	{
		yield return new WaitForSeconds(GameOverWaitTime);
		HideGameOverScreen();
		ShowHighScoreScreen(score, 0.4f);
	}

    public void HideGameOverScreen()
	{
		GameOver.HideGameOverScreen();
	}

    public void HideHighScoreScreen()
    {
		HighScores.HideHighScores();
    }

    public void ShowHighScoreScreen(int score, float delay = 0.0f)
    {
        HighScores.ShowHighScores(score, delay);
    }

    public void HideButtons()
	{
		RightButton.HideButton();
		LeftButton.HideButton();
	}

	public void ShowButtons(float delay = 0.0f)
	{
		RightButton.ShowButton(delay);
        LeftButton.ShowButton(delay);
	}

    void OnHighScoreEntryDone()
    {
		HideHighScoreScreen();
        ShowStartScreen(0.8f);
    }

    void OnHighScoreEntryNotAchieved()
    {
		StartCoroutine(HighScoreDelay());
    }

    void StartButtonClicked()
	{
		OnStartButtonClicked();
	}

	void BackButtonClicked()
    {
		OnBackButtonClicked();
    }

	void BackButtonClicked(object sender)
    {
        OnBackButtonClicked();
    }

	void CountdownOver() 
	{
		OnCountdownOver();
	}

	IEnumerator HighScoreDelay()
	{
		yield return new WaitForSeconds(HighScoreWaitTime);
		HideHighScoreScreen();
		ShowStartScreen(0.3f);
	}

	void OnEnable()
	{
        HighScores.OnHighScoreEntryDone += OnHighScoreEntryDone;
        HighScores.OnHighScoreNotAchieved += OnHighScoreEntryNotAchieved;
		LeftButton.OnButtonClicked += StartButtonClicked;
		RightButton.OnButtonClicked += BackButtonClicked;
		BackToSuite.OnButtonClicked += BackButtonClicked;
		Countdown.OnCountdownOver += CountdownOver;
	}

	void OnDisable()
	{
        HighScores.OnHighScoreEntryDone -= OnHighScoreEntryDone;
        HighScores.OnHighScoreNotAchieved -= OnHighScoreEntryNotAchieved;
		LeftButton.OnButtonClicked -= StartButtonClicked;
		RightButton.OnButtonClicked -= BackButtonClicked;
		BackToSuite.OnButtonClicked -= BackButtonClicked;
		Countdown.OnCountdownOver -= CountdownOver;
	}
}
