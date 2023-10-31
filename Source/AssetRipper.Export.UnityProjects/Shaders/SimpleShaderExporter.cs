﻿using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.SourceGenerated.Classes.ClassID_48;

namespace AssetRipper.Export.UnityProjects.Shaders
{
	/// <summary>
	/// An exporter for the occasional situation where a shader asset actually contains the shader source code
	/// </summary>
	public class SimpleShaderExporter : ShaderExporterBase
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, TemporaryAssetCollection temporaryFile, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			if (asset is IShader shader && shader.Has_Script() && HasDecompiledShaderText(shader.Script.String))
			{
				exportCollection = new ShaderExportCollection(this, shader);
				return true;
			}
			else
			{
				exportCollection = null;
				return false;
			}
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			using FileStream stream = File.OpenWrite(path);
			stream.Write(((IShader)asset).Script!.Data);
			stream.Flush();
			return true;
		}

		private static bool HasDecompiledShaderText(string text)
		{
			return !string.IsNullOrEmpty(text)
				&& !text.Contains("Program")
				&& !text.Contains("SubProgram");
		}
	}
}
