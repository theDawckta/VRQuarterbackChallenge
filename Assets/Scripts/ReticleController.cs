using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using VRStandardAssets.Utils;
using DG.Tweening;

public class ReticleController : MonoBehaviour
{
	public LayerMask ShowLayers;
	public LayerMask ReticleHideLayers;
	public GameObject ReticleCenter;
	public GameObject ReticleOuter;
	public GameObject ReticleLoader;
	public Image ReticleImageSelector;
	public float GazeDistance;
	public float GazeSelectionTime;
	[HideInInspector]
	public ReticleController Reticle;
	[HideInInspector]
	public Interactible CurrentInteractible;
	[HideInInspector]
	public Interactible PreviousInteractible;
	public float DefaultDistance = 5.0f;
	public Transform GVRControllerPointer;

	private Transform ReticleCamera;
	private Vector3 _originalScale;
	private VRInput _vrInput;
	private Image _reticalCenterImage;
	private Image _reticalOuterImage;
	private Image _reticalLoaderImage;
	private GazeTimer _gazeTimer;
	private ButtonController _buttonController;
	private bool _reticleHidden = false;
	private bool _allowUserInteraction = true;
	private float CurGazeSelectionTime;
	private GameObject _gearVRController;

	void Awake()
	{
        ReticleCamera = Camera.main.transform;
		float distance = (transform.position - ReticleCamera.position).magnitude;
		if (GVRControllerPointer != null)
		{
			distance = (transform.position - GVRControllerPointer.position).magnitude;
		}

		//if we're the reticle that does not have the controller pointer defined
		if (GlobalVars.Instance.IsDaydream && GVRControllerPointer == null)
		{
			this.gameObject.SetActive (false);
		}

		CurGazeSelectionTime = GazeSelectionTime;
		_originalScale = transform.localScale / distance;
		_vrInput = gameObject.GetComponent<VRInput>();
		_reticalCenterImage = ReticleCenter.GetComponent<Image>();
		_reticalOuterImage = ReticleOuter.GetComponent<Image>();
		_reticalLoaderImage = ReticleLoader.GetComponent<Image>();
		_reticalOuterImage.CrossFadeAlpha(0.0f, 0.0f, false);
		_reticalLoaderImage.CrossFadeAlpha(0.0f, 0.0f, false);
	}

	private void Update()
	{

//		Debug.Log ("OVRInput.IsControllerConnected(OVRInput.Controller.RTrackedRemote): " + OVRInput.IsControllerConnected (OVRInput.Controller.RTrackedRemote));
//		Debug.Log ("OVRInput.IsControllerConnected(OVRInput.Controller.LTrackedRemote): " + OVRInput.IsControllerConnected (OVRInput.Controller.LTrackedRemote));
//		Debug.Log ("OVRInput.Controller.RTrackedRemote: " + OVRInput.Controller.RTrackedRemote);
//		Debug.Log ("OVRInput.Controller.LTrackedRemote: " + OVRInput.Controller.LTrackedRemote);

		if (_allowUserInteraction)
		{
			EyeRaycast();
		}

		// for testing loading state of reticle
		//        if(Input.GetKey(KeyCode.Q))
		//        	Loading();
		//        if(Input.GetKey(KeyCode.W))
		//        	DoneLoading();
	}

	private void EyeRaycast()
	{

		if (GVRControllerPointer != null && SceneManager.GetActiveScene().name != "AdScene")
		{
			//Debug.DrawRay(GVRControllerPointer.position, GVRControllerPointer.forward * GazeDistance, Color.red, 0.1f);
		} else
		{
			//Debug.DrawRay(ReticleCamera.position, ReticleCamera.forward * GazeDistance, Color.red, 0.1f);
		}

		Ray ray;
		ray = new Ray(ReticleCamera.position, ReticleCamera.forward);

		RaycastHit hit = new RaycastHit();
		RaycastHit hideHit = new RaycastHit();
		bool showLayersHitOccured = false;
		bool hideReticeHitOccured = false;

		showLayersHitOccured = Physics.Raycast(ray, out hit, GazeDistance, ShowLayers);
		hideReticeHitOccured = Physics.Raycast(ray, out hideHit, GazeDistance, ReticleHideLayers);

        if (showLayersHitOccured)
        {
            Interactible interactible = hit.collider.gameObject.GetComponent<Interactible>();
            _reticalCenterImage.DOFade(1.0f, 0.3f);
            _reticleHidden = false;
            if (interactible == null)
            {
                _reticalOuterImage.DOFade(0.0f, 0.3f);
                if (CurrentInteractible != null && CurrentInteractible != PreviousInteractible)
                {
                    //Debug.Log("No interactible, but have a current one");
                    CurrentInteractible.Out();
                    CurrentInteractible = null;
                }
                DeactiveLastInteractible();
            }
            else
            {
                bool setImmediateClick = false;
                if (interactible != PreviousInteractible)
                {
                    //					// bad check so we do not show the reticle over secret panel, rework when we have better password ui
                    //					if (interactible.gameObject.name != "SecretPanel")
                    //					{
                    //						_reticalOuterImage.DOFade(1.0f, 0.3f);
                    //					}

                    StopCoroutine("GazeSelectCountdown");
                    CurGazeSelectionTime = GazeSelectionTime;
                    ReticleImageSelector.fillAmount = 0.0f;
                    SetGazeValue(0.0f);
                    interactible.Over();

                    if (interactible != CurrentInteractible && CurrentInteractible != PreviousInteractible)
                    {
                        if (CurrentInteractible != null /*&& PreviousInteractible != null*/)
                        {
                            CurrentInteractible.Out();
                            CurrentInteractible = null;
                        }
                    }

                    if (interactible.GazeSelectable)
                    {
                        _gazeTimer = hit.collider.gameObject.GetComponent<GazeTimer>();
                        _buttonController = hit.collider.transform.parent.GetComponent<ButtonController>();
                        if (interactible.UseGazeTime)
                        {
                            CurGazeSelectionTime = interactible.GazeTime;
                        }
                        if (interactible.TriggerImmediateClick)
                        {
                            setImmediateClick = true;
                        }
                        else
                        {
                            StartCoroutine("GazeSelectCountdown");
                        }
                    }

                    if (PreviousInteractible)
                    {
                        DeactiveLastInteractible();
                    }
                }
                PreviousInteractible = CurrentInteractible;
                CurrentInteractible = interactible;
                if (setImmediateClick)
                    HandleClick();
            }
            SetHitPosition(hit);
        }
        else if (hideReticeHitOccured)
        {
            if (CurrentInteractible != null && CurrentInteractible != PreviousInteractible)
            {
                CurrentInteractible.Out();
                CurrentInteractible = null;
            }
            DeactiveLastInteractible();
            if (!_reticleHidden)
            {
                HideReticle();
            }
        }
        else if (!hideReticeHitOccured && !showLayersHitOccured)
        {
            if (CurrentInteractible != null)
			{
                CurrentInteractible.Out();
                CurrentInteractible = null;
            }
            DeactiveLastInteractible();
			_reticalCenterImage.DOFade(1.0f, 0.3f);
			_reticleHidden = false;
			_reticalOuterImage.DOFade(0.0f, 0.3f);
			Quaternion newRot = new Quaternion();
			transform.position = ReticleCamera.position + ReticleCamera.forward * DefaultDistance;
			newRot = ReticleCamera.rotation;
			transform.localScale = _originalScale * DefaultDistance;

			transform.rotation = Quaternion.Lerp (transform.rotation, newRot, 0.2f);
		}
	}

	private void HideReticle()
	{
		_reticalCenterImage.DOFade(0.0f, 0.3f);
		_reticalOuterImage.DOFade(0.0f, 0.3f);
		_reticleHidden = true;
	}

	public void AnimReticleOut()
	{
		_allowUserInteraction = false;
		HideReticle ();
	}

	private void DeactiveLastInteractible()
	{
		if (PreviousInteractible == null)
			return;

		if (PreviousInteractible.GazeSelectable)
		{
			StopCoroutine("GazeSelectCountdown");
			CurGazeSelectionTime = GazeSelectionTime;
			ReticleImageSelector.DOFade(1.0f, 0.3f);
			ReticleImageSelector.fillAmount = 0.0f;
			SetGazeValue(0.0f);
		}

		PreviousInteractible.Out();
		PreviousInteractible = null;
		CurrentInteractible = null;
	}

	private void SetGazeValue(float gazeValue)
	{
		if (_gazeTimer != null)
		{
			_gazeTimer.SetGazeValue(gazeValue);
		}
		if (_buttonController != null)
		{
			_buttonController.SetGazeValue(gazeValue);
		}
	}

	public void SetHitPosition(RaycastHit hit)
	{
		transform.position = hit.point;
		if (GVRControllerPointer == null)
		{
			transform.localScale = _originalScale * hit.distance;
			Vector3 pos = -hit.normal;
			Quaternion newRot = Quaternion.LookRotation (pos);
			transform.rotation = Quaternion.Lerp (transform.rotation, newRot, 0.2f);
		}
	}

	IEnumerator GazeSelectCountdown()
	{
		float timePassed = 0.0f;
		_reticalLoaderImage.DOFade(0.0f, 0.3f);
		while (timePassed < CurGazeSelectionTime)
		{
			if (CurrentInteractible != null)
			{
				if (CurrentInteractible.TriggerReticleLoader)
				{
					if(ReticleImageSelector != null)
						ReticleImageSelector.fillAmount = Mathf.Lerp (0.0f, 1.0f, (timePassed / CurGazeSelectionTime));
				}
			}
			SetGazeValue(Mathf.Lerp(0.0f, 1.0f, (timePassed / CurGazeSelectionTime)));
			timePassed += Time.deltaTime;
			yield return null;
		}
		ReticleImageSelector.fillAmount = 0.0f;
		SetGazeValue(0.0f);

		HandleClick();
	}

	private void HandleUp()
	{
		if (CurrentInteractible != null)
			CurrentInteractible.Up();
	}

	private void HandleDown()
	{
		if (CurrentInteractible != null)
			CurrentInteractible.Down();
	}

	private void HandleClick()
	{
		if (CurrentInteractible != null)
		{
			CurrentInteractible.Click();
			ResetGazeTimer();
		}
	}

	private void HandleDoubleClick()
	{
		if (CurrentInteractible != null)
		{
			CurrentInteractible.DoubleClick();
			ResetGazeTimer();
		}
	}

	private void ResetGazeTimer()
	{
		StopCoroutine("GazeSelectCountdown");
		CurGazeSelectionTime = GazeSelectionTime;
		ReticleImageSelector.fillAmount = 0.0f;
		SetGazeValue(0.0f);
	}

	private void EnableEvents()
	{
		_allowUserInteraction = true;
		if (_vrInput != null)
		{
			_vrInput.OnClick += HandleClick;
			_vrInput.OnDoubleClick += HandleDoubleClick;
			_vrInput.OnUp += HandleUp;
			_vrInput.OnDown += HandleDown;
		}
	}

	private void DisableEvents()
	{
		_allowUserInteraction = false;
		if (_vrInput != null)
		{
			_vrInput.OnClick -= HandleClick;
			_vrInput.OnDoubleClick -= HandleDoubleClick;
			_vrInput.OnUp -= HandleUp;
			_vrInput.OnDown -= HandleDown;
		}
	}

	private void OnEnable()
	{
		EnableEvents();
	}

	private void OnDisable()
	{
		DisableEvents();
	}
}
