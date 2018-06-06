using System;
using System.Collections.Generic;
using UnityEngine;

public class AndroidActivityHelper {

    /// <summary>
    /// Returns the current activity
    /// </summary>
    /// <returns>Android Java Object for current activity</returns>
    public static AndroidJavaObject GetCurrentActivity()
    {
        AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        return currentActivity;
    }

    /// <summary>
    /// Returns the current activity's intent
    /// </summary>
    /// <returns>Android Java Object for Intent</returns>
    public static AndroidJavaObject GetIntent()
    {
        return GetCurrentActivity().Call<AndroidJavaObject>("getIntent");        
    }

    public static Dictionary<string, string> GetArguments()
    {
        Dictionary<string, string> argumentDicitonary = new Dictionary<string, string>();
        string arguments = GetIntent().Call<string>("getDataString");

        if (!string.IsNullOrEmpty(arguments))
        {
            int index = arguments.IndexOf('?');
            if (index != -1)
            {
                string[] parameters = arguments.Substring(index+1).Split('&');
                for (int i = 0; i < parameters.Length; i++)
                {
                    int indexOfEquals = parameters[i].IndexOf('=');
                    argumentDicitonary.Add(parameters[i].Substring(0,indexOfEquals),parameters[i].Substring(indexOfEquals+1));

                    //Debug.Log("ARG DICT :  " + parameters[i].Substring(0, indexOfEquals) + ", " + parameters[i].Substring(indexOfEquals + 1));
                }                
            }
        }

        return argumentDicitonary;
    }



}
