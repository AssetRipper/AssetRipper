using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_18;

namespace AssetRipper.Library.Exporters.Models
{
	public sealed class GlbPrefabModelExportCollection : AssetsExportCollection
	{
		public GlbPrefabModelExportCollection(GlbModelExporter assetExporter, IGameObject root) : base(assetExporter, root)
		{
			foreach (IEditorExtension extension in root.FetchHierarchy())
			{
				AddAsset(extension);
			}
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => "glb";
	}
}
