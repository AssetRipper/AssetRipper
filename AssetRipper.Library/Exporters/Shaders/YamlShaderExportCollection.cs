using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project.Collections;

namespace AssetRipper.Library.Exporters.Shaders
{
	public sealed class YamlShaderExportCollection : AssetExportCollection
	{
		public YamlShaderExportCollection(YamlShaderExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => "asset";
	}
}
