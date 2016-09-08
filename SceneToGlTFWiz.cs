/***************************************************************************
GlamExport
 - Unity3D Scriptable Wizard to export Hierarchy or Project objects as glTF


****************************************************************************/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using System.Reflection;


public class SceneToGlTFWiz : ScriptableWizard
{
//	static public List<GlTF_Accessor> Accessors;
//	static public List<GlTF_BufferView> BufferViews;
	static public GlTF_Writer writer;

	const string KEY_PATH = "GlTFPath";
	const string KEY_FILE = "GlTFFile";

    static public string path = "?";
	static XmlDocument xdoc;
	static string savedPath = EditorPrefs.GetString (KEY_PATH, "/");
	static string savedFile = EditorPrefs.GetString (KEY_FILE, "test.gltf");

	[MenuItem ("File/Export/glTF")]
	static void CreateWizard()
	{
		savedPath = EditorPrefs.GetString (KEY_PATH, "/");
		savedFile = EditorPrefs.GetString (KEY_FILE, "test.gltf");
		Debug.Log ("remembered "+savedPath+"   "+savedFile);
		path = savedPath + "/"+ savedFile;
		ScriptableWizard.DisplayWizard("Export Selected Stuff to glTF", typeof(SceneToGlTFWiz), "Export");
	}

	void OnWizardUpdate ()
	{
//		Texture[] txs = Selection.GetFiltered(Texture, SelectionMode.Assets);
//		Debug.Log("found "+txs.Length);
	}

    void OnWizardCreate() // Create (Export) button has been hit (NOT wizard has been created!)
    {
		writer = new GlTF_Writer();
		writer.Init ();
/*
		Object[] deps = EditorUtility.CollectDependencies  (trs);
		foreach (Object o in deps)
		{
			Debug.Log("obj "+o.name+"  "+o.GetType());
		}
*/		
		
		path = EditorUtility.SaveFilePanel("Save glTF file as", savedPath, savedFile, "gltf");
		if (path.Length != 0)
		{			
			savedPath = Path.GetDirectoryName(path);
			savedFile = Path.GetFileName(path);

			EditorPrefs.SetString(KEY_PATH, savedPath);
			EditorPrefs.SetString(KEY_FILE, savedFile);

			Debug.Log ("attempting to save to "+path);
			writer.OpenFiles (path);

			// FOR NOW!
			GlTF_Sampler sampler = new GlTF_Sampler("sampler1"); // make the default one for now
			GlTF_Writer.samplers.Add (sampler);
			// first, collect objects in the scene, add to lists
			Transform[] trs = Selection.GetTransforms (SelectionMode.Deep);
			foreach (Transform tr in trs)
			{
				if (tr.GetComponent<Camera>() != null)
				{
					if (tr.GetComponent<Camera>().orthographic)
					{
						GlTF_Orthographic cam;
						cam = new GlTF_Orthographic();
						cam.type = "orthographic";
						cam.zfar = tr.GetComponent<Camera>().farClipPlane;
						cam.znear = tr.GetComponent<Camera>().nearClipPlane;
						cam.name = tr.name;
						//cam.orthographic.xmag = tr.camera.
						GlTF_Writer.cameras.Add(cam);
					}
					else
					{
						GlTF_Perspective cam;
						cam = new GlTF_Perspective();
						cam.type = "perspective";
						cam.zfar = tr.GetComponent<Camera>().farClipPlane;
						cam.znear = tr.GetComponent<Camera>().nearClipPlane;
						cam.aspect_ratio = tr.GetComponent<Camera>().aspect;
						cam.yfov = tr.GetComponent<Camera>().fieldOfView;
						cam.name = tr.name;
						GlTF_Writer.cameras.Add(cam);
					}
				}
				
				if (tr.GetComponent<Light>() != null)
				{
					switch (tr.GetComponent<Light>().type)
					{
					case LightType.Point:
						GlTF_PointLight pl = new GlTF_PointLight();
						pl.color = new GlTF_ColorRGB (tr.GetComponent<Light>().color);
						pl.name = tr.name;
						GlTF_Writer.lights.Add (pl);
						break;

					case LightType.Spot:
						GlTF_SpotLight sl = new GlTF_SpotLight();
						sl.color = new GlTF_ColorRGB (tr.GetComponent<Light>().color);
						sl.name = tr.name;
						GlTF_Writer.lights.Add (sl);
						break;
						
					case LightType.Directional:
						GlTF_DirectionalLight dl = new GlTF_DirectionalLight();
						dl.color = new GlTF_ColorRGB (tr.GetComponent<Light>().color);
						dl.name = tr.name;
						GlTF_Writer.lights.Add (dl);
						break;
						
					case LightType.Area:
						GlTF_AmbientLight al = new GlTF_AmbientLight();
						al.color = new GlTF_ColorRGB (tr.GetComponent<Light>().color);
						al.name = tr.name;
						GlTF_Writer.lights.Add (al);
						break;
					}
				}					

				Mesh m = GetMesh(tr);
				if (m != null)
				{
					GlTF_Mesh mesh = new GlTF_Mesh();
					mesh.name = GlTF_Mesh.GetNameFromObject(m);

					GlTF_Accessor positionAccessor = new GlTF_Accessor(GlTF_Accessor.GetNameFromObject(m, "position"), "VEC3", "FLOAT");
					positionAccessor.bufferView = GlTF_Writer.vec3BufferView;
					GlTF_Writer.accessors.Add (positionAccessor);

					GlTF_Accessor normalAccessor = null;
					if (m.normals.Length > 0) 
					{
						normalAccessor = new GlTF_Accessor(GlTF_Accessor.GetNameFromObject(m, "normal"), "VEC3", "FLOAT");
						normalAccessor.bufferView = GlTF_Writer.vec3BufferView;
						GlTF_Writer.accessors.Add (normalAccessor);
					}

					GlTF_Accessor uv0Accessor = null;
					if (m.uv.Length > 0) {
						uv0Accessor =  new GlTF_Accessor(GlTF_Accessor.GetNameFromObject(m, "uv0"), "VEC2", "FLOAT");
						uv0Accessor.bufferView = GlTF_Writer.vec2BufferView;
						GlTF_Writer.accessors.Add (uv0Accessor);
					}

					GlTF_Accessor uv1Accessor = null;
					if (m.uv2.Length > 0) {
						uv1Accessor =  new GlTF_Accessor(GlTF_Accessor.GetNameFromObject(m, "uv1"), "VEC2", "FLOAT");
						uv1Accessor.bufferView = GlTF_Writer.vec2BufferView;
						GlTF_Writer.accessors.Add (uv1Accessor);
					}

					GlTF_Accessor uv2Accessor = null;
					if (m.uv3.Length > 0) {
						uv2Accessor =  new GlTF_Accessor(GlTF_Accessor.GetNameFromObject(m, "uv2"), "VEC2", "FLOAT");
						uv2Accessor.bufferView = GlTF_Writer.vec2BufferView;
						GlTF_Writer.accessors.Add (uv2Accessor);
					}

					GlTF_Accessor uv3Accessor = null;
					if (m.uv4.Length > 0) {
						uv3Accessor =  new GlTF_Accessor(GlTF_Accessor.GetNameFromObject(m, "uv3"), "VEC2", "FLOAT");
						uv3Accessor.bufferView = GlTF_Writer.vec2BufferView;
						GlTF_Writer.accessors.Add (uv3Accessor);
					}

					var smCount = m.subMeshCount;
					for (var i = 0; i < smCount; ++i) 
					{						
						GlTF_Primitive primitive = new GlTF_Primitive();
						primitive.name = GlTF_Primitive.GetNameFromObject(m, i);
						primitive.index = i;
						GlTF_Attributes attributes = new GlTF_Attributes();
						attributes.positionAccessor = positionAccessor;
						attributes.normalAccessor = normalAccessor;
						attributes.texCoord0Accessor = uv0Accessor;
						attributes.texCoord1Accessor = uv1Accessor;
						attributes.texCoord2Accessor = uv2Accessor;
						attributes.texCoord3Accessor = uv3Accessor;
						primitive.attributes = attributes;
						GlTF_Accessor indexAccessor = new GlTF_Accessor(GlTF_Accessor.GetNameFromObject(m, "indices"), "SCALAR", "USHORT");
						indexAccessor.bufferView = GlTF_Writer.ushortBufferView;
						GlTF_Writer.accessors.Add (indexAccessor);
						primitive.indices = indexAccessor;

						var mr = GetRenderer(tr);
						var sm = mr.sharedMaterials;
						if (i < sm.Length) {
							var mat = sm[i];
							var matName = GlTF_Material.GetNameFromObject(mat);
							primitive.materialName = matName;
							if (!GlTF_Writer.materials.ContainsKey (matName))
							{
								GlTF_Material material = new GlTF_Material();	
								material.name = matName;
								material.diffuse = new GlTF_MaterialColor ("diffuse", mat.color);
								GlTF_Writer.materials.Add (material.name, material);

								// if there are textures, add them too
								if (mat.mainTexture != null)
								{
									var tex = mat.mainTexture;
									var texName = "texture_" + tex.name + "_" + tex.GetInstanceID();
									if (!GlTF_Writer.textures.ContainsKey (texName))
									{
										GlTF_Texture texture = new GlTF_Texture ();
										texture.name = texName;
										texture.source = AssetDatabase.GetAssetPath(tex);
										texture.samplerName = sampler.name; // FIX! For now!
										GlTF_Writer.textures.Add (texName, texture);
										material.diffuse = new GlTF_MaterialTexture ("diffuse", texture);
									}
								}

								//technique
								var s = mat.shader;
								var techName = GlTF_Technique.GetNameFromObject(s);
								material.instanceTechniqueName = techName;
								if (!GlTF_Writer.techniques.ContainsKey(techName)) 
								{
									GlTF_Technique tech = new GlTF_Technique();
									tech.name = techName;
									GlTF_Writer.techniques.Add (techName, tech);

									int spCount = ShaderUtil.GetPropertyCount(s);
									for (var j = 0; j < spCount; ++j) 
									{
										var pName = ShaderUtil.GetPropertyName(s, j);
										var pType = ShaderUtil.GetPropertyType(s, j);
										Debug.Log(pName + " " +  pType);

									}

									// create program
									GlTF_Program program = new GlTF_Program();
									program.name = GlTF_Program.GetNameFromObject(s);
									tech.program = program.name;
									GlTF_Writer.programs.Add(program);

									// shader
									GlTF_Shader vs = new GlTF_Shader();
									vs.name = GlTF_Shader.GetNameFromObject(s, GlTF_Shader.Type.Vertex);
									program.vertexShader = vs.name;
									vs.type = GlTF_Shader.Type.Vertex;
									vs.uri = "DefaultVS.glsl";
									GlTF_Writer.shaders.Add(vs);

									GlTF_Shader fs = new GlTF_Shader();
									fs.name = GlTF_Shader.GetNameFromObject(s, GlTF_Shader.Type.Fragment);
									program.fragmentShader = fs.name;
									fs.type = GlTF_Shader.Type.Fragment;
									fs.uri = "DefaultFS.glsl";
									GlTF_Writer.shaders.Add(fs);
								}
							}
						}

						mesh.primitives.Add (primitive);
					}

					mesh.Populate (m);
					GlTF_Writer.meshes.Add (mesh);



//					GlTF_Accessor normalAccessor = new GlTF_Accessor("normalAccessor-" + tr.name + "_FIXTHIS", "VEC3", "FLOAT");
//					GlTF_Accessor positionAccessor = new GlTF_Accessor("positionAccessor-" + tr.name + "_FIXTHIS", "VEC3", "FLOAT");
//					GlTF_Accessor texCoord0Accessor = new GlTF_Accessor("texCoord0Accessor-" + tr.name + "_FIXTHIS", "VEC2", "FLOAT");
//					GlTF_Accessor indexAccessor = new GlTF_Accessor("indicesAccessor-" + tr.name + "_FIXTHIS", "SCALAR", "USHORT");
//					indexAccessor.bufferView = GlTF_Writer.ushortBufferView;
//					normalAccessor.bufferView = GlTF_Writer.vec3BufferView;
//					positionAccessor.bufferView = GlTF_Writer.vec3BufferView;
//					texCoord0Accessor.bufferView = GlTF_Writer.vec2BufferView;
//					GlTF_Mesh mesh = new GlTF_Mesh();
//					mesh.name = "mesh-" + tr.name;
//					GlTF_Primitive primitive = new GlTF_Primitive();
//					primitive.name = "primitive-"+tr.name+"_FIXTHIS";
//					GlTF_Attributes attributes = new GlTF_Attributes();
//					attributes.normalAccessor = normalAccessor;
//					attributes.positionAccessor = positionAccessor;
//					attributes.texCoord0Accessor = texCoord0Accessor;
//					primitive.attributes = attributes;
//					primitive.indices = indexAccessor;
//					mesh.primitives.Add (primitive);
//					mesh.Populate (m);
//					GlTF_Writer.accessors.Add (normalAccessor);
//					GlTF_Writer.accessors.Add (positionAccessor);
//					GlTF_Writer.accessors.Add (texCoord0Accessor);
//					GlTF_Writer.accessors.Add (indexAccessor);
//					GlTF_Writer.meshes.Add (mesh);
//
//					foreach (var mat in mr.sharedMaterials) 
//					{
//						var id = mat.GetInstanceID();
//						if (!usedMaterials.ContainsKey(id)) 
//						{	
//							GlTF_Material material = new GlTF_Material();
//							material.name = "material_" +  mat.name + "_" + id;
//							usedMaterials[id] = material;
//							GlTF_Writer.materials.Add (material.name, material);
//						}
//					}
//
//					// next, add material(s) to dictionary (when unique)
//					string matName = mr.sharedMaterial.name;
//					if (matName == "")
//						matName = "material-diffault-diffuse";
//					else
//						matName = "material-" + matName;
//					primitive.materialName = matName;
//					if (!GlTF_Writer.materials.ContainsKey (matName))
//					{
//						GlTF_Material material = new GlTF_Material();
//						material.name = matName;
//						if (mr.sharedMaterial.HasProperty ("shininess"))
//							material.shininess = mr.sharedMaterial.GetFloat("shininess");
//						material.diffuse = new GlTF_MaterialColor ("diffuse", mr.sharedMaterial.color);
//						//material.ambient = new GlTF_Color ("ambient", mr.material.color);
//						
//						if (mr.sharedMaterial.HasProperty ("specular"))
//						{
//							Color sc = mr.sharedMaterial.GetColor ("specular");
//							material.specular = new GlTF_MaterialColor ("specular", sc);
//						}
//						GlTF_Writer.materials.Add (material.name, material);
//
//						// if there are textures, add them too
//						if (mr.sharedMaterial.mainTexture != null)
//						{
//							if (!GlTF_Writer.textures.ContainsKey (mr.sharedMaterial.mainTexture.name))
//							{
//								GlTF_Texture texture = new GlTF_Texture ();
//								texture.name = mr.sharedMaterial.mainTexture.name;
//								texture.source = AssetDatabase.GetAssetPath(mr.sharedMaterial.mainTexture);
//								texture.samplerName = sampler.name; // FIX! For now!
//								GlTF_Writer.textures.Add (mr.sharedMaterial.mainTexture.name, texture);
//								material.diffuse = new GlTF_MaterialTexture ("diffuse", texture);
//							}
//						}
//					}
					
				}

				Animation a = tr.GetComponent<Animation>();
				
//				Animator a = tr.GetComponent<Animator>();				
				if (a != null)
				{
					AnimationClip[] clips = AnimationUtility.GetAnimationClips(tr.gameObject);
					int nClips = clips.Length;
//					int nClips = a.GetClipCount();
					for (int i = 0; i < nClips; i++)
					{
						GlTF_Animation anim = new GlTF_Animation(a.name);
						anim.Populate (clips[i]);
						GlTF_Writer.animations.Add (anim);
					}
				}

	
				// next, build hierarchy of nodes
				GlTF_Node node = new GlTF_Node();
				if (tr.parent != null)
					node.hasParent = true;
				if (tr.localPosition != Vector3.zero)
					node.translation = new GlTF_Translation (tr.localPosition);
				if (tr.localScale != Vector3.one)
					node.scale = new GlTF_Scale (tr.localScale);
				if (tr.localRotation != Quaternion.identity)
					node.rotation = new GlTF_Rotation (tr.localRotation);
				node.name = tr.name;
				if (tr.GetComponent<Camera>() != null)
				{
					node.cameraName = tr.name;
				}
				else if (tr.GetComponent<Light>() != null)
					node.lightName = tr.name;
				else if (m != null)
				{
					node.meshNames.Add (GlTF_Mesh.GetNameFromObject(m));
				}

				foreach (Transform t in tr.transform)
					node.childrenNames.Add ("node-" + t.name);
				
				GlTF_Writer.nodes.Add (node);
			}				

			// third, add meshes etc to byte stream, keeping track of buffer offsets
			writer.Write ();
			writer.CloseFiles();
		}
	}
	
	static string toGlTFname(string name)
	{
		// remove spaces and illegal chars, replace with underscores
		string correctString = name.Replace(" ", "_");
		// make sure it doesn't start with a number
		return correctString; 
	}
	
	static bool isInheritedFrom (Type t, Type baseT)
	{
		if (t == baseT)
			return true;
		t = t.BaseType;
		while (t != null && t != typeof(System.Object))
		{
			if (t == baseT)
				return true;
			t = t.BaseType;
		}
		return false;
	}

	static Renderer GetRenderer(Transform tr) 
	{
		Renderer mr = tr.GetComponent<MeshRenderer>();
		if (mr == null) {
			mr = tr.GetComponent<SkinnedMeshRenderer>();
		}
		return mr;
	}

	static Mesh GetMesh(Transform tr)
	{
		var mr = GetRenderer(tr);
		Mesh m = null;
		if (mr != null)
		{
			var t = mr.GetType();
			if (t == typeof(MeshRenderer)) 
			{
				MeshFilter mf = tr.GetComponent<MeshFilter>();
				m = mf.sharedMesh;
			} else if (t == typeof(SkinnedMeshRenderer)) 
			{
				SkinnedMeshRenderer smr = mr as SkinnedMeshRenderer;
				m = smr.sharedMesh;
			}
		}
		return m;
	}
}
