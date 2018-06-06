using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class BtnTextSwapAnim: MonoBehaviour {

	public Text TextElement;
	public GameObject Icon;
	public ButtonController ButtonController;

	// Use this for initialization
	void Awake () {
		if (TextElement == null)
			throw new Exception ("You must define a TextElement for BtnTextSwapAnim.");
		if (Icon == null)
			throw new Exception ("You must define an Icon for BtnTextSwapAnim.");
		if (ButtonController == null)
			throw new Exception ("You must define a ButtonController for BtnTextSwapAnim.");

		TextElement.gameObject.SetActive (false);
	}


	void OnEnable()
	{
		ButtonController.OnButtonOver += OnButtonOver;
		ButtonController.OnButtonOut += OnButtonOut;
	}

	void OnDisable()
	{
		ButtonController.OnButtonOver -= OnButtonOver;
		ButtonController.OnButtonOut -= OnButtonOut;
	}

	void OnButtonOver(object sender)
	{
		TextElement.gameObject.SetActive (true);
		Icon.SetActive (false);
	}

	void OnButtonOut(object sender)
	{
		TextElement.gameObject.SetActive (false);
		Icon.SetActive (true);
	}
}
