using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRStandardAssets.Utils;

public class ResourceManager : MonoBehaviour {

	public VRCameraFade VRCameraFadeObj;
	public Camera MainCamera;
	public Transform GVRControllerPointerTransform;
	public GameObject Reticle, ReticleDaydream;
	public JackalopeMediaPlayer MediaPlayer;
	public TwoDPlayerController TwoDMediaPlayer;
    public OverlayLayerOrdering TimeWarpController;
    public AuthenticationController AuthenticationController;

	void Awake()
	{
		if (ReticleDaydream != null) {
			if (GlobalVars.Instance.IsDaydream) Reticle = ReticleDaydream;
		} 
		else {
			Debug.LogError ("Assign daydream recticle in inspector for resource manager");
		}
	}

	//following PopupController format for singletons which are attached to existing GameObjects
	#region SINGLETON
	public static ResourceManager _instance;
	public static ResourceManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = GameObject.FindObjectOfType<ResourceManager>();
			}

			return _instance;
		}
	}
	#endregion

}
