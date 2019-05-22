using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GazeTimer : MonoBehaviour
{
    public Image GazeTimerImg;

    void Awake()
    {
        if (GazeTimerImg != null)
        {
            GazeTimerImg.fillAmount = 0.0f;
        }
    }

    public void SetGazeValue(float gazeValue)
    {
        if (GazeTimerImg != null)
        {
            GazeTimerImg.fillAmount = gazeValue;
        }
    }
}
