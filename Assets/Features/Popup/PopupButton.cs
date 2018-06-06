using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupButton : Interactible
{
    private TextMeshProUGUI _Text;
    private Image _Image;
    private Color _InitialColor;

    private void Awake()
    {
        base.DoAwake();

        _Text = GetComponentInChildren<TextMeshProUGUI>();
        _Image = GetComponent<Image>();
    }

    void OnRectTransformDimensionsChange()
    {
        var trans = GetComponent<RectTransform>();
        ResizeCollider(trans.sizeDelta);
    }

    private void OnEnable()
    {
        OnOver += PopupButton_OnOver;
        OnOut += PopupButton_OnOut;
    }

    private void OnDisable()
    {
        OnOver -= PopupButton_OnOver;
        OnOut -= PopupButton_OnOut;
    }

    private void PopupButton_OnOver()
    {
        _Image.DOFillAmount(1.0f, GazeTime);
    }

    private void PopupButton_OnOut()
    {
        _Image.DOKill(false);
        _Image.fillAmount = 0.0f;
    }

    public void Setup(string text, Action clickAction, float? gazeTime)
    {
        _Text.text = text;
        OnClick += clickAction;

        if (gazeTime != null)
            GazeTime = gazeTime.Value;
    }
}
