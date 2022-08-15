using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Utils;

namespace AssetRipper.Library.Exporters.Shaders
{
	public sealed class YamlShaderExportCollection : AssetExportCollection
	{
		public YamlShaderExportCollection(YamlShaderExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => "asset";
		
		protected override bool ExportInner(IProjectAssetContainer container, string filePath, string dirPath)
		{
			UnityPatchUtils.ApplyPatchFromManifestResource(typeof(YamlShaderExporter).Assembly, UnityPatchName, dirPath);
			return base.ExportInner(container, filePath, dirPath);
		}
		
		private const string UnityPatchName = "AssetRipper.Library.Exporters.Shaders.UnityPatch.YamlShaderPostprocessor.txt";
	}
}
