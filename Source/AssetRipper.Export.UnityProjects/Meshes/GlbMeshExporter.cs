using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Extensions;
using SharpGLTF.Scenes;

namespace AssetRipper.Export.UnityProjects.Meshes
{
	public sealed class GlbMeshExporter : BinaryAssetExporter
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			if (asset is IMesh mesh && mesh.IsSet())
			{
				exportCollection = new GlbExportCollection(this, asset);
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
			byte[] data = ExportBinary((IMesh)asset);
			if (data.Length == 0)
			{
				return false;
			}
			File.WriteAllBytes(path, data);
			return true;
		}

		private static byte[] ExportBinary(IMesh mesh)
		{
			SceneBuilder sceneBuilder = GlbMeshBuilder.Build(mesh);

			SharpGLTF.Schema2.WriteSettings writeSettings = new();

			return sceneBuilder.ToGltf2().WriteGLB(writeSettings).ToArray();
		}
	}
}
