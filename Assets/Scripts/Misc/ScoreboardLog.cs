using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreboardLog : MonoBehaviour {

	public TextMeshProUGUI TextField; 

	void OnEnable()
	{
		EventManager.OnLogStringEvent += EventManager_OnLogStringEvent;
	}

	void EventManager_OnLogStringEvent (string objectToLog)
	{
		TextField.text = objectToLog;
	}

	void OnDisable()
	{
		EventManager.OnLogStringEvent -= EventManager_OnLogStringEvent;
	}
}
