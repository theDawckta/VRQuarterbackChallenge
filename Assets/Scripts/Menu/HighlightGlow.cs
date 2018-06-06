using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class HighlightGlow : MonoBehaviour
{

    public int NumLoops = 5;
    public float AnimSpeed = 0.75f;
    public float AlphaTo = 0.3f;
    public Vector3 StartScale = new Vector3(0.8f, 0.8f, 0.8f);
    public Vector3 ScaleTo = new Vector3(1.0f, 1.0f, 1.0f);
    public float ScaleDelay = 0.3f;

    private int _counter = 0;
    private Image _image;
    private bool _isInitialPlaythrough = true;


    void Awake()
    {
        _image = this.gameObject.GetComponent<Image>();
        if (_image == null)
            throw new Exception("An _image is required for HighlightGlow");
        Hide();
		AnimPulse();
    }

    public void AnimScaleUp()
    {
        Show();
        if (_isInitialPlaythrough)
        {
            _isInitialPlaythrough = false;
        }
        DOTween.Kill(_image);
        this.gameObject.transform.localScale = StartScale;
        transform.DOScale(ScaleTo, AnimSpeed).SetDelay(ScaleDelay).SetEase(Ease.OutBounce).OnComplete(AnimScaleDown);
    }


    void AnimScaleDown()
    {
		DOTween.Kill(_image);
		transform.DOScale(StartScale, 0.15f).SetEase(Ease.InCubic).OnComplete(OnScaleDownComplete);
    }

    void OnScaleDownComplete()
    {
		DOTween.Kill(gameObject);
        _counter++;
        if (_counter < NumLoops)
        {
            AnimScaleUp();
        }
        else
        {
            _isInitialPlaythrough = true;
            _counter = 0;
            Hide();
        }
    }

    public void AnimPulse()
    {
    	_image.DOFade(AlphaTo, AnimSpeed).OnComplete(OnAlphaAnimUpComplete);
    }

    void Show()
    {
        Color imageColor = _image.color;
        imageColor.a = 1.0f;
        _image.color = imageColor;
    }

    void Hide()
    {
        Color imageColor = _image.color;
        imageColor.a = 0.0f;
        _image.color = imageColor;
    }

    void AlphaAnimDown()
    {
		_image.DOFade(0.0f, AnimSpeed).OnComplete(OnAlphaAnimDownComplete);
    }

    void OnAlphaAnimUpComplete()
    {
    	DOTween.Kill(gameObject);
        AlphaAnimDown();
    }

    void OnAlphaAnimDownComplete()
    {
    	DOTween.Kill(gameObject);
        _counter++;
        if (_counter < NumLoops)
        {
            AnimPulse();
        }
        else
        {
            _counter = 0;
            Hide();
        }
    }
}
