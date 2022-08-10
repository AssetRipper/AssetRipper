using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Library.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace AssetRipper.Library.Exporters.Meshes
{
	public sealed class GlbMeshExporter : BinaryAssetExporter
	{
		private MeshExportFormat ExportFormat { get; set; }
		public GlbMeshExporter(LibraryConfiguration configuration) : base() => ExportFormat = configuration.MeshExportFormat;

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new GlbMeshExportCollection(this, asset);
		}

		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IMesh mesh && IsHandle(mesh);
		}

		public bool IsHandle(IMesh mesh)
		{
			return ExportFormat == MeshExportFormat.GlbPrimitive; //&&
																  //mesh.Vertices != null &&
																  //mesh.Vertices.Length > 0 &&
																  //mesh.Indices != null &&
																  //mesh.Indices.Count > 0 &&
																  //mesh.Indices.Count % 3 == 0;
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			byte[] data = ExportBinary((IMesh)asset);
			if (data == null || data.Length == 0)
			{
				return false;
			}

			using FileStream fileStream = File.Create(path);
			fileStream.Write(data);
			return true;
		}

		private byte[] ExportBinary(IMesh mesh)
		{
			mesh.ReadData(
				out Vector3[]? vertices,
				out Vector3[]? normals,
				out Vector4[]? tangents,
				out ColorRGBA32[]? colors,
				out _, //skin
				out Vector2[]? uv0,
				out Vector2[]? uv1,
				out Vector2[]? uv2,
				out Vector2[]? uv3,
				out Vector2[]? uv4,
				out Vector2[]? uv5,
				out Vector2[]? uv6,
				out Vector2[]? uv7,
				out _, //bindpose
				out uint[] processedIndexBuffer);

			if (vertices is null)
			{
				throw new ArgumentException("Vertices can't be null", nameof(mesh));
			}

			List<uint> indices = TriangleProcessor.ReadIndices(mesh, processedIndexBuffer);

			bool hasNormals = normals != null && normals.Length == vertices.Length;
			bool hasTangents = tangents != null && tangents.Length == vertices.Length;
			bool hasUV0 = uv0 != null && uv0.Length == vertices.Length;
			bool hasUV1 = uv1 != null && uv1.Length == vertices.Length;
			//bool hasColors = colors != null && colors.Length == vertices.Length;

			SceneBuilder sceneBuilder = new SceneBuilder();
			MaterialBuilder material = new MaterialBuilder("material");

			if (hasTangents)
			{
				if (hasUV1)
				{
					AddMeshToScene<VertexPositionNormalTangent, VertexTexture2>(sceneBuilder, mesh.NameString, vertices, normals, tangents, uv0, uv1, indices);
				}
				else if (hasUV0)
				{
					AddMeshToScene<VertexPositionNormalTangent, VertexTexture1>(sceneBuilder, mesh.NameString, vertices, normals, tangents, uv0, uv1, indices);
				}
				else
				{
					AddMeshToScene<VertexPositionNormalTangent, VertexEmpty>(sceneBuilder, mesh.NameString, vertices, normals, tangents, uv0, uv1, indices);
				}
			}
			else if (hasNormals)
			{
				if (hasUV1)
				{
					AddMeshToScene<VertexPositionNormal, VertexTexture2>(sceneBuilder, mesh.NameString, vertices, normals, tangents, uv0, uv1, indices);
				}
				else if (hasUV0)
				{
					AddMeshToScene<VertexPositionNormal, VertexTexture1>(sceneBuilder, mesh.NameString, vertices, normals, tangents, uv0, uv1, indices);
				}
				else
				{
					AddMeshToScene<VertexPositionNormal, VertexEmpty>(sceneBuilder, mesh.NameString, vertices, normals, tangents, uv0, uv1, indices);
				}
			}
			else
			{
				if (hasUV1)
				{
					AddMeshToScene<VertexPosition, VertexTexture2>(sceneBuilder, mesh.NameString, vertices, normals, tangents, uv0, uv1, indices);
				}
				else if (hasUV0)
				{
					AddMeshToScene<VertexPosition, VertexTexture1>(sceneBuilder, mesh.NameString, vertices, normals, tangents, uv0, uv1, indices);
				}
				else
				{
					AddMeshToScene<VertexPosition, VertexEmpty>(sceneBuilder, mesh.NameString, vertices, normals, tangents, uv0, uv1, indices);
				}
			}

			SharpGLTF.Schema2.ModelRoot? model = sceneBuilder.ToGltf2();

			//Write settings can be used in the write glb method if desired
			//var writeSettings = new SharpGLTF.Schema2.WriteSettings();

			return model.WriteGLB().ToArray();
		}

		private static void AddMeshToScene<TvG, TvM>(SceneBuilder sceneBuilder, string name,
			Vector3[]? vertices,
			Vector3[]? normals,
			Vector4[]? tangents,
			Vector2[]? uv0,
			Vector2[]? uv1,
			List<uint> indices)
			where TvG : struct, IVertexGeometry
			where TvM : struct, IVertexMaterial
		{
			MaterialBuilder material = new MaterialBuilder("material");
			MeshBuilder<TvG, TvM, VertexEmpty>? meshBuilder = VertexBuilder<TvG, TvM, VertexEmpty>.CreateCompatibleMesh(name);
			PrimitiveBuilder<MaterialBuilder, TvG, TvM, VertexEmpty>? primitiveBuilder = meshBuilder.UsePrimitive(material);
			for (int j = 0; j < indices.Count; j += 3)
			{
				primitiveBuilder.AddTriangle(
					GetVertex<TvG, TvM>(indices[j], vertices, normals, tangents, uv0, uv1),
					GetVertex<TvG, TvM>(indices[j + 1], vertices, normals, tangents, uv0, uv1),
					GetVertex<TvG, TvM>(indices[j + 2], vertices, normals, tangents, uv0, uv1));
			}
			sceneBuilder.AddRigidMesh(meshBuilder, System.Numerics.Matrix4x4.Identity);
		}

		private static VertexBuilder<TvG, TvM, VertexEmpty> GetVertex<TvG, TvM>(
			uint index,
			Vector3[]? vertices,
			Vector3[]? normals,
			Vector4[]? tangents,
			Vector2[]? uv0,
			Vector2[]? uv1)
			where TvG : struct, IVertexGeometry
			where TvM : struct, IVertexMaterial
		{
			return GetVertex<TvG, TvM>(TryGetAtIndex(vertices, index), TryGetAtIndex(normals, index), TryGetAtIndex(tangents, index), TryGetAtIndex(uv0, index), TryGetAtIndex(uv1, index));
		}

		private static VertexBuilder<TvG, TvM, VertexEmpty> GetVertex<TvG, TvM>(Vector3 vertex, Vector3 normal, Vector4 tangent, Vector2 uv0, Vector2 uv1)
			where TvG : struct, IVertexGeometry
			where TvM : struct, IVertexMaterial
		{
			IVertexGeometry geometry;
			if (typeof(TvG) == typeof(VertexPosition))
			{
				geometry = new VertexPosition(vertex);
			}
			else if (typeof(TvG) == typeof(VertexPositionNormal))
			{
				geometry = new VertexPositionNormal(vertex, normal);
			}
			else if (typeof(TvG) == typeof(VertexPositionNormalTangent))
			{
				geometry = new VertexPositionNormalTangent(vertex, normal, tangent);
			}
			else
			{
				geometry = default(TvG);
			}

			IVertexMaterial material;
			if (typeof(TvM) == typeof(VertexTexture1))
			{
				material = new VertexTexture1(uv0);
			}
			else if (typeof(TvM) == typeof(VertexTexture2))
			{
				material = new VertexTexture2(uv0, uv1);
			}
			else
			{
				material = default(TvM);
			}

			return new VertexBuilder<TvG, TvM, VertexEmpty>((TvG)geometry, (TvM)material);
		}

		private static T TryGetAtIndex<T>(T[]? array, uint index) where T : struct
		{
			return array is null ? default : array[index];
		}
	}
}
