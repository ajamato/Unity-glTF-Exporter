﻿using UnityEngine;
using System.Collections;

public class GlTF_Primitive : GlTF_Writer {
	public GlTF_Attributes attributes = new GlTF_Attributes();
	public GlTF_Accessor indices;
	public string materialName;
	public int primitive =  4;
	public int semantics = 4;

	public void Populate (Mesh m)
	{
		attributes.Populate (m);
		indices.Populate (m.triangles, true);
	}

	public override void Write ()
	{
		IndentIn();
		CommaNL();
		if (attributes != null)
			attributes.Write();
		CommaNL();
		Indent();	jsonWriter.Write ("\"indices\": \"" + indices.name + "\",\n");
		Indent();	jsonWriter.Write ("\"material\": \"" + materialName + "\",\n");
		Indent();	jsonWriter.Write ("\"primitive\": " + primitive + "\n");
		// semantics
		IndentOut();
	}
}