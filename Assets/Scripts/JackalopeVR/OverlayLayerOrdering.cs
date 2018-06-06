using UnityEngine;
using System.Collections;

public class OverlayLayerOrdering : MonoBehaviour {

	public OVROverlay[] overlays = new OVROverlay[] { null, null};

	// Use this for initialization
	void Awake () 
	{
		for (int i = 0; i < overlays.Length; i++) 
		{
			overlays [i].gameObject.SetActive (true);
		}
	}	
}
