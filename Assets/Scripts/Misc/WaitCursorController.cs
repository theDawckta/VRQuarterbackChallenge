using System;
using System.Collections;
using UnityEngine;

public class WaitCursorController : MonoBehaviour
{
    public GameObject WaitCursor;

    void Awake()
    {
        WaitCursor.SetActive(false);
    }

    void OnFadeIn()
    {
        WaitCursor.SetActive(true);
    }

    void OnFadeOut()
    {
        WaitCursor.SetActive(false);
    }

    void OnEnable()
    {
        EventManager.OnBlackoutFadeInEvent += OnFadeIn;
        EventManager.OnBlackoutFadeComplete += OnFadeOut;
    }

    void OnDisable()
    {
        EventManager.OnBlackoutFadeInEvent -= OnFadeIn;
        EventManager.OnBlackoutFadeComplete -= OnFadeOut;
    }
}