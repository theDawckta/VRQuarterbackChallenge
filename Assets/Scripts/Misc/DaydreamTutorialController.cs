using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.VR;
using DG.Tweening;

public class DaydreamTutorialController : MonoBehaviour {

	public GameObject ClickBlock;
	public GameObject DaydreamContainer;
	public TextMeshProUGUI HeadlineTxt;
	public TextMeshProUGUI BodyTxt;
	public TextMeshProUGUI BtnTxt;
	public ButtonController ButtonController;
	public GameObject CloseIcon;
	public GameObject NextIcon;

	private string[] _BtnStr = new string[] {"NEXT", "NEXT", "CLOSE"}; 
	private string[] _HeadlineStr = new string[] {"Getting Started", "Getting Started", "Getting Started"}; 
	private string[] _ContentStr = new string[] {"To view content or select menu elements, simply press the touchpad on your controller.", 
		"Swipe your touchpad to scroll to the next screen and see more content.", 
		"While watching a video, press the touchpad controller to control playback, switch camera angles, or select menu elements."};

	private string nextStr = "Next";
	private string closeStr = "Close";
	private Vector3 _InitialTextPos;
	private float _YPosMove = 50.0f;	//how long should our y position move be

	private int _CurPage = 0;
	private CanvasGroup _TutorialCanvas;

	void Awake()
	{
		_InitialTextPos = BodyTxt.gameObject.transform.localPosition;
		CloseIcon.SetActive (false);
		NextIcon.SetActive (true);
		ClickBlock.SetActive (false);

		if (DaydreamContainer != null)
		{
			if (DaydreamContainer.GetComponent<CanvasGroup> () != null)
			{
				_TutorialCanvas = DaydreamContainer.GetComponent<CanvasGroup> ();
				_TutorialCanvas.alpha = 0;
			}
		}
		DaydreamContainer.SetActive (false);
	}

	public void Init()
	{
		//evaluate if we should run the tutorial, should only play one time
		string vrModel = UnityEngine.XR.XRDevice.model.ToLower();
		if (vrModel.IndexOf ("daydream") != -1 || GlobalVars.Instance.IsDaydream)
		{
			if (PlayerPrefs.GetInt ("DaydreamTutorialPlayed") != 1)
			{
				PlayerPrefs.SetInt ("DaydreamTutorialPlayed", 1);
				InitTutorial ();
			} else
			{
				this.gameObject.SetActive (false);
			}
		}
	}

	void InitTutorial()
	{
		FadeOutHome ();
		ClickBlock.SetActive (true);
		DaydreamContainer.SetActive (true);
		if (_TutorialCanvas != null)
		{
			_TutorialCanvas.alpha = 0;
			_TutorialCanvas.DOFade (1.0f, 0.3f).SetDelay(0.4f);
		}
		SetPage ();
	}

	void CloseTutorial()
	{
		ClickBlock.SetActive (false);
		DaydreamContainer.SetActive (false);
		FadeInHome ();
        ResourceManager.Instance.AuthenticationController.ShowInitialSignInPrompt();
	}


	void SetPage()
	{
		string curBtnText = _BtnStr[_CurPage];
		string curHeadline = _HeadlineStr[_CurPage];
		string curMessage = _ContentStr [_CurPage];

		HeadlineTxt.text = curHeadline;
		BodyTxt.text = curMessage;
		BtnTxt.text = curBtnText;

		if (_CurPage < _ContentStr.Length - 1)
		{
			CloseIcon.SetActive (false);
			NextIcon.SetActive (true);
		} else
		{
			CloseIcon.SetActive (true);
			NextIcon.SetActive (false);
		}

		BodyTxt.gameObject.transform.localPosition = new Vector3(_InitialTextPos.x, _InitialTextPos.y - _YPosMove, _InitialTextPos.z);
		BodyTxt.gameObject.transform.DOLocalMove (_InitialTextPos, 0.3f).SetEase(Ease.OutCubic);

	}

	void FadeOutHome()
	{
		try
		{
			//FindObjectOfType<HomeController>().FadeCameraHalfOut();
		}
		catch
		{
			//Debug.Log("HomeController not found, cannot call FadeCameraHalfOut");
		}
	}

	void FadeInHome()
	{
		try
		{
			//FindObjectOfType<HomeController>().FadeCameraIn();
		}
		catch
		{
			//Debug.Log("HomeController not found, cannot call FadeCameraIn");
		}
	}

	void OnEnable()
	{
		ButtonController.OnButtonClicked += ButtonController_OnButtonClicked;
	}

	void OnDisable()
	{
		ButtonController.OnButtonClicked -= ButtonController_OnButtonClicked;
	}

	void ButtonController_OnButtonClicked (object sender)
	{
		if (_CurPage < _ContentStr.Length - 1)
		{
			_CurPage++;
			SetPage ();
		} else
		{
			CloseTutorial ();
		}
	}

}
