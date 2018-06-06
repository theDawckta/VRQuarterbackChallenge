using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPivot : MonoBehaviour {

	public BoxCollider Collider;

	void Awake()
	{
		Collider.enabled = false;
	}

	void OnEnable()
	{
		EventManager.OnAllowCameraPivot += EventManager_OnAllowCameraPivot;
	}

	void OnDisable()
	{
		EventManager.OnAllowCameraPivot -= EventManager_OnAllowCameraPivot;
	}

	void EventManager_OnAllowCameraPivot()
	{
		Collider.enabled = true;
	}
}
