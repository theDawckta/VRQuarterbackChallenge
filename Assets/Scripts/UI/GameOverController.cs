using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class GameOverController : MonoBehaviour
{
	public TextMeshPro ScoreText;
	public TextMeshPro MessageText;
	public List<TextMeshPro> text = new List<TextMeshPro>();
	private string _loseText = "KEEP PLAYING - DAILY TOP SCORES CAN EARN YOU FIZZY COLA<sup>®</sup> GIVEAWAYS.";
	private string _winText = "THROWIN' LIKE A PRO - YOU'VE EARNED A TOP 5 SCORE! YOUR'RE NOW ELIGIBLE FOR FIZZY COLA<sup>®</sup> GIVEAWAYS.";
    
	private CanvasGroup _gameOverScreenCanvasGroup;

	void Awake()
    {
		_gameOverScreenCanvasGroup = GetComponent<CanvasGroup>();
		_gameOverScreenCanvasGroup.alpha = 0.0f;
		for (int i = 0; i < text.Count; i++)
        {
            text[i].DOFade(0.0f, 0.0f);
        }
    }
    
	public void ShowGameOverScreen(int scoreText, bool highScore, float delay = 0.0f)
	{
		ScoreText.text = string.Format("{0:n0}", scoreText);
        if(highScore)
		    MessageText.text = _winText;
		else
			MessageText.text = _loseText;
		gameObject.SetActive(true);
        for (int i = 0; i < text.Count; i++)
        {
			text[i].DOFade(1.0f, 0.4f).SetDelay(delay);
        }

		_gameOverScreenCanvasGroup.DOFade(1.0f, 0.2f).SetDelay(delay);
	}

	public void HideGameOverScreen()
    {
		for (int i = 0; i < text.Count; i++)
        {
            text[i].DOFade(0.0f, 0.2f);
        }

		_gameOverScreenCanvasGroup.DOFade(0.0f, 0.4f).OnComplete(() => {
            gameObject.SetActive(false);
        });
	}
}