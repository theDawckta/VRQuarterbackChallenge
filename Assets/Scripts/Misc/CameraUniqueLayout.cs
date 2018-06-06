using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VOKE.VokeApp.DataModel;
using TMPro;
using DG.Tweening;

public class CameraUniqueLayout : MonoBehaviour
{
	public DynamicTexture CourtObj;
	public GameObject CameraPrefab;
	public GameObject vrCastPrefab;
	public GameObject CameraPrefabContainer;
	public GameObject Pivot; //needed to determine how much to move cameras on Y to normalize

	[Header("Camera Navigation")]
	public GameObject CameraNaviation;
	public GameObject CameraLayoutListContainer;
	public DynamicTexture CameraLayoutPrefab;
	//Leaving the text left/right items for now, though not currently enabled in latest designs. Can pull if fully removed
	public TextMeshProUGUI TextLeft;
	public TextMeshProUGUI TextCenter;
	public TextMeshProUGUI TextRight;
	public GameObject BtnLeft;
	public GameObject BtnRight;
	public Interactible BtnCameraNavLeft;
	public Interactible BtnCameraNavRight;
	public GameObject HitArea;


	private Dictionary<string, BtnCameraSelectionHandler> _AllCameras;
	private Dictionary<string, Vector3> _InitialPositions;

	private float _NormalizeXPos = 170.0f;
	private float _NormalizeZ = 135.0f;
	private float _NormalizeY = 50.0f;
	private float _PivotRotation;
	//give the cameras slight rotation to point towards the camera
	private int _CameraRotation = 24;
	private float _YCorrection;
	private int _CurCameraIndex = 0;
	private IList<CameraLayoutUrl> _CameraLayoutList;
	private IList<CameraBtn> _CameraBtnList;
	private IList<DynamicTexture> _CameraTextureList;
	private int _CameraLayoutsLoaded = 0;
	private int _CameraLayoutsTotal = 0;
	private bool _AreTexturesLoaded = false;
	private Vector3 _PivotScale;
	private Vector3 _cameraScale = new Vector3(0.5f, 0.5f, 0.5f);
	private Vector3 _cameraNavScale;

	void Awake()
	{
		CheckRequired(CameraPrefab, "CameraPrefab");
		CheckRequired(vrCastPrefab, "VRCastPrefab");
		CheckRequired(CameraPrefabContainer, "CameraPrefabContainer");
		CheckRequired(Pivot, "Pivot");
		CheckRequired(CameraNaviation, "CameraNaviation");
		CheckRequired(TextLeft, "TextLeft");
		CheckRequired(TextCenter, "TextCenter");
		CheckRequired(TextRight, "TextRight");
		CheckRequired(BtnLeft, "BtnLeft");
		CheckRequired(BtnRight, "BtnRight");
		CheckRequired(CameraLayoutListContainer, "CameraLayoutListContainer");
		CheckRequired(CameraLayoutPrefab, "CameraLayoutPrefab");
		_PivotRotation = Pivot.transform.localEulerAngles.x;
		_YCorrection = _NormalizeZ / _PivotRotation;
		CameraNaviation.SetActive (false);
		_CameraBtnList = new List<CameraBtn> ();
		_CameraTextureList = new List<DynamicTexture> ();
		CourtObj.gameObject.SetActive (false);
		_CameraLayoutsLoaded = 0;
		_CameraLayoutsTotal = 0;
		_PivotScale = Pivot.transform.localScale;
		_cameraNavScale = BtnLeft.transform.localScale;
	}

	private void CheckRequired(object thing, string name)
	{
		if (thing == null)
			throw new Exception(String.Format("A {0} is required to run this scene.", name));
	}
		
	public void ClearCameras()
	{
		if (_CameraBtnList != null)
		{
			if (_CameraBtnList.Count > 0)
				_CameraBtnList.Clear ();
		}
	}

	public void SetHitPosition(float zPos)
	{
		//HitArea.transform.localPosition = new Vector3 (HitArea.transform.localPosition.x, HitArea.transform.localPosition.y, zPos * 50);

		HitArea.transform.SetParent (CameraPrefabContainer.transform, false);

		Vector3 newPosition = HitArea.transform.localPosition;


		if (zPos != null)
		{
			newPosition.z += (zPos * _NormalizeZ);
			newPosition.y += (zPos * -1) * _YCorrection;
		}

		HitArea.transform.localPosition = newPosition;
	}

	void DestroyCameraTextures()
	{
		foreach (Transform child in CameraLayoutListContainer.transform)
		{
			GameObject.Destroy(child.gameObject);
		}
		foreach (Transform child in CameraLayoutListContainer.transform)
		{
			GameObject.Destroy(child.gameObject);
		}
		_CameraTextureList.Clear ();
	}

	public void EnableCameraLayoutNavigation(ContentViewModel intent) //IList<CameraLayoutUrl> CameraLayoutList) 
	{
		//if we have an activeLayout already, we should keep the user on it
		string activeLayout = GetActiveLayoutID ();

		//since we're using our multiple camera setup, we need to hide the pivot until we're loaded
		Pivot.transform.localScale = new Vector3(0,0,0);
		_CameraLayoutList = intent.CameraLayoutList;

		if (!string.IsNullOrEmpty (activeLayout))
			SetCurrentCameraIndex (activeLayout);

		if (_CameraLayoutList.Count > 0)
		{
			//set our camera textures
			SetCameraTextures();
			//display our navigation to swap between camera layouts in page
			CameraNaviation.SetActive (true);
			SetCameraNavigation ();
		}
	}

	//return the ID of the active camera layout, if one exists
	public string GetActiveLayoutID()
	{
		string activeID = "";
		if (_CameraLayoutList != null)
		{
			if (_CameraLayoutList.Count > 0)
			{
				if (!string.IsNullOrEmpty (_CameraLayoutList [(_CurCameraIndex)].ID))
					activeID = _CameraLayoutList [(_CurCameraIndex)].ID;
			}
		}
		return activeID;
	}

	/// <summary>
	/// check to see if an active layout exists
	/// </summary>
	/// <param name="activeLayout">Active layout.</param> - string - the currently active layout
	void SetCurrentCameraIndex(string activeLayout)
	{
		_CurCameraIndex = 0;
		for(int i=0;i<_CameraLayoutList.Count; i++)
		{
			CameraLayoutUrl camera = _CameraLayoutList [i];
			if (camera.ID == activeLayout)
			{
				_CurCameraIndex = i;
				break;
			}
		}
	}

	/// <summary>
	/// Set the multiple camera textures based on CameraLayoutList
	/// </summary>
	void SetCameraTextures()
	{
		DestroyCameraTextures ();
		_AreTexturesLoaded = false;
		_CameraLayoutsLoaded = 0;
		_CameraLayoutsTotal = 0;
		foreach (var cameraLayout in _CameraLayoutList)
		{
			if(!string.IsNullOrEmpty(cameraLayout.Url))
			{
				//in theory should not need this and should be able to use list length - this provides a check if textures are not defined correctly
				_CameraLayoutsTotal++;
				DynamicTexture cameraLayoutImage = Instantiate(CameraLayoutPrefab) as DynamicTexture;
				cameraLayoutImage.transform.SetParent(CameraLayoutListContainer.transform, false);
				_CameraTextureList.Add (cameraLayoutImage);
				cameraLayoutImage.ID = cameraLayout.ID;
				cameraLayoutImage.SetTexture(cameraLayout.Url, false, TextureLoaded);
			}
		}
	}

	/// <summary>
	/// Backwards compatibility for setting a single camera texture based on CameraLayoutUrl
	/// </summary>
	/// <param name="textureUrl">Texture URL.</param>
	public void SetTexture(string textureUrl)
	{
		if (CourtObj != null)
		{
			CourtObj.gameObject.SetActive (true);
			CourtObj.SetTexture(textureUrl);
		}
	}
		
	/// <summary>
	/// callback function called when single court texture is loaded 
	/// </summary>
	void TextureLoaded()
	{
		_CameraLayoutsLoaded++;
		if (_CameraLayoutsLoaded == _CameraLayoutsTotal)
		{
			_AreTexturesLoaded = true;
			//images are fully loaded, we can now show everything
			SetTextureByID();
			Pivot.transform.localScale = _PivotScale;
		}
	}

	/// <summary>
	/// set the camera navigation according to the index of the camera item we are currently on
	/// </summary>
	void SetCameraNavigation(bool triggerCameras = false)
	{
		TextLeft.text = "";
		TextCenter.text = "";
		TextRight.text = "";

		DOTween.Kill (BtnLeft.gameObject);
		DOTween.Kill (TextCenter.gameObject);
		DOTween.Kill (BtnRight.gameObject);

		float randomDelay = 0.0f;

		if (_CurCameraIndex > 0)
		{
			BtnCameraNavLeft.gameObject.SetActive (true);
			BtnLeft.gameObject.SetActive (true);
			TextLeft.text = _CameraLayoutList [(_CurCameraIndex - 1)].Name.ToUpper();

			randomDelay = UnityEngine.Random.Range(0.0f,0.3f);
			BtnLeft.gameObject.transform.localScale = new Vector3(0,0,0);
			BtnLeft.gameObject.transform.DOScale(_cameraNavScale, 0.3f).SetDelay(randomDelay).SetEase(Ease.OutBack);
		} else
		{
			BtnCameraNavLeft.gameObject.SetActive (false);
			BtnLeft.gameObject.SetActive (false);
		}

		TextCenter.text = _CameraLayoutList [(_CurCameraIndex)].Name.ToUpper();
		randomDelay = UnityEngine.Random.Range(0.0f,0.3f);
		TextCenter.gameObject.transform.localScale = new Vector3(0,0,0);
		TextCenter.gameObject.transform.DOScale(new Vector3(1,1,1), 0.3f).SetDelay(randomDelay).SetEase(Ease.OutBack);

		SetCamerasByID (triggerCameras);

		if(_AreTexturesLoaded)
			SetTextureByID ();

		if ((_CurCameraIndex + 1) < _CameraLayoutList.Count)
		{
			BtnCameraNavRight.gameObject.SetActive (true);
			BtnRight.gameObject.SetActive (true);
			TextRight.text = _CameraLayoutList [(_CurCameraIndex + 1)].Name.ToUpper();

			randomDelay = UnityEngine.Random.Range(0.0f,0.3f);
			BtnRight.gameObject.transform.localScale = new Vector3(0,0,0);
			BtnRight.gameObject.transform.DOScale(_cameraNavScale, 0.3f).SetDelay(randomDelay).SetEase(Ease.OutBack);
		} else
		{
			BtnCameraNavRight.gameObject.SetActive (false);
			BtnRight.gameObject.SetActive (false);
		}
	}

	void SetTextureByID()
	{
		string activeLayout = GetActiveLayoutID ();

		foreach(DynamicTexture cameraTexture in _CameraTextureList)
		{
			DOTween.Kill (cameraTexture.gameObject);
			if (cameraTexture.ID != activeLayout)
			{
				cameraTexture.gameObject.SetActive (false);
			} else
			{
				cameraTexture.gameObject.SetActive (true);
			}
		}
	}

	/// <summary>
	/// loop through our list of cameras and hide any that do not match the current live ID
	/// camera without an ID should be considered "default"
	/// </summary>
	void SetCamerasByID(bool triggerCamera = false)
	{
		string activeLayout = GetActiveLayoutID ();
		CameraBtn cameraToTrigger = null;
		int counter = 0;
		bool isSetByVRCast = false;
		foreach(CameraBtn camera in _CameraBtnList)
		{
			//check to see if we currently have an active VRCast camera with no ID - if we do we should not trigger camera
			if (camera.IsActive && camera.IsVRCastCamera && string.IsNullOrEmpty (camera.ID))
				triggerCamera = false;

			if (!string.IsNullOrEmpty (camera.ID))
			{
				DOTween.Kill (camera.gameObject);
				if (camera.ID != activeLayout)
				{
					camera.gameObject.transform.localScale = new Vector3 (0, 0, 0);
				} else
				{
					if (counter == 0 || camera.IsVRCastCamera)
					{
						cameraToTrigger = camera;
						if (camera.IsVRCastCamera)
							isSetByVRCast = true;
					}
					
					counter++;
					float randomDelay = UnityEngine.Random.Range (0.0f, 0.3f);
					camera.gameObject.transform.localScale = new Vector3 (0, 0, 0);
					camera.gameObject.transform.DOScale (_cameraScale, 0.3f).SetDelay (randomDelay).SetEase (Ease.OutBack);
				}
			}
		}

		//need to check for non-ID'd (global) VR cameras and push to there if we haven't been set by VRCast
		if(!isSetByVRCast)
		{
			foreach (CameraBtn camera2 in _CameraBtnList)
			{
				if(camera2.IsVRCastCamera && string.IsNullOrEmpty(camera2.ID))
				{
					cameraToTrigger = camera2;
					break;
				}
			}
		}

		if (cameraToTrigger != null && triggerCamera)
		{
			Interactible camInteractible = cameraToTrigger.GetComponent<Interactible> ();
			if (camInteractible != null)
				camInteractible.Click ();
		}
	}
		
	public void SetCamera(CameraStreamModel model)
	{
		Vector3 newPos = new Vector3(0, 0, 0);

		GameObject curCamera;
		if (model.IsVRCastCamera)
			curCamera = Instantiate(vrCastPrefab, newPos, Quaternion.identity) as GameObject;
		else
			curCamera = Instantiate(CameraPrefab, newPos, Quaternion.identity) as GameObject;   

		//        curCamera.transform.parent = CameraPrefabContainer.transform;
		curCamera.transform.SetParent (CameraPrefabContainer.transform, false);
		curCamera.transform.localRotation = Quaternion.identity;
		curCamera.transform.localPosition = newPos;
		curCamera.transform.localScale = _cameraScale;

		CameraBtn cameraBtn = curCamera.GetComponent<CameraBtn> ();
		cameraBtn.ID = model.ID;
		cameraBtn.IsVRCastCamera = model.IsVRCastCamera;

		_CameraBtnList.Add (curCamera.GetComponent<CameraBtn> ());

		BtnCameraSelectionHandler camSelection = curCamera.GetComponent<BtnCameraSelectionHandler>();
		if (model != null)
		{
			camSelection.MediaURL = model.Url;
			camSelection.SetLabel(model.Label);
		}

		//reposition the cams if we have positioning values
		if ((model.OffsetX ?? model.OffsetY ?? model.OffsetZ) != null)
		{
			Vector3 newPosition = curCamera.transform.localPosition;

			if (model.OffsetX != null)
			{
				newPosition.x += (model.OffsetX.Value * _NormalizeXPos);
			}

			if (model.OffsetZ != null)
			{
				newPosition.z += (model.OffsetZ.Value * _NormalizeZ);
				newPosition.y += (model.OffsetZ.Value * -1) * _YCorrection;
			}

			if (model.OffsetY != null)
			{
				newPosition.y += (model.OffsetY.Value * _NormalizeY);
			}

			curCamera.transform.localPosition = newPosition;
		}
		curCamera.transform.Rotate(_CameraRotation, 180, 0);

		//If there's a rotation attribute then we only need to rotate the Camera icons, not the entire object
		if (model.Rotation != null)
		{
			if (curCamera.GetComponent<BtnCameraSelectionHandler>() != null)
			{
				BtnCameraSelectionHandler comp = curCamera.GetComponent<BtnCameraSelectionHandler>();
				Quaternion finalRotation = Quaternion.Euler(0f, 0f,model.Rotation.Value);
				comp.CameraNormal.transform.localRotation = finalRotation;
				comp.CameraActive.transform.localRotation = finalRotation;
				comp.CameraSelected.transform.localRotation = finalRotation;
			}
		}
	}

	/// <summary>
	/// Trigger a click on either our first camera, or the first instance of a VR camera
	/// </summary>
	public void TriggerFirstOrVRCast()
	{
		CameraBtn firstOrVR = null;
		if (_CameraBtnList != null)
		{
			for (int i = 0; i < _CameraBtnList.Count; i++)
			{
				if (i == 0 || _CameraBtnList[i].IsVRCastCamera)
				{
					firstOrVR = _CameraBtnList [i];

					if (_CameraBtnList [i].IsVRCastCamera)
						break;
				}
			}
			//trigger a click on this camera
			if (firstOrVR != null)
			{
				Interactible camInteractible = firstOrVR.GetComponent<Interactible> ();
				if (camInteractible != null)
				{
					camInteractible.Click ();
				}
			}
		}
	}

	void OnEnable()
	{
		if(BtnCameraNavLeft != null)
			BtnCameraNavLeft.OnClick += BtnCameraNavLeft_OnClick;

		if(BtnCameraNavRight != null)
			BtnCameraNavRight.OnClick += BtnCameraNavRight_OnClick;
	}

	void BtnCameraNavRight_OnClick ()
	{
		_CurCameraIndex++;
		if (_CurCameraIndex > _CameraLayoutList.Count - 1)
			_CurCameraIndex = _CameraLayoutList.Count - 1;

		SetCameraNavigation (true);
	}

	void BtnCameraNavLeft_OnClick ()
	{
		_CurCameraIndex--;
		if (_CurCameraIndex < 0)
			_CurCameraIndex = 0;
		SetCameraNavigation (true);
	}

	void OnDisable()
	{
		if(BtnCameraNavLeft != null)
			BtnCameraNavLeft.OnClick -= BtnCameraNavLeft_OnClick;

		if(BtnCameraNavRight != null)
			BtnCameraNavRight.OnClick -= BtnCameraNavRight_OnClick;
	}
}