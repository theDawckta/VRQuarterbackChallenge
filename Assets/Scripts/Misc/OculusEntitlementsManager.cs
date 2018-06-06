using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OculusEntitlementsManager : MonoBehaviour {

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Use this for initialization
    void Start () {
        string oculusID = RuntimeConfiguration.Default.OculusAppID;
        if (!string.IsNullOrEmpty(oculusID))
        {
            Oculus.Platform.Core.AsyncInitialize(oculusID);
            Oculus.Platform.Entitlements.IsUserEntitledToApplication().OnComplete(callbackMethod);
        }
    }

    void callbackMethod(Oculus.Platform.Message msg)
    {
        if (!msg.IsError)
        {
            // Entitlement check passed
            Debug.Log("******************* OculusEntitlementsManager  Passed ************************");
            Destroy(this.gameObject);
        }
        else
        {
            Debug.Log("******************* OculusEntitlementsManager  Failed ************************");
            Application.Quit();
        }
    }
}
