using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using VOKE.VokeApp.Util;

public abstract class XmlDataFileLoader<TType, T> : Singleton<TType> where TType : MonoBehaviour
{
    private T _Data;
    private bool _DataLoaded;

    private readonly XmlSerializer _Serializer = new XmlSerializer(typeof(T));

    public string FileUrl { get; set; }

    public TimeSpan? CheckInterval { get; set; }

    public Coroutine GetDataAsync(Action<T> callback, bool force = false)
    {
        if (_DataLoaded && !force)
        {
            callback(_Data);
            return null;
        }
        else
        {
            return RunAsync(callback);
        }
    }

    private Coroutine RunAsync(Action<T> callback)
    {
        return StartCoroutine(Run(callback));
    }

    private IEnumerator Run(Action<T> callback)
    {
    //    Trace.Log("Download a cofiguration file from {0}", FileUrl);
    //    using (var www = new WWW(FileUrl))
    //    {
    //        yield return www;

    //        if (!String.IsNullOrEmpty(www.error))
    //        {
    //            Trace.Error("Failed to download the XML file. ErrorMessage={0}", www.error);
				//Debug.Log ("Failed to download the XML file.");
				//Debug.Log (www.error);
				//callback (_Data);
     //           yield break;
     //       }

     //       bool isModified = CheckIfModified(www);

     //       if (!_DataLoaded || isModified)
     //       {
     //           try
     //           {
     //               //XML serialize/Deserialize
     //               var reader = new StringReader(www.text);
     //               var data = (T)_Serializer.Deserialize(reader);
					////Debug.Log("text: ");
					////Debug.Log(www.text);
					////Debug.Log("data: " + data);
					////Debug.Log(data);

        //            _Data = data;
        //            _DataLoaded = true;
        //            var ev = Updated;
        //            if (ev != null)
        //            {
        //                var args = new ContentUpdatedEventArgs<T>(_Data);
        //                Updated(this, args);
        //            }
        //        }
        //        catch (XmlException ex)
        //        {
        //            Debug.LogError("Fail to load the xml file. ErrorMessage=" + ex.Message);
        //        }
        //    }

        //    _LastCheck = Time.time;

           callback(_Data);
		yield return null;
        //}
    }

    private float? _LastCheck;

    private void Update()
    {
        if (CheckInterval != null && _LastCheck != null)
        {
            float timeNow = Time.time;

            bool shouldCheck = timeNow - _LastCheck.Value >= CheckInterval.Value.TotalSeconds;

            if (shouldCheck)
            {
                Debug.LogFormat(gameObject, "Time to check server at {0:00} seconds, last check at {1:00} seconds", timeNow, _LastCheck.Value);

                _LastCheck = null; // so that more than one update do not fire while checking

                RunAsync(data =>
                {
                    Debug.Log("Check finished");
                });
            }
        }
    }

    public event EventHandler<ContentUpdatedEventArgs<T>> Updated;

    private readonly IDictionary<string, string> _ModifiedDates = new Dictionary<string, string>();
    private bool CheckIfModified(WWW www)
    {
        string headerValue;

        const string filePrefix = "file://";
        if (FileUrl.StartsWith(filePrefix))
        {
            string path = FileUrl.Substring(filePrefix.Length);
            var info = new FileInfo(path);

            if (!info.Exists)
                return true; // if we can't get file write info, assume modified

            headerValue = info.LastWriteTime.ToString("r");
        }
        else if (www.responseHeaders == null || !www.responseHeaders.TryGetValue("Last-Modified", out headerValue))
        {
            return true; // no server header, assume modified
        }

        string clientValue;
        bool seenBefore = _ModifiedDates.TryGetValue(FileUrl, out clientValue);

        //Debug.LogFormat("File mod date {0}: {1}, existing {2}", FileUrl, headerValue, clientValue);

        _ModifiedDates[FileUrl] = headerValue; // record header value for later

        if (!seenBefore)
            return true; // never seen the file before, have to assume new

        return headerValue != clientValue; // modified if the date has changed
    }

    /// <summary>
    /// Set Data Loaded variable - USE CAREFULLY
    /// </summary>
    /// <param name="val">Value to set</param>
    public void SetDataLoaded(bool val)
    {
        _DataLoaded = val;
    }
}
