using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngineObject = UnityEngine.Object;

[CreateAssetMenu(fileName = "ProjectConfiguration", menuName = "Custom/Project Configuration")]
public class ProjectConfiguration : ScriptableObject
{
    public enum DeploymentPlatform
    {
        GearVR,
        DayDream,
        WinMR,
        OculusRift,
        HTCVive,
        PSVR
    }

    public enum HomeScene
    {
        TrueVR,
        Olympics
    }

    [Tooltip("Desired deployment platform")]
    public DeploymentPlatform DeploymentTarget;

    [Tooltip("HomeScene to use in the app.")]
    public HomeScene HomeSceneStyle;

    public string ProjectName;
    public string CompanyName;
    public string BundleIdentifier;
    [Tooltip("Name of the folder inside the configurations folder")]
    public string ConfigFolderName;
    public string DisplayVersion;
    public int AndroidVersionCode;
    public Texture2D[] Icons;
    public string AssetFolder;
    public string ProtocolHandler;
    [Tooltip("Hides the Feedback Button from the Global Menu")]
    public bool HideFeedbackButton;
    [Tooltip("Removes the Geo blocking permission and the feature")]
    public bool RemoveGeoBlocking;
    [Tooltip("Enables curved UI for Text Mesh Pro")]
    public bool EnableCurvedUI;

    public string CloudProjectId;
    public string CloudProjectName;
    public string CloudOrganizationId;
	public bool isBuildRelease = false;

    private const string _TempDirPath = "TempSavedAssets/";
    private const string _TempManifestDirPath = "TempSavedManifests/";
    private const string _TempProjectSettingsPath = "Assets/TempProjectSettings.asset";
    private const string _LocalConfigPath = "Assets/Resources/";
    private const string _ConfigFileName = "ConfigLive.asset";
    private const string _TrueVRHomeScenePath = "Assets/Scenes/HomeScene.unity";
    private const string _OlympicsHomeScenePath = "Assets/Scenes/Olympics/HomeScene.unity";
    private const string _AndroidManifestPath = "Assets/Plugins/Android/";
    private const string _AndroidManifestFile = "AndroidManifest.xml";
    private const string _AndroidManifestFile_GearVR = "AndroidManifest-GearVR.xml";
    private const string _AndroidManifestFile_GearVR_AppStore = "AndroidManifest-GearVR_AppStore.xml";
	private const string _AndroidManifestFile_GearVR_Build = "AndroidManifest-GearVR_Build.xml";
    private const string _AndroidManifestFile_Daydream = "AndroidManifest-Daydream.xml";

    [HideInInspector] public bool isStoreRelease = false;


    private List<EditorBuildSettingsScene> currentScenes;
    private string defines = "";

    public void Apply()
    {
        ApplyPlatformSettings(DeploymentTarget);
        ApplyProjectSettings();
        ApplyAssets();
    }

    public void Revert()
    {
        RevertPlatformSettings();
        RevertProjectSettings();
        RevertAssets();
    }

    /// <summary>
    /// Switches the platform if needed depending on the Deployment target
    /// </summary>
    public void ApplyPlatformSettings(DeploymentPlatform deploymentTarget)
    {
        switch (DeploymentTarget)
        {
            case DeploymentPlatform.GearVR:
            case DeploymentPlatform.DayDream:
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                ApplyAndroidSettings();
                break;

            case DeploymentPlatform.WinMR:
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);
                ApplyWinMRSettings();
                break;

            case DeploymentPlatform.OculusRift:
            case DeploymentPlatform.HTCVive:
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
                break;

            case DeploymentPlatform.PSVR:
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.PS4, BuildTarget.PS4);
                break;

            default:
                break;
        }

        SwitchScenes();
    }

    /// <summary>
    /// Switches the platform back to the previous config depending on its Deployment target
    /// </summary>
    public void RevertPlatformSettings()
    {
        DeploymentPlatform tempTarget;

        if (!File.Exists(_TempProjectSettingsPath))
            return;

        var config = AssetDatabase.LoadAssetAtPath<ProjectConfiguration>(_TempProjectSettingsPath);

        tempTarget = config.DeploymentTarget;

        ApplyPlatformSettings(tempTarget);
    }

    /// <summary>
    /// Switches the Home Scene in the build settings
    /// </summary>
    public void SwitchScenes()
    {
        currentScenes = EditorBuildSettings.scenes.ToList();

        if (HomeSceneStyle == HomeScene.TrueVR)
        {
            //Debug.Log("Target scene is TrueVR");
            CheckForSceneAndSwap(_TrueVRHomeScenePath, _OlympicsHomeScenePath);
        }

        if (HomeSceneStyle == HomeScene.Olympics)
        {
            //Debug.Log("Target scene is Olympics");
            CheckForSceneAndSwap(_OlympicsHomeScenePath, _TrueVRHomeScenePath);
        }
    }

    /// <summary>
    /// Checks if a Scene exists in the build settings, otherwises makes a call to swap it
    /// </summary>
    /// <param name="sceneToCheckPath">Filepath of scene to be checked</param>
    /// <param name="sceneToRemovePath">Filepath of scene to be removed</param>
    private void CheckForSceneAndSwap(string sceneToCheckPath, string sceneToRemovePath)
    {
        for (int i = 0; i < currentScenes.Count(); i++)
        {
            if (currentScenes[i].path == sceneToCheckPath)  //Desired Home scene found break
            {
                Debug.Log(sceneToCheckPath + " already exists in current scenes, exiting");
                return;
            }
        }
        //Desired Home scene not found, need to brign it in
        SwapScene(sceneToRemovePath, sceneToCheckPath);
    }

    /// <summary>
    /// Removes a scene from the build settings and adds another one as its replacement
    /// </summary>
    /// <param name="sceneToRemovePath">Filepath of scene to be removed</param>
    /// <param name="sceneToAddPath">Filepath of scene to be added</param>
    private void SwapScene(string sceneToRemovePath, string sceneToAddPath)
    {
        Debug.Log(sceneToAddPath + " being added and " + sceneToRemovePath + " being removed");
        currentScenes.Remove(currentScenes.Single(s => s.path == sceneToRemovePath));
        currentScenes.Add(new EditorBuildSettingsScene(sceneToAddPath, true));
        EditorBuildSettings.scenes = currentScenes.ToArray();
    }

    public void ApplyProjectSettings()
    {
        RevertProjectSettings();

        var oldConfig = CreateInstance<ProjectConfiguration>();

        SetProjectSettings(oldConfig);

        AssetDatabase.CreateAsset(oldConfig, _TempProjectSettingsPath);
        AssetDatabase.SaveAssets();
    }

    private const string _TempAndroidSettingsPath = "Assets/TempAndroidSettings.xml";
    private const string _AndroidSettingsPath = "Assets/Plugins/Android/Custom/res/values/custom.xml";

    public void ApplyAndroidSettings()
    {
        RevertAndroidSettings();

        var dict = new Dictionary<string, string>
        {
            { "protocol_handler", ProtocolHandler }
        };

        FileUtil.CopyFileOrDirectory(_AndroidSettingsPath, _TempAndroidSettingsPath);

        var doc = XDocument.Load(_AndroidSettingsPath);

        foreach (var node in doc.Root.Elements("string"))
        {
            string value;
            if (!dict.TryGetValue(node.Attribute("name").Value, out value) || String.IsNullOrEmpty(value))
                continue;

            node.SetValue(value);
        }

        if (DeploymentTarget == DeploymentPlatform.DayDream)
        {
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            UnityEditorInternal.VR.VREditor.SetVREnabledDevicesOnTargetGroup(BuildTargetGroup.Android, new string[] { "daydream" });
            FileUtil.ReplaceFile(_AndroidManifestPath + _AndroidManifestFile_Daydream, _AndroidManifestPath + _AndroidManifestFile);
        }

        if (DeploymentTarget == DeploymentPlatform.GearVR)
        {
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
            UnityEditorInternal.VR.VREditor.SetVREnabledDevicesOnTargetGroup(BuildTargetGroup.Android, new string[] { "Oculus" });
			if (isStoreRelease)
			{
				FileUtil.ReplaceFile (_AndroidManifestPath + _AndroidManifestFile_GearVR_AppStore, _AndroidManifestPath + _AndroidManifestFile);
			} else if (isBuildRelease)
			{
				FileUtil.ReplaceFile (_AndroidManifestPath + _AndroidManifestFile_GearVR_Build, _AndroidManifestPath + _AndroidManifestFile);
			} else
			{
				FileUtil.ReplaceFile (_AndroidManifestPath + _AndroidManifestFile_GearVR, _AndroidManifestPath + _AndroidManifestFile);
			}
        }

        doc.Save(_AndroidSettingsPath);

        RefreshFirebaseXML();
    }

    public void RevertAndroidSettings()
    {
        if (!File.Exists(_TempAndroidSettingsPath))
            return;

        FileUtil.ReplaceFile(_TempAndroidSettingsPath, _AndroidSettingsPath);
        FileUtil.DeleteFileOrDirectory(_TempAndroidSettingsPath);
    }

    /// <summary>
    /// Applies settings/changes needed to build for Windows MR
    /// </summary>
    private void ApplyWinMRSettings()
    {
        PlayerSettings.virtualRealitySupported = true;

    }

    private void SetProjectSettings(ProjectConfiguration oldConfig)
    {
        if (!String.IsNullOrEmpty(DisplayVersion))
        {
            if (oldConfig != null)
                oldConfig.DisplayVersion = PlayerSettings.bundleVersion;

            PlayerSettings.bundleVersion = DisplayVersion;
        }

        if (!String.IsNullOrEmpty(BundleIdentifier))
        {
            #region Workaround for Firebase Unity plugin

            const string identifierToReplace = "com.vokevr.vokeviewer";

            if (!String.Equals(identifierToReplace, BundleIdentifier) && oldConfig != null) // don't do it on revert
            {
                using (var scope = new AssetScope(_TempManifestDirPath))
                {
                    var manifests = AssetDatabase.FindAssets("AndroidManifest");

                    foreach (var guid in manifests)
                    {
                        string manifestPath = AssetDatabase.GUIDToAssetPath(guid);

                        string text = File.ReadAllText(manifestPath);

                        if (text.Contains(identifierToReplace))
                        {
                            scope.Backup(manifestPath);
                            text = text.Replace(identifierToReplace, BundleIdentifier);
                            File.WriteAllText(manifestPath, text);
                        }
                    }
                }
            }
            #endregion

            if (oldConfig != null)
                oldConfig.BundleIdentifier = PlayerSettings.applicationIdentifier;

            PlayerSettings.applicationIdentifier = BundleIdentifier;
        }

        if (!String.IsNullOrEmpty(CompanyName))
        {
            if (oldConfig != null)
                oldConfig.CompanyName = PlayerSettings.companyName;
            PlayerSettings.companyName = CompanyName;
        }

        if (!String.IsNullOrEmpty(ProjectName))
        {
            if (oldConfig != null)
                oldConfig.ProjectName = PlayerSettings.productName;
            PlayerSettings.productName = ProjectName;
        }

        if (AndroidVersionCode > 0)
        {
            if (oldConfig != null)
                oldConfig.AndroidVersionCode = PlayerSettings.Android.bundleVersionCode;
            PlayerSettings.Android.bundleVersionCode = AndroidVersionCode;
        }

        if (Icons != null)
        {
            if (oldConfig != null)
                oldConfig.Icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown);
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, Icons);
        }

        if (DeploymentTarget == DeploymentPlatform.GearVR)
            if (oldConfig != null)
                oldConfig.isStoreRelease = isStoreRelease;

        if (oldConfig != null)
        {
            string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            oldConfig.HideFeedbackButton = defineSymbols.Contains("HideFeedbackButton");
            oldConfig.RemoveGeoBlocking = defineSymbols.Contains("RemoveGeoBlocking");
            oldConfig.EnableCurvedUI = defineSymbols.Contains("CURVEDUI_TMP");
            defines = "";
        }

        if (HideFeedbackButton)
            AddScriptingDefineSymbol("HideFeedbackButton");
        if (RemoveGeoBlocking)
            AddScriptingDefineSymbol("RemoveGeoBlocking");
        if (EnableCurvedUI)
            AddScriptingDefineSymbol("CURVEDUI_TMP");

        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);

        if (!String.IsNullOrEmpty(CloudProjectId))
        {
            const string assetSettingsPath = "ProjectSettings/ProjectSettings.asset";
            string text = File.ReadAllText(assetSettingsPath);

            if (oldConfig != null)
            {
                var values = _CloudIdMatch.Matches(text).Cast<Match>().ToLookup(_ => _.Groups["name"].Value, _ => _.Groups["value"].Value, StringComparer.OrdinalIgnoreCase);

                oldConfig.CloudProjectId = values["cloudProjectId"].First();
                oldConfig.CloudOrganizationId = values["organizationId"].First();
                oldConfig.CloudProjectName = values["projectName"].First();
            }

            var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "cloudProjectId", CloudProjectId },
                { "organizationId", CloudOrganizationId },
                { "projectName", CloudProjectName }
            };

            text = _CloudIdMatch.Replace(text, match =>
            {
                string key = match.Groups["name"].Value;

                string value;
                if (!replacements.TryGetValue(key, out value))
                    return match.Value; // unchanged

                return match.Value.Remove(match.Value.IndexOf(':')) + ": " + value;
            });

            File.WriteAllText(assetSettingsPath, text);
        }
    }

    /// <summary>
    /// Adds a custom scripting define symbol ot the settings
    /// </summary>
    /// <param name="toAdd">Symbol to add</param>
    private void AddScriptingDefineSymbol(string toAdd)
    {
        if (defines == "")
            defines += toAdd;
        else
            defines = defines + ";" + toAdd;
    }

    private static readonly Regex _CloudIdMatch = new Regex(@"^\s*(?<name>[^:]+)\: (?<value>.*)$", RegexOptions.Multiline);

    public void RevertProjectSettings()
    {
        if (Directory.Exists(_TempManifestDirPath))
        {
            RevertDirectory(_TempManifestDirPath);
        }

        if (File.Exists(_TempProjectSettingsPath))
        {
            var config = AssetDatabase.LoadAssetAtPath<ProjectConfiguration>(_TempProjectSettingsPath);

            config.SetProjectSettings(null);

            AssetDatabase.SaveAssets();
            AssetDatabase.DeleteAsset(_TempProjectSettingsPath);
        }
    }

    public void ApplyAssets()
    {
        RevertAssets();

        if (String.IsNullOrEmpty(AssetFolder))
            return;

        string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        string folderPath = String.Format("Assets/Configurations/{0}/", AssetFolder);
        string configPath = String.Format("Assets/Configurations/{0}/Config/", ConfigFolderName);

        //Copy config into Resources file
        FileUtil.ReplaceFile(configPath + _ConfigFileName, _LocalConfigPath + _ConfigFileName);

        using (var scope = new AssetScope(_TempDirPath))
        {

            foreach (string sourcePath in assetPaths.Where(_ => _.StartsWith(folderPath)))
            {
                Type sourceType = AssetDatabase.GetMainAssetTypeAtPath(sourcePath);

                if (sourceType == typeof(DefaultAsset))
                    continue; // skip folders, other unknown types

                string relativePath = sourcePath.Remove(sourcePath.LastIndexOf('.')).Substring(folderPath.Length);

                string destFilter = String.Format("Assets/{0}.", relativePath); // ignore extension

                string destPath = assetPaths.FirstOrDefault(_ => _.StartsWith(destFilter));

                if (String.IsNullOrEmpty(destPath))
                {
                    Debug.LogWarningFormat("Asset found at '{0}', but no matching asset found to replace at '{1}*'.", sourcePath, destFilter);
                    continue;
                }

                Type destType = AssetDatabase.GetMainAssetTypeAtPath(destPath);

                if (destType != sourceType)
                {
                    Debug.LogWarningFormat("Asset found at '{0}', but type {1} does not match type of asset found at '{2}', which is {3}", sourcePath, sourceType, destPath, destType);
                    continue;
                }

                Debug.LogFormat("Moving '{0}' to '{1}'", sourcePath, destPath);

                scope.Backup(destPath);
                FileUtil.ReplaceFile(sourcePath, destPath);
            }

        }
    }

    private class AssetScope : IDisposable
    {
        private string _Path;
        public AssetScope(string path)
        {
            _Path = path;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            AssetDatabase.StartAssetEditing();
        }

        public void Dispose()
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        public void Backup(string destPath)
        {
            FileUtil.CopyFileOrDirectory(destPath, Path.Combine(_Path, AssetDatabase.AssetPathToGUID(destPath)));
        }
    }

    public void RevertAssets()
    {
        RevertDirectory(_TempDirPath);
    }

    private void RevertDirectory(string path)
    {
        if (!Directory.Exists(path))
            return;

        AssetDatabase.StartAssetEditing();
        foreach (var file in Directory.GetFiles(path))
        {
            string destPath = AssetDatabase.GUIDToAssetPath(Path.GetFileName(file));
            Debug.LogFormat("Moving {0} to {1}", file, destPath);
            FileUtil.ReplaceFile(file, destPath);
        }
        AssetDatabase.StopAssetEditing();

        Directory.Delete(path, true);
        AssetDatabase.Refresh();
    }

    //Executed `D:/SourceTree/TrueVR/Assets\..\Assets\Firebase\Editor\generate_xml_from_google_services_json.exe -i "D:/SourceTree/TrueVR/Assets\..\Assets/Plugins/JSON/PushNotifications/google-services.json" -o "D:/SourceTree/TrueVR/Assets\..\Assets\Plugins\Android\Firebase\res\values\google-services.xml" -p "com.vokevr.vokeviewer"`
    /// <summary>
    /// When the package name changes, the google-services.xml needs to refresh to reflect the current app,
    /// otherwise notifications are sent to the app whose appID is embedded (TrueVR)
    /// </summary>
    private void RefreshFirebaseXML()
    {
        string _GoogleServicesJSONPath = "\"" + Application.dataPath + "/Plugins/JSON/PushNotifications/google-services.json" + "\"";
        string _GoogleServicesXMLPath = "\"" + Application.dataPath + "/Plugins/Android/Firebase/res/values/google-services.xml" + "\"";
        string bundleIdentifier = "\"" + BundleIdentifier + "\"";
        string args = " -i " + _GoogleServicesJSONPath + " -o " + _GoogleServicesXMLPath + " -p " + bundleIdentifier;

        //Debug.Log("Running exe" + Application.dataPath + "/Firebase/Editor/generate_xml_from_google_services_json.exe " + "with arguments : " + args);

        System.Diagnostics.Process.Start(Application.dataPath + "/Firebase/Editor/generate_xml_from_google_services_json.exe", args);
    }
}