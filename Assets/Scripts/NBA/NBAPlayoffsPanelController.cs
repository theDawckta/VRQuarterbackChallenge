using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

public class NBAPlayoffsPanelController : MonoBehaviour
{
	public NBAPlayoffRoundController PlayoffRound1Controller;
	public NBAPlayoffRoundController PlayoffRound2Controller;
	public NBAPlayoffRoundController PlayoffRound3Controller;
	public NBAPlayoffRoundController PlayoffRound4Controller;

	public DynamicTexture Round1Image;
	public DynamicTexture Round2Image;
	public DynamicTexture Round3Image;
	public DynamicTexture Round4Image;

	public RawImage EastHeaderImage;
	public RawImage WestHeaderImage;

	public ButtonControllerReplacement PrevButton;
	public ButtonControllerReplacement NextButton;

	private int _TotalRounds = 4;
	private int _InitialRound = 1;
	private int _CurRound;

	private CanvasGroup _PlayoffRound1Canvas;
	private CanvasGroup _PlayoffRound2Canvas;
	private CanvasGroup _PlayoffRound3Canvas;
	private CanvasGroup _PlayoffRound4Canvas;

	private bool _IsEast = false;

	void Awake()
	{
		CheckRequired (PlayoffRound1Controller, "Round1");
		CheckRequired (PlayoffRound2Controller, "Round2");
		CheckRequired (PlayoffRound3Controller, "Round1");
		CheckRequired (PlayoffRound4Controller, "Round2");
		CheckRequired (Round1Image, "Round1Image");
		CheckRequired (Round2Image, "Round2Image");
		CheckRequired (Round3Image, "Round3Image");
		CheckRequired (Round4Image, "Round4Image");
		CheckRequired (PrevButton, "PrevButton");
		CheckRequired (NextButton, "NextButton");
		_PlayoffRound1Canvas = PlayoffRound1Controller.GetComponent<CanvasGroup> ();
		_PlayoffRound2Canvas = PlayoffRound2Controller.GetComponent<CanvasGroup> ();
		_PlayoffRound3Canvas = PlayoffRound3Controller.GetComponent<CanvasGroup> ();
		_PlayoffRound4Canvas = PlayoffRound4Controller.GetComponent<CanvasGroup> ();
		CheckRequired (_PlayoffRound1Canvas, "_PlayoffRound1Canvas");
		CheckRequired (_PlayoffRound2Canvas, "_PlayoffRound2Canvas");
		CheckRequired (_PlayoffRound3Canvas, "_PlayoffRound3Canvas");
		CheckRequired (_PlayoffRound4Canvas, "_PlayoffRound4Canvas");


		_CurRound = _InitialRound;

		HidePanels ();
		PrevButton.Disable ();
		NextButton.Disable ();
	}

	void HidePanels()
	{
		Hide (PlayoffRound1Controller.gameObject, _PlayoffRound1Canvas);
		Hide (PlayoffRound2Controller.gameObject, _PlayoffRound2Canvas);
		Hide (PlayoffRound3Controller.gameObject, _PlayoffRound3Canvas);
		Hide (PlayoffRound4Controller.gameObject, _PlayoffRound4Canvas);
	}

	void Hide(GameObject objToHide, CanvasGroup canvasGroup = null)
	{
		if (objToHide != null)
			objToHide.transform.localScale = Vector3.zero;

		if (canvasGroup != null)
			canvasGroup.alpha = 0;
	}
	void Show(GameObject objToShow, CanvasGroup canvasGroup = null)
	{
		if (objToShow != null)
			objToShow.transform.localScale = Vector3.one;

		if(canvasGroup != null)
			canvasGroup.DOFade (1.0f, 0.3f);
	}

	void CheckRequired(object thing, string name)
	{
		if (thing == null)
			throw new Exception(String.Format("A {0} is required to run this scene.", name));
	}

	/// <summary>
	/// Initialize our full panel; send it only the data from the JSON that applies (east or west)
	/// </summary>
	/// <param name="conferenceSeries">Conference series.</param>
	/// <param name="curRound">Current round that should be displayed.</param>
	/// <param name="IsEast">If set to <c>true</c> is east.</param>
	public void InitPanel(List<Round> conferenceSeries, int curRound, bool isEast = false)
	{
		_IsEast = isEast;
		EastHeaderImage.gameObject.SetActive (isEast);
		WestHeaderImage.gameObject.SetActive (!isEast);
		_CurRound = curRound;
		_TotalRounds = conferenceSeries.Count;

		//if there is only a single round
		if (_TotalRounds == 1)
		{
			PrevButton.gameObject.SetActive (false);
			NextButton.gameObject.SetActive (false);
		}

		for (int i = 0; i < _TotalRounds; i++)
		{
			Round curPlayoffRound = conferenceSeries [i];
			InitRound (curPlayoffRound);
		}
	}

	/// <summary>
	/// Only called once to initialize the data for the rounds
	/// </summary>
	/// <param name="curRound">Current round to initialize.</param>
	void InitRound(Round curRound)
	{
		switch (curRound.roundNum)
		{
		case 1:
			Round1Image.SetTexture (curRound.backgroundImg, true, () => SetPanel (1));
			PlayoffRound1Controller.SetContent (curRound, _IsEast);
			break;
		case 2:
			Round2Image.SetTexture (curRound.backgroundImg, true, ()=>SetPanel(2));
			PlayoffRound2Controller.SetContent (curRound, _IsEast);
			break;
		case 3:
			Round3Image.SetTexture (curRound.backgroundImg, true, ()=>SetPanel(3));
			PlayoffRound3Controller.SetContent (curRound, _IsEast);
			break;
		case 4:
			Round4Image.SetTexture (curRound.backgroundImg, true, ()=>SetPanel(4));
			PlayoffRound4Controller.SetContent (curRound, _IsEast);
			break;
		}
	}

	void NormalizeRounds()
	{
		if (_CurRound < _InitialRound)
			_CurRound = _InitialRound;

		if (_CurRound > _TotalRounds)
			_CurRound = _TotalRounds;
	}

	void SetNav()
	{
		if (_CurRound > _InitialRound)
			PrevButton.Enable ();
		else
			PrevButton.Disable ();
		
		if (_CurRound < _TotalRounds)
			NextButton.Enable ();
		else
			NextButton.Disable ();
	}

	/// <summary>
	/// Called every time a panel needs to be displayed (a round is switched)
	/// </summary>
	/// <param name="curRound">Current round to show.</param>
	void SetPanel(int curRound)
	{
		SetNav ();
		if (_CurRound == curRound)
		{
			switch (_CurRound)
			{
			case 1:
				Show (PlayoffRound1Controller.gameObject, _PlayoffRound1Canvas);
				break;
			case 2:
				Show (PlayoffRound2Controller.gameObject, _PlayoffRound2Canvas);
				break;
			case 3:
				Show (PlayoffRound3Controller.gameObject, _PlayoffRound3Canvas);
				break;
			case 4:
				Show (PlayoffRound4Controller.gameObject, _PlayoffRound4Canvas);
				break;
			}

		}
	}

	void OnEnable()
	{
		PrevButton.OnButtonClicked += PrevButton_OnButtonClicked;
		NextButton.OnButtonClicked += NextButton_OnButtonClicked;
	}
	void OnDisable()
	{
		PrevButton.OnButtonClicked -= PrevButton_OnButtonClicked;
		NextButton.OnButtonClicked -= NextButton_OnButtonClicked;
	}

	void NextButton_OnButtonClicked (object sender)
	{
		_CurRound++;
		HidePanels ();
		NormalizeRounds ();
		SetPanel (_CurRound);

	}

	void PrevButton_OnButtonClicked (object sender)
	{
		_CurRound--;
		HidePanels ();
		NormalizeRounds ();
		SetPanel (_CurRound);
	}
}

