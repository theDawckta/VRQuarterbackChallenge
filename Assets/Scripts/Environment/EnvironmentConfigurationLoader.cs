using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

public class EnvironmentConfigurationLoader : XmlDataFileLoader<EnvironmentConfigurationLoader, EnvironmentConfiguration>
{
    private void Awake()
    {
		FileUrl = RuntimeConfiguration.Default.EnvironmentConfigUrl;
//		FileUrl = "http://config.vokevr.com/vokegearvrapp/Config/Production/EnvironmentConfig-2.3.xml";

#if UNITY_EDITOR
        var localPath = Application.dataPath + "/Data/EnvironmentConfig.xml";
        if (File.Exists(localPath))
        {
            FileUrl = "file://" + localPath;
        }
#elif UNITY_ANDROID
        string[] localPaths = { "/mnt/sdcard/VOKE/EnvironmentConfig.xml", "/sdcard/VOKE/EnvironmentConfig.xml" };
        foreach(var localPath in localPaths)
        {
		    if(File.Exists(localPath))
		    {
			    FileUrl = "file://" + localPath;
		    }
        }
#endif
    }
}