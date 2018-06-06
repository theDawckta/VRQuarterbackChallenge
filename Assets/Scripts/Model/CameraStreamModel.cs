using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VOKE.VokeApp.DataModel;
using UnityEngine;

public class CameraStreamModel
{
    private readonly CameraStream _Stream;

    public CameraStreamModel(CameraStream stream)
    {
        _Stream = stream;
        Url = GetUrlWithAuthTokens(stream.Url);

        Links = new Dictionary<Vector2, CameraStreamModel>();
    }

	void SetHEVCUrl()
	{
		//if we do not have a URL directly defined
		if (string.IsNullOrEmpty (_Stream.Url))
		{
			if (GlobalVars.Instance.UseHEVCStream && !string.IsNullOrEmpty(_Stream.HEVC))
			{
				_Stream.Url = _Stream.HEVC;
			} else
			{
				_Stream.Url = _Stream.H264;
			}
		}
//		string newUrl = _Stream.Url;
//		if (GlobalVars.Instance.UseHEVCStream && newUrl.IndexOf("-hevc") == -1)
//		{
//			int index = _Stream.Url.LastIndexOf (".");
//			newUrl = _Stream.Url.Substring (0, index) + "-hevc" + _Stream.Url.Substring (index);
//			_Stream.Url = newUrl;
//		}
	}

    private string GetUrlWithAuthTokens(string url)
    {
        if (String.IsNullOrEmpty(url))
            return url;

        if (url.Contains("hsvr.akamaized.net") || url.Contains("hsvrvod.akamaized.net")) // Akamai Authentication
        {
#if (UNITY_EDITOR) || (UNITY_STANDLONE)
#warning Authenticated urls only implemented on Android
#elif UNITY_ANDROID
            // Get a java class
            AndroidJavaClass akamaiHelperClass = new AndroidJavaClass("in/startv/hotstar/utils/akamai/AkamaiHelper");

            // Call a static
            url = akamaiHelperClass.CallStatic<string>("authorizeStreamUrl", url);
            Debug.Log("getUrlWithAuthTokens()" + url);
#endif
        }

        return url;
    }

    public string Url { 
		//TODO: decide if we are going to append the URL here
		get { SetHEVCUrl (); return _Stream.Url; }
		set { _Stream.Url = value; }
	}

    public bool IsTokenized;

    public bool IsPreferredCamera
    {
        get { return _Stream.IsPreferred; }
    }

    public bool IsVRCastCamera
    {
        get { return _Stream.IsVRcast; }
    }

    public string ID
    {
        get { return _Stream.ID; }
    }

    public string Name
    {
        get { return _Stream.Name; }
    }

    public string Label
    {
        get { return _Stream.Label; }
    }

    public float? OffsetX
    {
        get { return _Stream.OffsetX; }
    }

    public float? OffsetY
    {
        get { return _Stream.OffsetY; }
    }

    public float? OffsetZ
    {
        get { return _Stream.OffsetZ; }
    }

    public float? Rotation
    {
        get { return _Stream.Rotation; }
    }

    public Dictionary<Vector2, CameraStreamModel> Links { get; private set; }
}
