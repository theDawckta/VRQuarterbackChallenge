using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class VideoSliderHandler : MonoBehaviour
{
    public Slider CurSlider;
    public Camera UICamera;
    public GameObject SliderLeft;
    public GameObject SliderRight;

    private Interactible _Interactible;
    private VideoController _VideoController;

    void Awake()
    {
        if (CurSlider == null)
            throw new Exception("A slider must be defined.");

        if (UICamera == null)
            throw new Exception("A UICamera must be defined.");

        if (SliderLeft == null)
            throw new Exception("A SliderLeft must be defined.");

        if (SliderRight == null)
            throw new Exception("A SliderRight must be defined.");

        _Interactible = GetComponent<Interactible>();

        if (_Interactible == null)
            throw new Exception("An _Interactible must exist on this slider.");

        _VideoController = Extensions.GetRequired<VideoController>();

    }

    void OnEnable()
    {
        _Interactible.OnClick += OnSliderClick;
    }

    void OnDisable()
    {
        _Interactible.OnClick -= OnSliderClick;
    }

    void Update()
    {
        float sliderPos = _VideoController.GetSliderPosition();
        CurSlider.value = sliderPos;
    }

    void OnSliderClick()
    {
        SeekToOnClick();
    }

    void SeekToOnClick()
    {
        RaycastHit rch;
		GameObject reticle = ResourceManager.Instance.Reticle;
		if (GlobalVars.Instance.IsDaydream)
		{
			reticle = ResourceManager.Instance.ReticleDaydream;
		}
        //if (Physics.Raycast(UICamera.transform.position, UICamera.transform.forward, out rch))
		if (Physics.Raycast(reticle.transform.position, reticle.transform.forward, out rch))
        {
            float sliderLen = -(SliderRight.transform.position.x - SliderLeft.transform.position.x);
            float clickProj = -(rch.point.x  - SliderLeft.transform.position.x);
            float fract = clickProj / sliderLen;

            fract = Mathf.Max(0, fract);
            fract = Mathf.Min(fract, 1.0f);

            _VideoController.SeekToPercentageTime(fract);

            #region Analytics Call
            Analytics.CustomEvent("VideoSliderClicked", new Dictionary<string, object>
            {
                { "SliderPercentageClicked",  fract}
            });
            #endregion
        }

		EventManager.Instance.VideoPositionChangedEvent();
    }
}