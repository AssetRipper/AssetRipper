using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Generics;
using AssetRipper.Export.UnityProjects.Meshes;
using AssetRipper.SourceGenerated.Classes.ClassID_238;
using AssetRipper.SourceGenerated.Subclasses.HeightMeshData;
using AssetRipper.SourceGenerated.Subclasses.Vector3f;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using System.Numerics;

namespace AssetRipper.Export.UnityProjects.NavMeshes
{
	public sealed class GlbNavMeshExporter : BinaryAssetExporter
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			if (asset is INavMeshData)
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
			byte[] data = ExportAssetToGlb((INavMeshData)asset);
			if (data.Length == 0)
			{
				return false;
			}
			File.WriteAllBytes(path, data);
			return true;
		}

		private static byte[] ExportAssetToGlb(INavMeshData asset)
		{
			SceneBuilder sceneBuilder = new SceneBuilder();
			AddAssetToSceneBuilder(sceneBuilder, asset);
			return sceneBuilder.ToGltf2().WriteGLB().ToArray();
		}

		private static void AddAssetToSceneBuilder(SceneBuilder sceneBuilder, INavMeshData asset)
		{
			MaterialBuilder material = new MaterialBuilder("DefaultMaterial");
			MeshBuilder<VertexPosition, VertexEmpty, VertexEmpty> meshBuilder = new();
			PrimitiveBuilder<MaterialBuilder, VertexPosition, VertexEmpty, VertexEmpty> primitiveBuilder = meshBuilder.UsePrimitive(material);
			
			foreach (IHeightMeshData heightMeshData in asset.HeightMeshes)
			{
				AssetList<int> indices = heightMeshData.Indices;
				if (indices.Count % 3 != 0)
				{
					throw new Exception("Height mesh must be triangles.");
				}

				AssetList<Vector3f> vertices = heightMeshData.Vertices;

				for (int i = 0; i < indices.Count; i += 3)
				{
					primitiveBuilder.AddTriangle(
						FromVector(vertices[indices[i + 2]]),
						FromVector(vertices[indices[i + 1]]),
						FromVector(vertices[indices[i]]));
				}
			}

			NodeBuilder nodeBuilder = new NodeBuilder(asset.Name);
			sceneBuilder.AddRigidMesh(meshBuilder, nodeBuilder);
		}

		private static VertexBuilder<VertexPosition, VertexEmpty, VertexEmpty> FromVector(Vector3f vector)
		{
			return new VertexBuilder<VertexPosition, VertexEmpty, VertexEmpty>(new Vector3(-vector.X, vector.Y, vector.Z));
		}
	}
}
