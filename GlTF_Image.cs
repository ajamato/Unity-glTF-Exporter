#if UNITY_EDITOR
using UnityEngine;
using System.Collections;

public class GlTF_Image : GlTF_Writer {
	public string uri;

	public static string GetNameFromObject(Object o)
	{
		// Don't use the object ID, so that we can deterministically produce
		// the same filenames when exporting the same asset again.
		return "image_" + GlTF_Writer.GetNameFromObject(o, false);
	}

	public override void Write()
	{
		Indent();		jsonWriter.Write ("{\n");
		IndentIn();
		Indent();		jsonWriter.Write ("\"uri\": \"" + uri + "\"\n");
		IndentOut();
		Indent();		jsonWriter.Write ("}");
	}
}
#endif