using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class BtnAnimations : MonoBehaviour
{
    public GameObject BtnNormal;
    public GameObject BtnRollover;
    public float ScaleSize = 1.2f;
    public bool IsActive = true;

    private Interactible _Interactible;
    private Vector3 _startScaleSize;
    private Vector3 _endScaleSize;

    void Awake()
    {
        if (BtnNormal != null)
        {
            BtnNormal.SetActive(true);
        }
        if (BtnRollover != null)
        {
            BtnRollover.SetActive(false);
        }

        _Interactible = gameObject.GetComponent<Interactible>();
        _startScaleSize = gameObject.transform.localScale;
		//make sure we're not zero
		if (_startScaleSize == new Vector3 (0, 0, 0))
		{
			_startScaleSize = new Vector3 (1, 1, 1);
		}
        //our end scale size is a percentage of our start
		_endScaleSize = new Vector3(_startScaleSize.x * ScaleSize, _startScaleSize.y * ScaleSize, _startScaleSize.z * ScaleSize);
    }

    void OnEnable()
    {
        if (_Interactible != null)
        {
            _Interactible.OnOver += HandleOver;
            _Interactible.OnOut += HandleOut;
        }
    }

    void OnDisable()
    {
		BtnNormal.SetActive(true);
		BtnRollover.SetActive(false);
		transform.localScale = _startScaleSize;
        if (_Interactible != null)
        {
            _Interactible.OnOver -= HandleOver;
            _Interactible.OnOut -= HandleOut;
        }
    }

    public void HandleOver()
    {
    	transform.DOScale(_endScaleSize, 0.3f).SetEase(Ease.OutBack);
        if (IsActive)
        {
            ToggleOver(true);
        }
    }

    public void HandleOut()
    {
        if (IsActive)
        {
            ToggleOver(false);
        }
        transform.DOScale(_startScaleSize, 0.3f).SetEase(Ease.OutBack);
    }

    void ToggleOver(bool isOver)
    {
        if (BtnNormal != null)
        {
            BtnNormal.SetActive(!isOver);
        }
        if (BtnRollover != null)
        {
            BtnRollover.SetActive(isOver);
        }
    }

    public void Enable()
    {
        IsActive = true;
        _Interactible.OnOver += HandleOver;
        _Interactible.OnOut += HandleOut;
        _Interactible.Enable();
        BtnNormal.SetActive(true);
        BtnRollover.SetActive(false);
    }

    public void Disable()
    {
        IsActive = false;
        HandleOut();
        _Interactible.OnOver -= HandleOver;
        _Interactible.OnOut -= HandleOut;
        _Interactible.Disable();
        BtnNormal.SetActive(false);
        BtnRollover.SetActive(true);
    }
}