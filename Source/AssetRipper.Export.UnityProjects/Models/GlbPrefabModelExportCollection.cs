using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_18;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Models
{
	public sealed class GlbPrefabModelExportCollection : AssetsExportCollection<IGameObject>
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
