using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.Export.Modules.Models;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_238;
using AssetRipper.SourceGenerated.Subclasses.HeightMeshData;
using AssetRipper.SourceGenerated.Subclasses.Vector3f;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using System.Diagnostics;
using System.Numerics;

namespace AssetRipper.Export.PrimaryContent.Models;

public sealed class GlbNavMeshExporter : IContentExtractor
{
	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out ExportCollectionBase? exportCollection)
	{
		if (asset is INavMeshData { HeightMeshes: not null })
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

	public bool Export(IUnityObjectBase asset, string path, FileSystem fileSystem)
	{
		SceneBuilder sceneBuilder = new();
		AddAssetToSceneBuilder(sceneBuilder, (INavMeshData)asset);
		using Stream fileStream = fileSystem.File.Create(path);
		if (GlbWriter.TryWrite(sceneBuilder, fileStream, out string? errorMessage))
		{
			return true;
		}
		else
		{
			Logger.Error(LogCategory.Export, errorMessage);
			return false;
		}
	}

	private static void AddAssetToSceneBuilder(SceneBuilder sceneBuilder, INavMeshData asset)
	{
		MaterialBuilder material = new MaterialBuilder("DefaultMaterial");
		MeshBuilder<VertexPosition, VertexEmpty, VertexEmpty> meshBuilder = new();
		PrimitiveBuilder<MaterialBuilder, VertexPosition, VertexEmpty, VertexEmpty> primitiveBuilder = meshBuilder.UsePrimitive(material);

		Debug.Assert(asset.HeightMeshes != null);
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
