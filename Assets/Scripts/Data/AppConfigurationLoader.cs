using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using VOKE.VokeApp.DataModel;

public class AppConfigurationLoader : XmlDataFileLoader<AppConfigurationLoader, VokeAppConfiguration>
{
    public bool IsLocalFile { get; private set; }

    private void Awake()
    {
//        CheckInterval = TimeSpan.FromSeconds(60);
////		FileUrl = "http://config.vokevr.com/vokegearvrapp/Config/Production/AppConfig-2.3.xml";
//		FileUrl = RuntimeConfiguration.Default.AppConfigUrl;

//		Debug.Log ("FileUrl: " + FileUrl);

//#if UNITY_EDITOR
//        var localPath = Application.dataPath + "/Data/AppConfig.xml";

//        if (File.Exists(localPath))
//        {
//            FileUrl = "file://" + localPath;
//            CheckInterval = TimeSpan.FromSeconds(30);
//            IsLocalFile = true;
//        }
//#elif UNITY_STANDALONE
//        if (File.Exists("AppConfig-Rift.xml"))
//        {
//           FileUrl = "file://" + "AppConfig-Rift.xml";
//		   IsLocalFile = true;
//        }
//        else
//        {
//            FileUrl = "http://intelvoke-staging.popworldwide.com/AppConfig-Rift.xml";
//        }
//#elif UNITY_ANDROID

//		string[] localPaths = { "/mnt/sdcard/VOKE/AppConfig-Staging.xml", "/sdcard/VOKE/AppConfig-Staging.xml" };
//        foreach(var localPath in localPaths)
//        {
//		    if(File.Exists(localPath))
//		    {
//				Debug.Log("fileExists: " + localPath);
//			    FileUrl = "file://" + localPath;
//                IsLocalFile = true;
//				Debug.Log("FileUrl: " + FileUrl);
//				//GlobalVars.Instance.IsLocalBuild = true;
//		    }
//        }
//#endif
    }
}