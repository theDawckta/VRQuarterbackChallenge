using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class StatLine : MonoBehaviour 
{
	public Text LeftText;
	public Text CenterText;
	public Text RightText;
	public Slider LeftSlider;
	public Slider RightSlider;
	
	public void StatLineInit(string leftText, string centerText, string rightText, float leftSliderValue, float rightSliderValue)
	{
		LeftText.text = SetTextSize (leftText, 5);
		CenterText.text = centerText;
		RightText.text = SetTextSize (rightText, 5);
		LeftSlider.value = leftSliderValue;
		RightSlider.value = rightSliderValue;
	}

	public void UpdateStatBar(string leftText, string centerText, string rightText, float leftSliderValue, float rightSliderValue)
	{
		//we need to round left/right text to smaller values
		leftText = SetTextSize(leftText,5);
		rightText = SetTextSize(rightText,5);
		if(LeftText.text != leftText)
			StartCoroutine(UpdateText(LeftText, leftText));
		if(CenterText.text != centerText)
			StartCoroutine(UpdateText(CenterText, centerText));
		if(RightText.text != rightText)
			StartCoroutine(UpdateText(RightText, rightText));
		if(LeftSlider.value != leftSliderValue)
		{
			LeftSlider.DOValue(leftSliderValue, 0.3f);
		}
		if(RightSlider.value != rightSliderValue)
		{
			RightSlider.DOValue(rightSliderValue, 0.3f);
		}
	}

	IEnumerator UpdateText(Text textControl, string newText)
    {
        textControl.CrossFadeAlpha(0.0f, 0.3f, false);
        yield return new WaitForSeconds(0.3f);
        textControl.text = newText;
        textControl.CrossFadeAlpha(1.0f, 0.3f, false);
    }

	string SetTextSize(string textStr, int size)
	{
		if (textStr.Length > size)
		{
			textStr = textStr.Substring (0, size);
		}
		return textStr;
	}
}
