using AssetRipper.Core;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using System.IO;

namespace AssetRipper.Library.Exporters.Shaders
{
	/// <summary>
	/// An exporter for the occasional situation where a shader asset actually contains the shader source code
	/// </summary>
	public class SimpleShaderExporter : BinaryAssetExporter
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			if (asset is IShader && asset is ITextAsset textAsset)
			{
				return HasDecompiledShaderText(textAsset.Script_C49.String);
			}
			else
			{
				return false;
			}
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			TaskManager.AddTask(File.WriteAllBytesAsync(path, ((ITextAsset)asset).Script_C49.Data));
			return true;
		}

		private static bool HasDecompiledShaderText(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}

			return !text.Contains("Program") && !text.Contains("SubProgram");
		}
	}
}
