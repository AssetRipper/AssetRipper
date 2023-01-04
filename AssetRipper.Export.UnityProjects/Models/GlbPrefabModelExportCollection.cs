using AssetRipper.Assets;
using AssetRipper.Import.Project.Collections;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_18;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Models
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
