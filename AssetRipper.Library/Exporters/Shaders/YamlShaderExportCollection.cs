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

		private const string RegisterShaderUnityPatchName = "AssetRipper.Library.Exporters.Shaders.UnityPatch.YamlShaderPostprocessor.txt";
		private const string FileLockerUnityPatchName = "AssetRipper.Library.Exporters.Shaders.UnityPatch.YamlShaderLocker.txt";
	}
}
