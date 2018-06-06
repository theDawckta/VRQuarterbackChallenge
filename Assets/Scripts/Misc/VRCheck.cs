using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRCheck : MonoBehaviour
{

    void Awake()
    {
        string vrModel = XRDevice.model.ToLower();
        if (vrModel.IndexOf("daydream") != -1)
        {
            GlobalVars.Instance.IsDaydream = true;
        }
    }

    void Update()
    {
        if (GlobalVars.Instance.IsDaydream) //DAYDREAM EXIT BUTTON
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }
}
