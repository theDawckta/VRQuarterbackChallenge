using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildAssets
{

    static string assetBundleFolderPath = Application.dataPath + "/AssetBundles/";            

    /// <summary>
    /// Builds an Asset Bundle for the current build target
    /// </summary>
    [MenuItem("AssetBundles/Build/Current")]
    static void BuildAssetBundle()
    {
        string suffix="";

        switch (EditorUserBuildSettings.activeBuildTarget)
        {
            case BuildTarget.Android:
                suffix = "Android";
                break;

            case BuildTarget.iOS:
                suffix = "iOS";
                break;

            case BuildTarget.StandaloneWindows:
                suffix = "Windows";
                break;

            case BuildTarget.StandaloneOSX:
                suffix = "OSX";
                break;

            default:
                break;
        }

        BuildBundle(assetBundleFolderPath + suffix,BuildAssetBundleOptions.None,EditorUserBuildSettings.activeBuildTarget);
    }

    /// <summary>
    /// Builds an Asset Bundle for the Android build target
    /// </summary>
    [MenuItem("AssetBundles/Build/Android")]
    static void BuildAndroid()
    {
        BuildBundle(assetBundleFolderPath + "Android",BuildAssetBundleOptions.None, BuildTarget.Android);
    }

    /// <summary>
    /// Builds an Asset Bundle for the iOS build target
    /// </summary>
    [MenuItem("AssetBundles/Build/iOS")]
    static void BuildiOS()
    {
        BuildBundle(assetBundleFolderPath + "iOS", BuildAssetBundleOptions.None, BuildTarget.iOS);
    }

    /// <summary>
    /// Builds an Asset Bundle for the Windows build target
    /// </summary>
    [MenuItem("AssetBundles/Build/Windows")]
    static void BuildWindows()
    {
        BuildBundle(assetBundleFolderPath + "Windows", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }

    /// <summary>
    /// Builds an Asset Bundle for the OSX build target
    /// </summary>
    [MenuItem("AssetBundles/Build/OSX")]
    static void BuildOSX()
    {
        BuildBundle(assetBundleFolderPath + "OSX", BuildAssetBundleOptions.None, BuildTarget.StandaloneOSX);
    }


    /// <summary>
    /// Builds an AssetBundle for Android,iOS and PC
    /// </summary>
    [MenuItem("AssetBundles/Build/Build All")]
    static void BuildAllAssetBundles()
    {
        BuildAndroid();
        BuildiOS();
        BuildWindows();
        BuildOSX();
    }

    /// <summary>
    /// Common function called by different Menu options to build asset bundles
    /// </summary>
    /// <param name="path">Folder Path to build AssetBundle</param>
    /// <param name="options">BuildAssetBundle options</param>
    /// <param name="target">Build target (platform) to build for</param>
    private static void BuildBundle(string path, BuildAssetBundleOptions options, BuildTarget target)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        BuildPipeline.BuildAssetBundles(path, options, target);
    }

    /// <summary>
    /// Cleans the internal Asset Bundle Cache
    /// </summary>
    [MenuItem ("AssetBundles/Clear AssetBundle Cache")]
    static void ClearAssetBundleCache()
    {
        Caching.ClearCache();
        Debug.Log("Cleared AssetBundle Cache");
    }
}
