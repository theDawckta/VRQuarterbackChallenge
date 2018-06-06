using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class ShowHideControl : MonoBehaviour
{
    public Camera UICamera;
    public LayerMask ShowHideLayerMask;
    public GameObject Pivot;
    public GameObject ControlsHolder;
    public bool MoveItem;
    public float MoveTime = 0.5f;
    public Vector3 MoveDelta;
    public bool RotateItem;
    public float RotateTime = 0.5f;
    public Vector3 RotateDelta;
    public Vector3 ColliderPosition;
    public Vector3 ColliderSize;
	public bool OpenIfStatsOpen = true;
	public bool ResizeOnComplete = true;

    private bool _gazed = false;
    private bool _newGaze = false;
    private Vector3 _originalPosition;
    private Vector3 _originalRotation;
    private Transform _cameraTransform;
    private BoxCollider _boxCollider;
    private Vector3 _boxColliderOriginalSize;
    private Vector3 _boxColliderOriginalPosition;
    private Vector3 _boxColliderOriginalRotation;

    void Start()
    {
        _boxCollider = gameObject.GetComponent<BoxCollider>();
        _cameraTransform = UICamera.transform;

		transform.LookAt(_cameraTransform.position);
        if (_boxCollider)
        {
	          _boxColliderOriginalSize = _boxCollider.size;
            _boxColliderOriginalPosition = transform.position;
            _boxColliderOriginalRotation = transform.eulerAngles;
        }
        _originalPosition = Pivot.transform.localPosition;
        _originalRotation = Pivot.transform.localEulerAngles;

		if(OVRInput.IsControllerConnected(OVRInput.Controller.RTrackedRemote) || OVRInput.IsControllerConnected(OVRInput.Controller.LTrackedRemote))
		{
			if(!MoveItem)
			{
				RotateDelta = new Vector3(-20.0f, 0f, 0f);
			}
			else
			{
				MoveDelta = new Vector3(0f, 250f, 0f);
			}
		}

    }
		
    void Update()
    {

		if (OpenIfStatsOpen || (!OpenIfStatsOpen && GlobalVars.Instance.IsAllowControlsOpen))
		{
			RaycastHit hit;
			if (Physics.Raycast (_cameraTransform.position, _cameraTransform.forward, out hit, 1000.0f, ShowHideLayerMask))
			{
				_newGaze = true;
			} else
			{
				_newGaze = false;
			}

			if (_gazed != _newGaze)
			{
				_gazed = _newGaze;
				if (_newGaze == true)
				{
					if (MoveItem)
					{
						Pivot.transform.DOLocalMove(_originalPosition + MoveDelta, MoveTime).OnStart(OnAnimationStart).OnComplete(OnAnimationShowComplete).SetEase(Ease.OutCubic);
					}
					if (RotateItem)
					{
						if (_boxCollider)
						{
							transform.position = _boxColliderOriginalPosition + ColliderPosition;
							_boxCollider.size = _boxCollider.size + ColliderSize;
						}
						Pivot.transform.DOLocalRotate(_originalRotation + RotateDelta, RotateTime).OnStart(OnAnimationStart).OnComplete(OnAnimationShowComplete).SetEase(Ease.OutCubic);
					}
				} else
				{
					if (MoveItem)
					{
						Pivot.transform.DOLocalMove(_originalPosition, MoveTime).OnComplete(OnAnimationHideComplete).SetEase(Ease.OutCubic);
					}
					if (RotateItem)
					{
						if (_boxCollider)
						{
							transform.position = _boxColliderOriginalPosition;
							_boxCollider.size = _boxColliderOriginalSize;
						}
						Pivot.transform.DOLocalRotate(_originalRotation, RotateTime).OnComplete(OnAnimationHideComplete).SetEase(Ease.OutCubic);
					}
				}
			}
		} else
		{
			//close us 
			if (MoveItem)
			{
				Pivot.transform.DOLocalMove(_originalPosition, MoveTime).OnComplete(OnAnimationHideComplete).SetEase(Ease.OutCubic);
			}
			if (RotateItem)
			{
				if (_boxCollider)
				{
					transform.position = _boxColliderOriginalPosition;
					_boxCollider.size = _boxColliderOriginalSize;
				}
				Pivot.transform.DOLocalRotate(_originalRotation, RotateTime).OnComplete(OnAnimationHideComplete).SetEase(Ease.OutCubic);
			}
		}

    }

    void OnAnimationStart()
    {
        if (_boxCollider)
            _boxCollider.size = _boxCollider.size * 2.0f;
    }

    void OnAnimationShowComplete()
    {
        if (ControlsHolder)
        {
            if (_boxCollider)
            {
				//do not resize for the LinearHideShow
				if (ResizeOnComplete)
				{
					_boxCollider.size = _boxColliderOriginalSize + ColliderSize;
					transform.position = ControlsHolder.transform.position + ColliderPosition;
					transform.rotation = ControlsHolder.transform.rotation;
				}
            }
        }
    }

    void OnAnimationHideComplete()
    {
        if (_boxCollider)
        {
            _boxCollider.size = _boxColliderOriginalSize;
            transform.position = _boxColliderOriginalPosition;
            transform.eulerAngles = _boxColliderOriginalRotation;
        }
    }
}