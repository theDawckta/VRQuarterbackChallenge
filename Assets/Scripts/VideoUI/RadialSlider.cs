using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RadialSlider : MonoBehaviour {

	public Image FilledSlider;

	private VideoController _VideoController;

	void Awake () {
		_VideoController = Extensions.GetRequired<VideoController>();
		FilledSlider.fillAmount = 0.0f;
	}
	
	void Update()
	{
		float sliderPos = _VideoController.GetSliderPosition();
		FilledSlider.fillAmount = sliderPos;
	}
}
