using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.Export.UnityProjects.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Classes.ClassID_49;

namespace AssetRipper.Export.UnityProjects.Shaders
{
	/// <summary>
	/// An exporter for the occasional situation where a shader asset actually contains the shader source code
	/// </summary>
	public class SimpleShaderExporter : BinaryAssetExporter
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, TemporaryAssetCollection temporaryFile, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			if (asset is IShader and ITextAsset textAsset && HasDecompiledShaderText(textAsset.Script_C49.String))
			{
				exportCollection = new AssetExportCollection(this, asset);
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
			File.WriteAllBytes(path, ((ITextAsset)asset).Script_C49.Data);
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
