using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(CylindricalScreen))] 
public class CylindricalScreenEditor : Editor
{

	[MenuItem ("GameObject/Create Other/CylindricalScreen")]
	static void Create ()
	{
		GameObject gameObject = new GameObject ("CylindricalScreen");
		CylindricalScreen screen = gameObject.AddComponent<CylindricalScreen> ();
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter> ();
		meshFilter.mesh = new Mesh ();
		screen.Rebuild ();
	}

	public override void OnInspectorGUI ()
	{
		CylindricalScreen obj;

		obj = target as CylindricalScreen;

		if (obj == null) {
			return;
		}

		base.DrawDefaultInspector ();

		EditorGUILayout.BeginHorizontal ();

		// Rebuild mesh when user click the Rebuild button
		if (GUILayout.Button ("Rebuild")) {
			obj.Rebuild ();
		}
		EditorGUILayout.EndHorizontal ();
	}
}
