#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

public class ExportToGLTF : EditorWindow
{
	GameObject exporterGo;
	SceneToGlTFWiz exporter;
	string path = "";

	[MenuItem("Tools/Export Selected to GLTF")]
	static void Export()
	{
		ExportToGLTF window = (ExportToGLTF)EditorWindow.GetWindow(typeof(ExportToGLTF));
		window.titleContent.text = "Export To GLTF";
		window.Show();
	}

	void Awake() {
		exporterGo = new GameObject("Exporter");
		exporter = exporterGo.AddComponent<SceneToGlTFWiz>();
	}

	void OnGUI()
	{
		path = EditorGUILayout.TextField("Output File", path);

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Select Output File", GUILayout.Width(150), GUILayout.Height(40))) {
			// Hack to deal with the fact that its implemented
			// in a Coroutine.
			path = EditorUtility.SaveFilePanel("glTF Export Filename", "", "", "gltf");
		}

		EditorGUI.BeginDisabledGroup(path == String.Empty || path.EndsWith(".zip"));
		if (GUILayout.Button("Convert", GUILayout.Width(150), GUILayout.Height(40))) {
			exporter.ExportCoroutine(path, null, true, true, false, true);
		}
		EditorGUI.EndDisabledGroup();
		GUILayout.EndHorizontal();
	}

	void OnDestroy()
	  {
		  if (exporterGo)
		  {
			  DestroyImmediate(exporterGo);
			  exporter = null;
		  }
	}
}

#endif