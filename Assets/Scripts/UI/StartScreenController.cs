using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class StartScreenController : MonoBehaviour 
{   
	public List<TextMeshPro> text = new List<TextMeshPro>();

	private CanvasGroup _startScreenCanvasGroup;

	void Awake()
	{
		_startScreenCanvasGroup = GetComponent<CanvasGroup>();
		_startScreenCanvasGroup.alpha = 0.0f;
		for (int i = 0; i < text.Count; i++)
        {
            text[i].DOFade(0.0f, 0.0f);
        }
	}

	public void ShowStartScreen(float delay = 0.0f)
    {
		gameObject.SetActive(true);
		for(int i = 0; i < text.Count; i++)
		{
			text[i].DOFade(1.0f, 0.4f).SetDelay(delay);
		}

		_startScreenCanvasGroup.DOFade(1.0f, 0.2f).SetDelay(delay);
    }

    public void HideStartScreen()
    {
		for (int i = 0; i < text.Count; i++)
        {
			text[i].DOFade(0.0f, 0.2f);
        }

		_startScreenCanvasGroup.DOFade(0.0f, 0.4f).OnComplete(() => {
            gameObject.SetActive(false);
		});
    }
}
