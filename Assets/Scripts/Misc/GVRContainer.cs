using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GVRContainer : MonoBehaviour {

	// Use this for initialization
	void Awake()
	{

		this.gameObject.SetActive (GlobalVars.Instance.IsDaydream);
	}
}