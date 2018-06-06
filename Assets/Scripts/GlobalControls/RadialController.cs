using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class RadialController : MonoBehaviour {

	public GameObject ParentGameObject;
	public List<RadialItem> RadialItems;
	public float Radius;
	public float TotalAngle = 360.0f;	//how much room do we have to work with? Full circle?
	public float StartAngle = 0.0f;		//where to start the camera angle from
	public LayerMask ShowHideLayerMask;	//which layer mask should we use to determine hits
	public bool UseHitBoxForInteraction;
	public bool UseZAxis;
	public bool AllowHit = true;
	[Header("Angle Layout")]
	public bool UseAngleSpacing = false;		//instead of giving us a range to fit inside, place items X apart
	public float AngleSpacing = 15.0f;
	public OVRGearVrController LeftController;
	public OVRGearVrController RightController;


	private ButtonController _parentButtonController;
	private bool _isMenuOpen = false;
	private bool _isAnimating = false;
	private float _animSpeed = 0.3f;
	private float _animDelay = 0.1f;
	private Transform _cameraTransform;
	private Vector3 _btnScale = new Vector3(1,1,1);
	private Vector3 _btnHideScale = new Vector3(0,0,0);
	private bool _IsInited = false;
	private GameObject _gearVRController;

	void Awake()
	{
#if HideFeedbackButton
        RadialItem feedBackButton = RadialItems.SingleOrDefault(r => r.gameObject.name == "BtnFeedbackContainer");
        if(feedBackButton != null)
        {
            feedBackButton.gameObject.SetActive(false);
            RadialItems.Remove(feedBackButton);
        }
#endif

        if (ResourceManager.Instance.MainCamera == null)
			throw new Exception ("A MainCamera in ResourceManager is required for RadialController");

		if(ResourceManager.Instance.GVRControllerPointerTransform == null)
			throw new Exception ("A GVRControllerPointerTransform in ResourceManager is required for RadialController");

		//some of our radial menus are created dynamically - if we're one of those we should not need to step through init

		_parentButtonController = ParentGameObject.GetComponent<ButtonController> ();
		if (GlobalVars.Instance.IsDaydream)
		{
			_cameraTransform = ResourceManager.Instance.GVRControllerPointerTransform;
		} else
		{
			_cameraTransform = ResourceManager.Instance.MainCamera.transform;
		}
		//_gearVRController = GameObject.FindObjectOfType<OVRGearVrController>().m_model;

		if (RadialItems.Count > 0)
		{
			_btnScale = RadialItems [0].transform.localScale;
			Init ();
		}
	}

	void Init()
	{
		float angleSpacing = TotalAngle/(RadialItems.Count - 1);
		float convertToRad = Mathf.PI / 180;

		if (UseAngleSpacing)
		{
			angleSpacing = AngleSpacing;
			StartAngle = (180 - (angleSpacing * (RadialItems.Count - 1)))/2;
		}

		//before initializing, reverse our list
		RadialItems.Reverse();
		//need to determine our start and end positions
		for (int i = 0; i < RadialItems.Count; i++)
		{
			RadialItem curRadialItem = RadialItems [i];
			curRadialItem.StartX = ParentGameObject.transform.localPosition.x;
			curRadialItem.StartY = ParentGameObject.transform.localPosition.y;
			curRadialItem.StartZ = ParentGameObject.transform.localPosition.z;
			float angleIncrement = angleSpacing * i;
			float endX = Mathf.Cos ((angleIncrement + StartAngle) * convertToRad) * Radius;
			float endY = Mathf.Sin ((angleIncrement + StartAngle) * convertToRad) * Radius;
			curRadialItem.EndX = endX;
			if (UseZAxis)
			{
				curRadialItem.EndZ = endY;
				curRadialItem.EndY = curRadialItem.StartY;
			} else
			{
				curRadialItem.EndZ = curRadialItem.StartZ;
				curRadialItem.EndY = endY;
			}
			curRadialItem.gameObject.transform.localPosition = new Vector3 (curRadialItem.StartX, curRadialItem.StartY, curRadialItem.StartZ);
			curRadialItem.gameObject.transform.localScale = _btnHideScale;
		}
		_IsInited = true;
	}


	public void SetBtnScale(Vector3 btnScale)
	{
		_btnScale = btnScale;
	}

	public void ResetRadialItemPositions(List<RadialItem> curRadialList, bool reopenMenu = false)
	{
		//if there is a current radial list, reset the items
		for (int i = 0; i < RadialItems.Count; i++)
		{
			RadialItem curRadialItem = RadialItems [i];
			curRadialItem.gameObject.transform.localPosition = new Vector3 (curRadialItem.StartX, curRadialItem.StartY, curRadialItem.StartZ);
			curRadialItem.gameObject.transform.localScale = _btnHideScale;
		}
		RadialItems = curRadialList;
		Init ();
		if (_isMenuOpen && reopenMenu)
		{
			_isMenuOpen = false;
		}
	}

	public void Disable()
	{
		_parentButtonController.Disable();
	}

	public void Enable()
	{
		_parentButtonController.Enable();
	}

	//**************
	// METHODS
	//**************

	void Update()
	{
		//should only set the gvr controller once
		if (_gearVRController == null)
		{
			if (OVRInput.IsControllerConnected (OVRInput.Controller.RTrackedRemote))
			{
				if (RightController != null)
					_gearVRController = RightController.m_model;
			} else if (OVRInput.IsControllerConnected (OVRInput.Controller.LTrackedRemote))
			{
				if (LeftController != null)
					_gearVRController = LeftController.m_model;
			}
		}


		if (RadialItems.Count > 0 && _IsInited)
		{
			RaycastHit hit;
			if((OVRInput.IsControllerConnected(OVRInput.Controller.RTrackedRemote) || OVRInput.IsControllerConnected(OVRInput.Controller.LTrackedRemote)) && _gearVRController!=null && Physics.Raycast(_gearVRController.transform.position, _gearVRController.transform.forward, out hit, 1000.0f, ShowHideLayerMask) && AllowHit)
			{
				if (!_isMenuOpen && !_isAnimating && UseHitBoxForInteraction)
				{
					_isMenuOpen = true;
					AnimMenuOpen ();
				}
			}
			else if (Physics.Raycast (_cameraTransform.position, _cameraTransform.forward, out hit, 1000.0f, ShowHideLayerMask) && AllowHit)
			{
				//if used to open menu
				if (!_isMenuOpen && !_isAnimating && UseHitBoxForInteraction)
				{
					_isMenuOpen = true;
					AnimMenuOpen ();
				}
			} else
			{
				//close out the menu if open
				if (_isMenuOpen && !_isAnimating)
				{
					_isMenuOpen = false;
					AnimMenuClosed ();
				}
			}
		}
	}

	void AnimMenuClosed()
	{
		_isAnimating = true;
		//this could be static, however in various scenes not all radial items may be showing
		int totalRadialItems = RadialItems.Count;
		float animCompleteDelay = _animSpeed + (_animDelay * totalRadialItems);
		for (int i = 0; i < totalRadialItems; i++)
		{
			RadialItem curRadialItem = RadialItems [i];
			Vector3 newPos = new Vector3 (curRadialItem.StartX, curRadialItem.StartY, curRadialItem.StartZ);
			Interactible curInteractible = curRadialItem.GetComponent<Interactible> ();
			if (curInteractible != null)
			{
				DisableItem (curInteractible, true);
			}
			curRadialItem.transform.DOLocalMove(newPos, _animSpeed).SetDelay(_animDelay * i).SetEase(Ease.InCubic).OnComplete(()=>OnAnimMenuClosedComplete(curRadialItem));
		}
		StartCoroutine (AnimationDelayComplete (animCompleteDelay));
	}

	void AnimMenuOpen()
	{
		_isAnimating = true;
		//this could be static, however in various scenes not all radial items may be showing
		int totalRadialItems = RadialItems.Count;
		float animCompleteDelay = _animSpeed + (_animDelay * totalRadialItems);
		for (int i = 0; i < totalRadialItems; i++)
		{
			RadialItem curRadialItem = RadialItems [i];
			curRadialItem.gameObject.transform.localScale = _btnScale;
			Vector3 newPos = new Vector3 (curRadialItem.EndX, curRadialItem.EndY, curRadialItem.EndZ);
			Interactible curInteractible = curRadialItem.GetComponent<Interactible> ();
			if (curInteractible != null)
			{
				DisableItem (curInteractible, true);
			}
			curRadialItem.transform.DOLocalMove(newPos, _animSpeed).SetDelay(_animDelay * i).SetEase(Ease.OutCubic).OnComplete(()=>OnAnimMenuOpenComplete(curRadialItem));
		}
		StartCoroutine (AnimationDelayComplete (animCompleteDelay));
	}

	IEnumerator AnimationDelayComplete(float delay)
	{
		yield return new WaitForSeconds(delay);
		_isAnimating = false;
	}

	void DisableItem(Interactible animInteractible, bool isDisabled)
	{
		if (isDisabled)
		{
			animInteractible.Disable ();
		} else
		{
			CanvasGroup interactibleCanvas = animInteractible.GetComponent<CanvasGroup> ();
			if (interactibleCanvas != null)
			{
				//if our alpha is not 1, we're meant to be disabled
				if (interactibleCanvas.alpha == 1)
				{
					animInteractible.Enable ();
				}
			} else
			{
				animInteractible.Enable ();
			}
		}
	}

	void ToggleMenu()
	{

		if (_isMenuOpen && !_isAnimating)
		{
			_isMenuOpen = false;
			AnimMenuClosed ();
		} else if(!_isMenuOpen && !_isAnimating)
		{
			_isMenuOpen = true;
			AnimMenuOpen ();
		}
	}

	//**************
	// EVENTS
	//**************

	void OnEnable()
	{
		if(_parentButtonController != null) _parentButtonController.OnButtonClicked += OnParentClick;
	}

	void OnDisable()
	{
		if(_parentButtonController != null) _parentButtonController.OnButtonClicked -= OnParentClick;
	}

	void OnParentClick(object sender)
	{
		//only toggle open, never closed
		if(!_isMenuOpen && !_isAnimating)
			ToggleMenu ();
	}

	void OnAnimMenuClosedComplete(RadialItem curRadialItem)
	{
		curRadialItem.gameObject.transform.localScale = _btnHideScale;
	}

	void OnAnimMenuOpenComplete(RadialItem curRadialItem)
	{
		Interactible curInteractible = curRadialItem.GetComponent<Interactible> ();
		if (curInteractible != null)
		{
			DisableItem (curInteractible, false);
		}
	}
}
