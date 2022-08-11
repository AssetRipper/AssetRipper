using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Math.Colors;
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
using System.Runtime.CompilerServices;

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
			return ExportFormat is MeshExportFormat.GlbPrimitive && asset is IMesh mesh && mesh.IsSet();
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
				out Vector2[]? _, //uv2
				out Vector2[]? _, //uv3
				out Vector2[]? _, //uv4
				out Vector2[]? _, //uv5
				out Vector2[]? _, //uv6
				out Vector2[]? _, //uv7
				out _, //bindpose
				out uint[] processedIndexBuffer);

			if (vertices is null)
			{
				throw new ArgumentException("Vertices can't be null.", nameof(mesh));
			}
			
			List<uint> indices = TriangleProcessor.ReadIndices(mesh, processedIndexBuffer);

			if (indices.Count % 3 != 0)
			{
				throw new ArgumentException("Index count must be a multiple of 3.", nameof(mesh));
			}

			bool hasNormals = normals != null && normals.Length == vertices.Length;
			bool hasTangents = tangents != null && tangents.Length == vertices.Length;
			bool hasUV0 = uv0 != null && uv0.Length == vertices.Length;
			bool hasUV1 = uv1 != null && uv1.Length == vertices.Length;
			//bool hasColors = colors != null && colors.Length == vertices.Length;
			//bool hasSkin;

			SceneBuilder sceneBuilder = new SceneBuilder();
			MaterialBuilder material = new MaterialBuilder("material");
			string name = mesh.NameString;
			MeshData meshData = new MeshData(vertices, normals, tangents, null, uv0, uv1, indices);
			GlbMeshType meshType = default;
			
			if (hasNormals)
			{
				if (hasTangents)
				{
					meshType |= GlbMeshType.PositionNormalTangent;
				}
				else
				{
					meshType |= GlbMeshType.PositionNormal;
				}
			}
			
			if (hasUV0)
			{
				if (hasUV1)
				{
					meshType |= GlbMeshType.Texture2;
				}
				else
				{
					meshType |= GlbMeshType.Texture1;
				}
			}

			switch (meshType)
			{
				case GlbMeshType.Position | GlbMeshType.Empty | GlbMeshType.Empty:
					AddMeshToScene<VertexPosition, VertexEmpty, VertexEmpty>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormal | GlbMeshType.Empty | GlbMeshType.Empty:
					AddMeshToScene<VertexPositionNormal, VertexEmpty, VertexEmpty>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Empty | GlbMeshType.Empty:
					AddMeshToScene<VertexPositionNormalTangent, VertexEmpty, VertexEmpty>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.Position | GlbMeshType.Texture1 | GlbMeshType.Empty:
					AddMeshToScene<VertexPosition, VertexTexture1, VertexEmpty>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormal | GlbMeshType.Texture1 | GlbMeshType.Empty:
					AddMeshToScene<VertexPositionNormal, VertexTexture1, VertexEmpty>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Texture1 | GlbMeshType.Empty:
					AddMeshToScene<VertexPositionNormalTangent, VertexTexture1, VertexEmpty>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.Position | GlbMeshType.Texture2 | GlbMeshType.Empty:
					AddMeshToScene<VertexPosition, VertexTexture2, VertexEmpty>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormal | GlbMeshType.Texture2 | GlbMeshType.Empty:
					AddMeshToScene<VertexPositionNormal, VertexTexture2, VertexEmpty>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Texture2 | GlbMeshType.Empty:
					AddMeshToScene<VertexPositionNormalTangent, VertexTexture2, VertexEmpty>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.Position | GlbMeshType.Color1Texture1 | GlbMeshType.Empty:
					AddMeshToScene<VertexPosition, VertexColor1Texture1, VertexEmpty>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormal | GlbMeshType.Color1Texture1 | GlbMeshType.Empty:
					AddMeshToScene<VertexPositionNormal, VertexColor1Texture1, VertexEmpty>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Color1Texture1 | GlbMeshType.Empty:
					AddMeshToScene<VertexPositionNormalTangent, VertexColor1Texture1, VertexEmpty>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.Position | GlbMeshType.Color1Texture2 | GlbMeshType.Empty:
					AddMeshToScene<VertexPosition, VertexColor1Texture2, VertexEmpty>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormal | GlbMeshType.Color1Texture2 | GlbMeshType.Empty:
					AddMeshToScene<VertexPositionNormal, VertexColor1Texture2, VertexEmpty>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Color1Texture2 | GlbMeshType.Empty:
					AddMeshToScene<VertexPositionNormalTangent, VertexColor1Texture2, VertexEmpty>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.Position | GlbMeshType.Empty | GlbMeshType.Joints4:
					AddMeshToScene<VertexPosition, VertexEmpty, VertexJoints4>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormal | GlbMeshType.Empty | GlbMeshType.Joints4:
					AddMeshToScene<VertexPositionNormal, VertexEmpty, VertexJoints4>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Empty | GlbMeshType.Joints4:
					AddMeshToScene<VertexPositionNormalTangent, VertexEmpty, VertexJoints4>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.Position | GlbMeshType.Texture1 | GlbMeshType.Joints4:
					AddMeshToScene<VertexPosition, VertexTexture1, VertexJoints4>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormal | GlbMeshType.Texture1 | GlbMeshType.Joints4:
					AddMeshToScene<VertexPositionNormal, VertexTexture1, VertexJoints4>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Texture1 | GlbMeshType.Joints4:
					AddMeshToScene<VertexPositionNormalTangent, VertexTexture1, VertexJoints4>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.Position | GlbMeshType.Texture2 | GlbMeshType.Joints4:
					AddMeshToScene<VertexPosition, VertexTexture2, VertexJoints4>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormal | GlbMeshType.Texture2 | GlbMeshType.Joints4:
					AddMeshToScene<VertexPositionNormal, VertexTexture2, VertexJoints4>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Texture2 | GlbMeshType.Joints4:
					AddMeshToScene<VertexPositionNormalTangent, VertexTexture2, VertexJoints4>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.Position | GlbMeshType.Color1Texture1 | GlbMeshType.Joints4:
					AddMeshToScene<VertexPosition, VertexColor1Texture1, VertexJoints4>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormal | GlbMeshType.Color1Texture1 | GlbMeshType.Joints4:
					AddMeshToScene<VertexPositionNormal, VertexColor1Texture1, VertexJoints4>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Color1Texture1 | GlbMeshType.Joints4:
					AddMeshToScene<VertexPositionNormalTangent, VertexColor1Texture1, VertexJoints4>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.Position | GlbMeshType.Color1Texture2 | GlbMeshType.Joints4:
					AddMeshToScene<VertexPosition, VertexColor1Texture2, VertexJoints4>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormal | GlbMeshType.Color1Texture2 | GlbMeshType.Joints4:
					AddMeshToScene<VertexPositionNormal, VertexColor1Texture2, VertexJoints4>(sceneBuilder, material, name, meshData);
					break;
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Color1Texture2 | GlbMeshType.Joints4:
					AddMeshToScene<VertexPositionNormalTangent, VertexColor1Texture2, VertexJoints4>(sceneBuilder, material, name, meshData);
					break;
			}

			SharpGLTF.Schema2.ModelRoot model = sceneBuilder.ToGltf2();

			//Write settings can be used in the write glb method if desired
			//SharpGLTF.Schema2.WriteSettings writeSettings = new();

			return model.WriteGLB().ToArray();
		}

		private static void AddMeshToScene<TvG, TvM, TvS>(SceneBuilder sceneBuilder, MaterialBuilder material, string name, MeshData meshData)
			where TvG : unmanaged, IVertexGeometry
			where TvM : unmanaged, IVertexMaterial
			where TvS : unmanaged, IVertexSkinning
		{
			MeshBuilder<TvG, TvM, TvS> meshBuilder = VertexBuilder<TvG, TvM, TvS>.CreateCompatibleMesh();
			PrimitiveBuilder<MaterialBuilder, TvG, TvM, TvS> primitiveBuilder = meshBuilder.UsePrimitive(material);
			
			for (int j = 0; j < meshData.Indices.Count; j += 3)
			{
				primitiveBuilder.AddTriangle(
					GetVertex<TvG, TvM, TvS>(meshData, meshData.Indices[j]),
					GetVertex<TvG, TvM, TvS>(meshData, meshData.Indices[j + 1]),
					GetVertex<TvG, TvM, TvS>(meshData, meshData.Indices[j + 2]));
			}
			
			NodeBuilder nodeBuilder = new NodeBuilder(name);
			//nodeBuilder.LocalMatrix = Matrix4x4.Identity; //Local transform can be changed if desired
			sceneBuilder.AddNode(nodeBuilder);
			sceneBuilder.AddRigidMesh(meshBuilder, nodeBuilder);
		}

		private static VertexBuilder<TvG, TvM, TvS> GetVertex<TvG, TvM, TvS>(MeshData meshData, uint index)
			where TvG : unmanaged, IVertexGeometry
			where TvM : unmanaged, IVertexMaterial
			where TvS : unmanaged, IVertexSkinning
		{
			TvG geometry;
			if (typeof(TvG) == typeof(VertexPosition))
			{
				geometry = Cast<VertexPosition, TvG>(new VertexPosition(meshData.TryGetVertexAtIndex(index)));
			}
			else if (typeof(TvG) == typeof(VertexPositionNormal))
			{
				geometry = Cast<VertexPositionNormal, TvG>(new VertexPositionNormal(meshData.TryGetVertexAtIndex(index), meshData.TryGetNormalAtIndex(index)));
			}
			else if (typeof(TvG) == typeof(VertexPositionNormalTangent))
			{
				geometry = Cast<VertexPositionNormalTangent, TvG>(new VertexPositionNormalTangent(meshData.TryGetVertexAtIndex(index), meshData.TryGetNormalAtIndex(index), meshData.TryGetTangentAtIndex(index)));
			}
			else
			{
				geometry = default;
			}

			TvM material;
			if (typeof(TvM) == typeof(VertexTexture1))
			{
				material = Cast<VertexTexture1,TvM>(new VertexTexture1(meshData.TryGetUV0AtIndex(index)));
			}
			else if (typeof(TvM) == typeof(VertexTexture2))
			{
				material = Cast<VertexTexture2, TvM>(new VertexTexture2(meshData.TryGetUV0AtIndex(index), meshData.TryGetUV1AtIndex(index)));
			}
			else if (typeof(TvM) == typeof(VertexColor1Texture1))
			{
				material = Cast<VertexColor1Texture1, TvM>(new VertexColor1Texture1(meshData.TryGetColorAtIndex(index), meshData.TryGetUV0AtIndex(index)));
			}
			else if (typeof(TvM) == typeof(VertexColor1Texture2))
			{
				material = Cast<VertexColor1Texture2, TvM>(new VertexColor1Texture2(meshData.TryGetColorAtIndex(index), meshData.TryGetUV0AtIndex(index), meshData.TryGetUV1AtIndex(index)));
			}
			else
			{
				material = default;
			}
			
			TvS skin;
			if (typeof(TvS) == typeof(VertexJoints4))
			{
				skin = default;
			}
			else
			{
				skin = default;
			}

			return new VertexBuilder<TvG, TvM, TvS>(geometry, material, skin);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static TTo Cast<TFrom, TTo>(TFrom value) where TFrom : unmanaged where TTo : unmanaged
		{
			//Protect against accidental misuse
			//Branches will get optimized away by JIT
			if (typeof(TFrom) == typeof(TTo))
			{
				return Unsafe.As<TFrom, TTo>(ref value);
			}
			else
			{
				return default;
			}
		}
		
		private readonly record struct MeshData(
			Vector3[] Vertices,
			Vector3[]? Normals,
			Vector4[]? Tangents,
			Vector4[]? Colors,
			Vector2[]? UV0,
			Vector2[]? UV1,
			IReadOnlyList<uint> Indices)
		{
			public Vector3 TryGetVertexAtIndex(uint index) => Vertices[index];
			public Vector3 TryGetNormalAtIndex(uint index) => TryGetAtIndex(Normals, index);
			public Vector4 TryGetTangentAtIndex(uint index) => TryGetAtIndex(Tangents, index);
			public Vector4 TryGetColorAtIndex(uint index) => TryGetAtIndex(Colors, index);
			public Vector2 TryGetUV0AtIndex(uint index) => TryGetAtIndex(UV0, index);
			public Vector2 TryGetUV1AtIndex(uint index) => TryGetAtIndex(UV1, index);

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			private static T TryGetAtIndex<T>(T[]? array, uint index) where T : struct
			{
				return array is null ? default : array[index];
			}
		}
	}
}
