using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Generics;
using AssetRipper.Export.UnityProjects.Meshes;
using AssetRipper.Import.Project.Exporters;
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
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is INavMeshData;
		}

		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			return new GlbExportCollection(this, asset);
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
			
			foreach (IHeightMeshData heightMeshData in asset.HeightMeshes_C238)
			{
				int[] indices = heightMeshData.Indices;
				if (indices.Length % 3 != 0)
				{
					throw new Exception("Height mesh must be triangles.");
				}

				AssetList<Vector3f_3_5_0> vertices = heightMeshData.Vertices;

				for (int i = 0; i < indices.Length; i += 3)
				{
					primitiveBuilder.AddTriangle(
						FromVector(vertices[indices[i + 2]]),
						FromVector(vertices[indices[i + 1]]),
						FromVector(vertices[indices[i]]));
				}
			}

			NodeBuilder nodeBuilder = new NodeBuilder(asset.NameString);
			sceneBuilder.AddRigidMesh(meshBuilder, nodeBuilder);
		}

		private static VertexBuilder<VertexPosition, VertexEmpty, VertexEmpty> FromVector(Vector3f_3_5_0 vector)
		{
			return new VertexBuilder<VertexPosition, VertexEmpty, VertexEmpty>(new Vector3(-vector.X, vector.Y, vector.Z));
		}
	}
}
