using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;

using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class PopupController : MonoBehaviour
{
    public TextMeshProUGUI Message;
    public CanvasGroup Popup;
    public GameObject ButtonArea;
    public GameObject ButtonPrefab;
    public GameObject DividerPrefab;

    public float PopupAnimationSpeed = 0.2f;

    private AudioController _Audio;
    private Collider clickBlock;

    //Not using template as this singleton is associated with a prefab and hence needs different behabiour, unqie to each scene but still a singleton
    #region SINGLETON
    public static PopupController _instance;

    public static PopupController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PopupController>();
            }

            return _instance;
        }
    }
    #endregion

    private void Awake()
    {
        _Audio = FindObjectOfType<AudioController>();
        if(Popup.GetComponent<BoxCollider>())
            clickBlock = Popup.GetComponent<BoxCollider>();
    }

    private void Start()
    {
        Popup.alpha = 0;
    }

    public PopupController ShowPopup(string message, bool fadeOutHome = true)
    {
        return Setup(message).WithButton("OK").Show(fadeOutHome);
    }

    public PopupController WithButton(string label)
    {
        return WithButton(label, () => { });
    }

    public PopupController WithButton(string label, Action callback, bool hidePopupOnClick = true, float? gazeDelay = null)
    {
        if (DividerPrefab != null)
        {
            bool hasButtons = ButtonArea.transform.childCount > 0;

            if (hasButtons)
            {
                Instantiate(DividerPrefab, ButtonArea.transform);
            }
        }

        var go = Instantiate(ButtonPrefab, ButtonArea.transform);
        var button = go.GetComponent<PopupButton>();

        button.Setup(label, callback, gazeDelay);

        if (hidePopupOnClick)
        {
            button.OnClick += () => HidePopup();
        }

        if (_Audio != null)
        {
            button.OnClick += () => _Audio.PlayAudio(AudioController.AudioClips.GenericClick);
        }

        return this;
    }

    public PopupController Show(bool fadeOutHome = true)
    {
        if (fadeOutHome)
            FadeOutHome();

        Popup.DOFade(1.0f, PopupAnimationSpeed);
        EventManager.Instance.PopupShownEvent();

        if (clickBlock != null)
            clickBlock.enabled = true;

        return this;
    }

    public PopupController Setup(string message)
    {
        CleanupChildren();
        SetPopupText(message);
        return this;
    }

    /// <summary>
    /// Hides the Button Poup Object
    /// </summary>
    /// <param name="fadeInHome">Should Home fade in and the tiles be reactivated?</param>
    public void HidePopup(bool fadeInHome = true)
    {
        if (fadeInHome)
            FadeInHome();

        if (clickBlock != null)
            clickBlock.enabled = false;

        SetPopupText(String.Empty);

        CleanupChildren();

        Popup.alpha = 0;
    }

    private void CleanupChildren()
    {
        foreach (Transform child in ButtonArea.transform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Sets the message text to be displayed on the Image Popup Object
    /// </summary>
    /// <param name="message">Message to be displayed</param>
    public void SetPopupText(string message)
    {
        Message.text = message;
    }

    private void FadeOutHome()
    {
        //try
        //{
        //    FindObjectOfType<HomeController>().FadeCameraHalfOut();
        //}
        //catch
        //{
        //    //Debug.Log("HomeController not found, cannot call FadeCameraHalfOut");
        //}
    }

    private void FadeInHome()
    {
        //try
        //{
        //    FindObjectOfType<HomeController>().FadeCameraIn();
        //}
        //catch
        //{
        //    //Debug.Log("HomeController not found, cannot call FadeCameraIn");
        //}
    }
}