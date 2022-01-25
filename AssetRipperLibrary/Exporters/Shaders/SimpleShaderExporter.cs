using AssetRipper.Core;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Exporters;
using System.IO;
using System.Text;

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
				return HasDecompiledShaderText(textAsset.ParseWithUTF8());
			else
				return false;
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			TaskManager.AddTask(File.WriteAllBytesAsync(path, ((ITextAsset)asset).Script));
			return true;
		}

		private static bool HasDecompiledShaderText(string text)
		{
			if (string.IsNullOrEmpty(text))
				return false;

			return !text.Contains("Program") && !text.Contains("SubProgram");
		}
	}
}
