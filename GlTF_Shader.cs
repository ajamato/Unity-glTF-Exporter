#if UNITY_EDITOR
using UnityEngine;
using System.Collections;

public class GlTF_Shader : GlTF_Writer {
	public enum Type {
		Vertex,
		Fragment
	}

	public Type type = Type.Vertex;
	public string uri = "";

	public static string GetNameFromObject(Object o, Type type)
	{
		// Don't use the object ID, so that we can deterministically produce
		// the same filenames when exporting the same asset again.
		var name = GlTF_Writer.GetNameFromObject(o, false);
		var typeName = type == Type.Vertex ? "vertex" : "fragment";
		return typeName + "_" + name;
	}

	public override void Write()
	{
		Indent();		jsonWriter.Write ("\"" + name + "\": {\n");
		IndentIn();
		Indent();		jsonWriter.Write ("\"type\": " + TypeStr() +",\n");
		Indent();		jsonWriter.Write ("\"uri\": \"" + uri +"\"\n");
		IndentOut();
		Indent();		jsonWriter.Write ("}");
	}

	int TypeStr()
	{
		if (type == Type.Vertex)
		{
			return 35633;
		}
		else if (type == Type.Fragment)
		{
			return 35632;
		}

		return 0;
	}
}
#endif