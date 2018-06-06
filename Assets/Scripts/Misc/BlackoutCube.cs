using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class BlackoutCube : MonoBehaviour
{
	public float FadeOutTime = 0.5f;
	public float FadeInTime = 0.5f;

	private Material _blackOutCubeMaterial;

	void Awake()
	{
		_blackOutCubeMaterial = gameObject.GetComponentInChildren<Renderer>().material;
		gameObject.SetActive(true);
	}

	void HideBlackout()
	{
		gameObject.SetActive(false);
		EventManager.Instance.BlackoutFadeCompleteEvent();
	}

	void FadeInComplete()
	{
		EventManager.Instance.BlackoutFadeInEvent();
	}

	public void FadeIn()
	{
		_blackOutCubeMaterial.DOFade(1.0f, FadeInTime).OnComplete(FadeInComplete);
		gameObject.SetActive(true);
	}

	public void FadeOut()
	{
		_blackOutCubeMaterial.DOFade(0.0f, FadeOutTime).OnComplete(HideBlackout);
	}
}