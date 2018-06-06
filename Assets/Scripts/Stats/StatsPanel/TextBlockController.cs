using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class TextBlockController : MonoBehaviour 
{
    private TextMeshProUGUI _textMeshPro;
    private Image _backgroundImage;

    void Awake()
    {
        _textMeshPro = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        _backgroundImage = gameObject.GetComponentInChildren<Image>();
    }

    public void SetText(string text)
    {
        _textMeshPro.text = text;
    }

    public void UpdateText(string text)
    {
        _textMeshPro.DOFade(0.0f, 0.3f).OnComplete(() => {
            _textMeshPro.text = text;
            _textMeshPro.DOFade(1.0f, 0.3f);
        });
    }

    public string GetText()
    {
        return _textMeshPro.text;
    }

    public void SetAlignment(TextAlignmentOptions alignment)
    {
        _textMeshPro.alignment = alignment;
    }

    public void SetMargin(float left, float right)
    {
        _textMeshPro.margin = new Vector4(left, 0.0f, right, 0.0f);
    }

    public float GetWidth()
    {
        return _textMeshPro.preferredWidth;
    }

    public void SetBackgroundColor(Color backgroundColor)
    {
        _backgroundImage.color = backgroundColor;
        _backgroundImage.enabled = true;
    }
}
