using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PageIndicatorController : MonoBehaviour
{
    public Sprite PageOff;
    public Sprite PageOn;

    private Image _pageImage;

    void Awake()
    {
        _pageImage = gameObject.GetComponent<Image>();
        TurnPageOff();
    }

    public void TurnPageOn()
    {
        _pageImage.sprite = PageOn;
    }

    public void TurnPageOff()
    {
        _pageImage.sprite = PageOff;
    }
}