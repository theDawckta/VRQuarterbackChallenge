using System;
using System.Collections;
using UnityEngine;

public class BtnCloseAppHandler : MonoBehaviour
{
    private Interactible _Interactible;
    private AudioController _AudioController;

    void Awake()
    {
        _Interactible = GetComponent<Interactible>();

        if (_Interactible == null)
            throw new Exception("An _Interactible must exist on this button.");

        _AudioController = Extensions.GetRequired<AudioController>();
    }

    void OnEnable()
    {
        _Interactible.OnClick += OnCloseAppClick;
    }

    void OnDisable()
    {
        _Interactible.OnClick -= OnCloseAppClick;
    }


    /// <summary>
    /// App close click has occured - dispatch event to close app
    /// </summary>
    void OnCloseAppClick()
    {
        _AudioController.PlayAudio(AudioController.AudioClips.GenericClick);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
        Application.Quit();
#elif UNITY_ANDROID
        OVRManager.instance.ReturnToLauncher();
#endif
    }
}
