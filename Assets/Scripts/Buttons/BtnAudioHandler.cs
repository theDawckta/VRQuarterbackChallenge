using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class BtnAudioHandler : MonoBehaviour
{
    public GameObject BtnAudioOn;
    public GameObject BtnAudioOff;

    private AudioController _AudioController;
    private VolumeController _VolumeController;
    private Interactible _Interactible;

    public bool IsAudioOn
    {
        get { return _VolumeController.IsAudioOn; }
    }

    void Awake()
    {
        if (BtnAudioOn == null)
            throw new Exception("You must define a BtnAudioOn state");

        if (BtnAudioOff == null)
            throw new Exception("You must define a BtnAudioOff state");

        if (!_Interactible)
        {
            _Interactible = GetComponent<Interactible>();
        }

        if (_Interactible == null)
            throw new Exception("You must define an _Interactible state");

        _VolumeController = Extensions.GetRequired<VolumeController>();
        _AudioController = Extensions.GetRequired<AudioController>();

        SetButtons();
    }

    private void SetButtons()
    {
        BtnAudioOn.SetActive(IsAudioOn);
        BtnAudioOff.SetActive(!IsAudioOn);
    }

    void OnEnable()
    {
        _Interactible.OnOver += HandleOver;
        _Interactible.OnOut += HandleOut;
        _Interactible.OnClick += HandleClick;
        _VolumeController.AudioStatusChanged += VolumeController_AudioStatusChanged;
        EventManager.OnHardwareAudioIsMute += OnHardwareAudioMute;
    }

    private void VolumeController_AudioStatusChanged(object sender, AudioStatusEventArgs e)
    {
        SetButtons();

        if (e.IsAudioOn)
            _AudioController.PlayAudio(AudioController.AudioClips.GenericClick);
    }

    void OnDisable()
    {
        _Interactible.OnClick -= HandleClick;
        _Interactible.OnOver -= HandleOver;
        _Interactible.OnOut -= HandleOut;
        EventManager.OnHardwareAudioIsMute -= OnHardwareAudioMute;
        if (!_IsApplicationQuitting)
        {
            _VolumeController.AudioStatusChanged -= VolumeController_AudioStatusChanged;
        }
    }

    private bool _IsApplicationQuitting;
    private void OnApplicationQuit()
    {
        _IsApplicationQuitting = true;
    }

    void HandleOver()
    {
        if (BtnAudioOn.activeSelf)
        {
            BtnAnimations btnAnimations = BtnAudioOn.GetComponent<BtnAnimations>();
            btnAnimations.HandleOver();
        }
        else if (BtnAudioOff.activeSelf)
        {
            BtnAnimations btnAnimations = BtnAudioOff.GetComponent<BtnAnimations>();
            btnAnimations.HandleOver();
        }
    }

    void HandleOut()
    {
        if (BtnAudioOn.activeSelf)
        {
            BtnAnimations btnAnimations = BtnAudioOn.GetComponent<BtnAnimations>();
            btnAnimations.HandleOut();
        }
        else if (BtnAudioOff.activeSelf)
        {
            BtnAnimations btnAnimations = BtnAudioOff.GetComponent<BtnAnimations>();
            btnAnimations.HandleOut();
        }
    }

    void OnHardwareAudioMute(bool isMute)
    {
        if (IsAudioOn == isMute)
        {
            _VolumeController.SetAudioPlaying(!IsAudioOn);
        }
    }

    void HandleClick()
    {
        HandleOut();

        #region Analytics Call
        Analytics.CustomEvent("AudioButtonClicked", new Dictionary<string, object>
        {
            { "IsAudioOn", !IsAudioOn }
        });
        #endregion

        _VolumeController.SetAudioPlaying(!IsAudioOn);
        HandleOut();
    }
}