using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class GlobalControlsController : MonoBehaviour
{
    public BtnHomeHandler BtnHome;
    public BtnBackHandler BtnBack;
    public GameObject CloseButton;
    public bool IsHomeEnvironment = false;
    public bool HideClose = false;

	private CanvasGroup _controlCanvasGroup;

    void Awake()
    {
        if (BtnHome == null)
            throw new Exception("An BtnHome must be defined.");

        if (BtnBack == null)
            throw new Exception("An BtnBack must be defined.");

		_controlCanvasGroup = this.gameObject.GetComponent<CanvasGroup> ();
    }

    void OnEnable()
    {
        EventManager.OnDrawTilesComplete += OnDrawTilesComplete;
    }

    void OnDisable()
    {
        EventManager.OnDrawTilesComplete -= OnDrawTilesComplete;
    }

    void OnDrawTilesComplete(bool isHome)
    {
        //only affect these in the home environ
        if (IsHomeEnvironment)
        {
            if (isHome)
            {
                BtnHome.Disable();
				BtnBack.IsBackBtnClose = true;
				//check if we were in a channel and track if so
				EventManager.Instance.ChannelTriggerEvent();
                //BtnBack.Disable();
            }
            else
            {
                BtnHome.Enable();
                //BtnBack.Enable();
				BtnBack.IsBackBtnClose = false;
            }
        }
    }
}