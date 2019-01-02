using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.IO;
using System.Xml;

public class ProjectBuilder
{
    // Constant Variables
    private const string KEYSTORE_FILE = "user.keystore";
    private const string KEYSTORE_PASS = "R1tesh@k4le";
    private const string KEYALIAS_NAME = "vokevr";
    private const string KEYALIAS_PASS = "R1tesh@k4le";
        
    // Static Variables
    private static string m_sAppTargetPlatform;
    private static string m_sAppFileName;
    private static string m_sAppVersion;
    private static string m_sProjectPath;
    private static string m_sBranchName;

    /// <summary>
    /// It configures project settings before performing a build 
    /// </summary>
    public static void ConfigureProject()
    {
        EditorPrefs.SetString("AndroidSdkRoot", "C:/Users/dmathur/AppData/Local/Android/sdk");

        // Update values for the common variables
        updateRequiredVariables();

        // Get the Project name and asset path
        int startIndex = (m_sAppFileName.LastIndexOf("/") != -1)? m_sAppFileName.LastIndexOf("/") + 1 : 0;
        int endIndex = (m_sAppFileName.LastIndexOf(".") != -1)? m_sAppFileName.LastIndexOf(".") : m_sAppFileName.Length;
        string projectName = m_sAppFileName.Substring(startIndex, endIndex - startIndex);
        string assetFile = "Assets/Configurations/" + projectName + "/" + projectName +".asset";

        // Check if the asset file exists
        if (!File.Exists(m_sProjectPath + "/" + assetFile))
            throw new UnityException ("The configuration asset file does not exits. File=" + assetFile);

        // White-label the project
        Debug.Log("Apply the asset file from " + assetFile);
        ProjectConfiguration config = AssetDatabase.LoadAssetAtPath<ProjectConfiguration>(assetFile);
        config.Apply();

        // Write a version to a text file
        string majorVersion = Application.version.Split('.')[0];
        string minorVerison = Application.version.Split('.')[1];
        string versionText = string.Format("MajorVersion={0}\r\nMinorVersion={1}", majorVersion, minorVerison);
        File.WriteAllText(m_sProjectPath + "/Version.txt", versionText);
        Debug.Log("The version file is created. File=" + m_sProjectPath + "/Version.txt");
        Debug.Log("The application version is " + versionText.Replace("\r\n", ", "));

        // Configure platform-specific settings
        if (m_sBranchName.ToLower().Equals("master") || m_sBranchName.ToLower().StartsWith("release"))
        {
            if (m_sAppTargetPlatform.ToUpper() == "GEARVR")
            {
                // Update the manifest file
                string xmlFile = m_sProjectPath + "/Assets/Plugins/Android/AndroidManifest.xml";
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);

                // Update the attributes
                XmlNode nodeCategory = xmlDoc.DocumentElement.SelectSingleNode("/manifest/application/activity/intent-filter/category");
                if (nodeCategory != null)
                {
                    nodeCategory.Attributes["android:name"].Value = "android.intent.category.INFO";
                }

                xmlDoc.Save(xmlFile);
            }
        }
    }

    /// <summary>
    /// It execute a build process accroding to the given platform
    /// </summary>
	public static void PerformBuild()
	{
        // Update values for the common variables
        updateRequiredVariables();

        // Switch the build platform
        BuildTarget buildTarget;
        if (m_sAppTargetPlatform.ToUpper() == "GEARVR")
        {
            buildTarget = BuildTarget.Android;
        }
        else
        {
            throw new UnityException ("The build target (" + m_sAppTargetPlatform + ") is not supported.");
        }

        // Switch the target platform if needed
        if (EditorUserBuildSettings.activeBuildTarget != buildTarget) 
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget (buildTarget);
        }

        // Update the player settings according to the given platforme
        if (m_sAppTargetPlatform.ToUpper() == "GEARVR") 
        {
            PlayerSettings.virtualRealitySupported = true;
            PlayerSettings.SetScriptingDefineSymbolsForGroup (BuildTargetGroup.Android, "");

            // Update the keystore
            if (m_sBranchName.ToLower().Equals("master") || m_sBranchName.ToLower().StartsWith("release") || m_sBranchName.ToLower().StartsWith("dev"))
            {
                PlayerSettings.Android.keystoreName = m_sProjectPath + "/Keystore/" + KEYSTORE_FILE;
                PlayerSettings.Android.keystorePass = KEYSTORE_PASS;
                PlayerSettings.Android.keyaliasName = KEYALIAS_NAME;
                PlayerSettings.Android.keyaliasPass = KEYALIAS_PASS;
            }
        }

        // Add scenes to be built
        string[] scenes = null;
        if (m_sAppTargetPlatform.ToUpper() == "GEARVR")
        {
            scenes = new string[]
                { "Assets/Scenes/SplashScene.unity", "Assets/Scenes/TransitionScene.unity", 
                  "Assets/Scenes/HomeScene.unity", "Assets/Scenes/ConsumptionScene.unity"
                };
        }

        // Build the project
        BuildPipeline.BuildPlayer (scenes, m_sAppFileName, buildTarget, BuildOptions.None);
	}

    /// <summary>
    /// It cleans up the project after building the project
    /// </summary>
    private static void CleanupProject()
    {
    }
        
    private static void updateRequiredVariables()
    {
        m_sAppTargetPlatform = getValueOfArgument ("-buildAppTarget").ToUpper();
        if (string.IsNullOrEmpty(m_sAppTargetPlatform))
            throw new UnityException ("The build target is not specified.");

        m_sAppFileName = getValueOfArgument ("-buildAppName");
        if (string.IsNullOrEmpty (m_sAppFileName))
            throw new UnityException ("The application file name is not specified.");

        m_sAppVersion = getValueOfArgument ("-buildAppVersion");
        if (string.IsNullOrEmpty (m_sAppVersion))
            throw new UnityException ("The application file name is not specified.");

        m_sProjectPath = getValueOfArgument ("-projectPath");
        if (string.IsNullOrEmpty (m_sProjectPath))
            throw new UnityException ("The project path is not specified.");

        m_sBranchName = getValueOfArgument("-buildAppBranch");
        if (string.IsNullOrEmpty(m_sBranchName))
            throw new UnityException("The branch name is not specified.");

        Debug.Log ("Platform: " + m_sAppTargetPlatform + ", Application Name:" + m_sAppFileName + ", Application Version:" + m_sAppVersion + ", Branch Name:" + m_sBranchName);

    }

	private static string getValueOfArgument(string argName)
	{
		string[] arguments = System.Environment.GetCommandLineArgs ();
		//Debug.Log ("GetCommandLineArgs: " + string.Join (", ", arguments));

		var indices = arguments.Select ((s, i) => new {Index = i, Value = s})
			.Where (t => t.Value == argName)
			.Select (t => t.Index)
			.ToList ();

		if (indices == null || indices.Count <= 0) 
		{
			return string.Empty;
		}

		return arguments [indices [0] + 1];
	}
}
