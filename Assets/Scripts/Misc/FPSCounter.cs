using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    //public Text fpsText;
	public TextMeshProUGUI fpsText;

    private float deltaTime = 0.0f;

    void Update()
    {
		if (fpsText != null)
		{
			deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

			float fps = 1.0f / deltaTime;
			//turn off smoothing
			//float fps = 1.0f/Time.deltaTime;
			fpsText.text = String.Format ("{0:0.} fps", fps);
		}
    }
}