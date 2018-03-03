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
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX // edit: added Platform Dependent Compilation - win or osx standalone
		ExportToGLTF window = (ExportToGLTF)EditorWindow.GetWindow(typeof(ExportToGLTF));
		window.titleContent.text = "Export To GLTF";
		window.Show();
#else // and error dialog if not standalone
		EditorUtility.DisplayDialog("Error", "Your build target must be set to standalone", "Okay");
#endif
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