using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class BtnMessageBoardHandler : MonoBehaviour {

	private Interactible _Interactible;

	void Awake()
	{
		_Interactible = GetComponent<Interactible>();

		if (_Interactible == null)
			throw new Exception("An _Interactible must exist on this button.");

	}

	void OnEnable()
	{
		_Interactible.OnClick += OnMessageBoardClick;
	}

	void OnDisable()
	{
		_Interactible.OnClick -= OnMessageBoardClick;
	}

	void OnHomeBtnClick()
	{
		#region Analytics Call
		Analytics.CustomEvent("OKButtonClicked", new Dictionary<string, object>
		{
			{ "Operation", "MessageBoardOKClick" }
		});
		#endregion

	}

	void OnMessageBoardClick()
	{
		Debug.Log ("message board clicked");
	}
}
