using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Import.Project.Collections;
using AssetRipper.Import.Utils;

namespace AssetRipper.Export.UnityProjects.Shaders
{
	public sealed class YamlShaderExportCollection : AssetExportCollection
	{
		public YamlShaderExportCollection(YamlShaderExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => "asset";
		
		protected override bool ExportInner(IProjectAssetContainer container, string filePath, string dirPath)
		{
			// This patch uses ShaderUtil.RegisterShader(), which is only available start from Unity 2018.
			if (container.ExportVersion.IsGreaterEqual(2018, 1, 0))
			{
				UnityPatchUtils.ApplyPatchFromManifestResource(typeof(YamlShaderExporter).Assembly, RegisterShaderUnityPatchName, dirPath);
			}
			// This patch uses AssetModificationProcessor, which is only available start from Unity 3.5.
			if (container.ExportVersion.IsGreaterEqual(3, 5, 0))
			{
				UnityPatchUtils.ApplyPatchFromManifestResource(typeof(YamlShaderExporter).Assembly, FileLockerUnityPatchName, dirPath);
			}
			return base.ExportInner(container, filePath, dirPath);
		}

		private const string RegisterShaderUnityPatchName = "AssetRipper.Export.UnityProjects.Shaders.UnityPatch.YamlShaderPostprocessor.txt";
		private const string FileLockerUnityPatchName = "AssetRipper.Export.UnityProjects.Shaders.UnityPatch.YamlShaderLocker.txt";
	}
}
