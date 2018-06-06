using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(SphericalScreen))] 
public class SphericalScreenEditor : Editor
{

	[MenuItem ("GameObject/Create Other/SphericalScreen")]
	static void Create ()
	{
		GameObject gameObject = new GameObject ("SphericalScreen");
		SphericalScreen screen = gameObject.AddComponent<SphericalScreen> ();
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter> ();
		meshFilter.mesh = new Mesh ();
		screen.Rebuild ();
	}

	public override void OnInspectorGUI ()
	{
		SphericalScreen obj;

		obj = target as SphericalScreen;

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
