using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class Builder
{

	private const string KEYSTORE_FILE = "user.keystore";
	private const string KEYSTORE_PASS = "testtest";
	private const string KEYALIAS_NAME = "intel poc";
	private const string KEYALIAS_PASS = "testtest";


    [MenuItem("File/Build/Android")]
    public static void BuildAndroid()
    {
        Build(BuildTarget.Android, ".apk");
    }

    [MenuItem("File/Build/All")]
    public static void BuildAll()
    {
        BuildAndroid();
    }

    private static void Build(BuildTarget target, string extension, params string[] scenesToBuild)
    {
        var args = Environment.GetCommandLineArgs();

        string filename = "voke";
        string configurationName = null;
        int version = 0;

        if (args != null)
        {
            string configArg = args.SkipWhile(_ => _ != "--config").Skip(1).FirstOrDefault();
            if (!String.IsNullOrEmpty(configArg))
            {
                configurationName = configArg;
            }

            string versionArg = args.SkipWhile(_ => _ != "--version").Skip(1).FirstOrDefault();
            int v;
            if (!String.IsNullOrEmpty(versionArg) && Int32.TryParse(versionArg, out v) && v > 0)
            {
                version = v;
            }
        }

        ProjectConfiguration config = null;

        if (!String.IsNullOrEmpty(configurationName))
        {
            config = AssetDatabase.LoadAssetAtPath<ProjectConfiguration>("Assets/Configurations/" + configurationName + "/" + configurationName + ".asset");
            config.Apply();

            if (!String.IsNullOrEmpty(config.ConfigFolderName))
                filename = config.ConfigFolderName;
        }

        if (version > 0)
        {
            // version passed in by command line always higher priority
            PlayerSettings.Android.bundleVersionCode = version;
        }

        string targetName = Enum.GetName(typeof(BuildTarget), target);
        string folderPath = String.Format("Builds/{0}/", targetName);
        string outputFilename = filename + extension;

        string filePath = Path.Combine(folderPath, outputFilename);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
		
		List<string> newScenesToBuild = new List<string>();
		for (int j = 0; j < EditorBuildSettings.scenes.Length; j++)
		{
			string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex (j);
			EditorBuildSettingsScene curScene = EditorBuildSettings.scenes [j];
			if (curScene.enabled)
			{
				newScenesToBuild.Add(scenePath);
				Debug.Log ("scenePath: " + scenePath);
			}
		}

		PlayerSettings.Android.keystoreName = "./keystore/" + KEYSTORE_FILE;
		PlayerSettings.Android.keystorePass = KEYSTORE_PASS;
		PlayerSettings.Android.keyaliasName = KEYALIAS_NAME;
		PlayerSettings.Android.keyaliasPass = KEYALIAS_PASS;

		string err = BuildPipeline.BuildPlayer(newScenesToBuild.ToArray(), filePath, target, BuildOptions.None);

        /*
        string bundlePath = String.Format("Bundles/{0}/", targetName);

        if (!Directory.Exists(bundlePath))
            Directory.CreateDirectory(bundlePath);

        BuildPipeline.BuildAssetBundles(bundlePath, BuildAssetBundleOptions.None, target);
        */

        if (config != null)
        {
            config.Revert();
        }

        if (!String.IsNullOrEmpty(err))
            throw new Exception(err);
    }
}