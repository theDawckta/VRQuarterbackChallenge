using System;
using System.Collections;
using UnityEngine;

public class NetworkUtility : MonoBehaviour
{
    public static IEnumerator CheckInternetConnection(Action<bool> action)
    {
        // Try to connect to any website
        WWW www = new WWW("http://intel.com");
        yield return www;

        //if (www.error != null)
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log("NetworkUtility CheckInternetConnection error: " + www.error);
            action(false);
        }
        else
        {
            action(true);
        }
    }
}