#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GlTF_Program : GlTF_Writer {
	public List<string> attributes = new List<string>();
	public string vertexShader = "";
	public string fragmentShader = "";

	public static string GetNameFromObject(Object o)
	{
		// Don't use the object ID, so that we can deterministically produce
		// the same filenames when exporting the same asset again.
		return "program_" + GlTF_Writer.GetNameFromObject(o, false);
	}

	public override void Write()
	{
		Indent();		jsonWriter.Write ("{\n");
		IndentIn();
		Indent();		jsonWriter.Write ("\"attributes\": [\n");
		IndentIn();
		foreach (var a in attributes)
		{
			CommaNL();
			Indent();	jsonWriter.Write ("\"" + a + "\"");
		}
		jsonWriter.Write ("\n");
		IndentOut();
		Indent();		jsonWriter.Write ("],\n");
		Indent();		jsonWriter.Write ("\"vertexShader\": \"" + vertexShader + "\",\n");
		Indent();		jsonWriter.Write ("\"fragmentShader\": \"" + fragmentShader + "\"\n");
		IndentOut();
		Indent();		jsonWriter.Write ("}");
	}
}
#endif