###Modifications Made in this Fork

- Extensions/ExportToGLTF.cs - Added an option to export a zip file from the Unity editor.
- SceneToGlTFWiz.cs
 - Support Standard (Roughness setup) shader.
 - All images are exported as PNG instead of JPG sometimes.
 - Fixed bug. Non AlphaNumeric characters are cleaned from texture image names where it was missing.
- ExporterSKFB.cs
  - Disabled the Publish to Sketchfab menu item, since this is just being used as an export to zip library.
