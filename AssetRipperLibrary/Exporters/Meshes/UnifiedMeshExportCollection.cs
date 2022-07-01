using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Library.Configuration;

namespace AssetRipper.Library.Exporters.Meshes
{
	public sealed class UnifiedMeshExportCollection : AssetExportCollection
	{
		public UnifiedMeshExportCollection(UnifiedMeshExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			return ((UnifiedMeshExporter)AssetExporter).ExportFormat.GetFileExtension();
		}
	}
}
