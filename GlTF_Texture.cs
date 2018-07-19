#if UNITY_EDITOR
using UnityEngine;
using System.Collections;

public class GlTF_Texture : GlTF_Writer {
	/*
		"texture_O21_jpg": {
			"format": 6408,
			"internalFormat": 6408,
			"sampler": "sampler_0",
			"source": "O21_jpg",
			"target": 3553,
			"type": 5121
		},
*/
	public int samplerIndex;
	public int source;
	public bool flipy = true;

  // Create differnet names based on the format
  // Opaque and Transparent textures are exported differently.
	public static string GetNameFromObject(Object o, IMAGETYPE format)
	{
    string transparencyStyle = (
        format == IMAGETYPE.RGBA_OPAQUE) ? "OPAQUE" : "TRANSPARENT";

    // Don't use the object ID, so that we can deterministically produce
    // the same filenames when exporting the same asset again.
    return "texture_" + transparencyStyle + "_" +
        GlTF_Writer.GetNameFromObject(o, false);
  }

  public static string GetNameFromObject(Object o)
  {
    return GlTF_Texture.GetNameFromObject(o, IMAGETYPE.RGBA);
  }

	public override void Write()
	{
		Indent();	jsonWriter.Write ("{\n");
		IndentIn();

		writeExtras();

		Indent();	jsonWriter.Write ("\"sampler\": " + samplerIndex + ",\n");
		Indent();	jsonWriter.Write ("\"source\": " + source + "\n");
		IndentOut();
		Indent();	jsonWriter.Write ("}");
	}
}
#endif