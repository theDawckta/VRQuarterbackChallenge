using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProjectConfiguration))]
public class ProjectConfigurationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ProjectConfiguration config = (ProjectConfiguration)target;

		if (config.DeploymentTarget == ProjectConfiguration.DeploymentPlatform.GearVR)
		{
			config.isStoreRelease = EditorGUILayout.Toggle("Store Release", config.isStoreRelease);
		}

        if (GUILayout.Button("Apply"))
        {
            config.Apply();
        }

        if (GUILayout.Button("Apply Platform Settings"))
        {
            config.ApplyPlatformSettings(config.DeploymentTarget);
        }

        if (GUILayout.Button("Apply Assets"))
        {
            config.ApplyAssets();
        }

        if (GUILayout.Button("Apply Project Settings"))
        {
            config.ApplyProjectSettings();
        }

        if (GUILayout.Button("Revert"))
        {
            config.Revert();
        }

        if (GUILayout.Button("Revert Platform Settings"))
        {
            config.RevertPlatformSettings();
        }

        if (GUILayout.Button("Revert Project Settings"))
        {
            config.RevertProjectSettings();
        }

        if (GUILayout.Button("Revert Assets"))
        {
            config.RevertAssets();
        }
    }
}