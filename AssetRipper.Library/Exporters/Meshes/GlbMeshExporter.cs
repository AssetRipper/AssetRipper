using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Library.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
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

		private static byte[] ExportBinary(IMesh mesh)
		{
			mesh.ReadData(
				out Vector3[]? vertices,
				out Vector3[]? normals,
				out Vector4[]? tangents,
				out ColorFloat[]? colors,
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

			SceneBuilder sceneBuilder = new SceneBuilder();
			MaterialBuilder material = new MaterialBuilder("material");
			string name = mesh.NameString;
			MeshData meshData = new MeshData(vertices, normals, tangents, colors, uv0, uv1, processedIndexBuffer, mesh);
			GlbMeshType meshType = GetMeshType(vertices, normals, tangents, colors, uv0, uv1);

			AddMeshToScene(sceneBuilder, material, name, meshData, meshType);

			SharpGLTF.Schema2.ModelRoot model = sceneBuilder.ToGltf2();

			//Write settings can be used in the write glb method if desired
			//SharpGLTF.Schema2.WriteSettings writeSettings = new();

			return model.WriteGLB().ToArray();
		}

		private static GlbMeshType GetMeshType(Vector3[] vertices, Vector3[]? normals, Vector4[]? tangents, ColorFloat[]? colors, Vector2[]? uv0, Vector2[]? uv1)
		{
			GlbMeshType meshType = default;

			if (normals != null && normals.Length == vertices.Length)
			{
				if (tangents != null && tangents.Length == vertices.Length)
				{
					meshType |= GlbMeshType.PositionNormalTangent;
				}
				else
				{
					meshType |= GlbMeshType.PositionNormal;
				}
			}

			if (uv0 != null && uv0.Length == vertices.Length)
			{
				if (uv1 != null && uv1.Length == vertices.Length)
				{
					meshType |= GlbMeshType.Texture2;
				}
				else
				{
					meshType |= GlbMeshType.Texture1;
				}
			}

			if (colors != null && colors.Length == vertices.Length)
			{
				meshType |= GlbMeshType.Color1;
			}

			return meshType;
		}

		private static void AddMeshToScene(SceneBuilder sceneBuilder, MaterialBuilder material, string name, MeshData meshData, GlbMeshType meshType)
		{
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
		}

		private static void AddMeshToScene<TvG, TvM, TvS>(SceneBuilder sceneBuilder, MaterialBuilder material, string name, MeshData meshData)
			where TvG : unmanaged, IVertexGeometry
			where TvM : unmanaged, IVertexMaterial
			where TvS : unmanaged, IVertexSkinning
		{
			NodeBuilder rootNodeForMesh = new NodeBuilder(name);
			//nodeBuilder.LocalMatrix = Matrix4x4.Identity; //Local transform can be changed if desired
			sceneBuilder.AddNode(rootNodeForMesh);

			for (int submeshIndex = 0; submeshIndex < meshData.Mesh.SubMeshes_C43.Count; submeshIndex++)
			{
				ISubMesh subMesh = meshData.Mesh.SubMeshes_C43[submeshIndex];
				uint firstIndex = meshData.Mesh.Is16BitIndices() ? subMesh.FirstByte / 2 : subMesh.FirstByte / 4;

				uint indexCount = subMesh.IndexCount;
				MeshTopology topology = subMesh.GetTopology();
				if (topology != MeshTopology.Triangles && meshData.Mesh.SerializedFile.Version.IsLess(4))
				{
					topology = MeshTopology.TriangleStrip;
				}
				
				MeshBuilder<TvG, TvM, TvS> meshBuilder = VertexBuilder<TvG, TvM, TvS>.CreateCompatibleMesh();
				int primitiveVertexCount = topology switch
				{
					MeshTopology.Lines or MeshTopology.LineStrip => 2,
					MeshTopology.Points => 1,
					_ => 3
				};
				PrimitiveBuilder<MaterialBuilder, TvG, TvM, TvS> primitiveBuilder = meshBuilder.UsePrimitive(material, primitiveVertexCount);

				switch (topology)
				{
					case MeshTopology.Triangles:
						{
							for (int i = 0; i < indexCount; i += 3)
							{
								primitiveBuilder.AddTriangle(
									GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + i]),
									GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + i + 1]),
									GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + i + 2]));
							}
						}
						break;

					case MeshTopology.TriangleStrip:
						{
							// de-stripify :
							uint triIndex = 0;
							for (int i = 0; i < indexCount - 2; i++)
							{
								uint a = meshData.ProcessedIndexBuffer[firstIndex + i];
								uint b = meshData.ProcessedIndexBuffer[firstIndex + i + 1];
								uint c = meshData.ProcessedIndexBuffer[firstIndex + i + 2];

								// skip degenerates
								if (a == b || a == c || b == c)
								{
									continue;
								}

								// do the winding flip-flop of strips :
								if ((i & 1) == 1)
								{
									primitiveBuilder.AddTriangle(
										GetVertex<TvG, TvM, TvS>(meshData, b),
										GetVertex<TvG, TvM, TvS>(meshData, a),
										GetVertex<TvG, TvM, TvS>(meshData, c));
								}
								else
								{
									primitiveBuilder.AddTriangle(
										GetVertex<TvG, TvM, TvS>(meshData, a),
										GetVertex<TvG, TvM, TvS>(meshData, b),
										GetVertex<TvG, TvM, TvS>(meshData, c));
								}
								triIndex += 3;
							}
						}
						break;

					case MeshTopology.Quads:
						{
							for (int q = 0; q < indexCount; q += 4)
							{
								primitiveBuilder.AddQuadrangle(
									GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + q]),
									GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + q + 1]),
									GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + q + 2]),
									GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + q + 3]));
							}
						}
						break;

					case MeshTopology.Lines:
						{
							for (int l = 0; l < indexCount; l += 2)
							{
								primitiveBuilder.AddLine(
									GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + l]),
									GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + l + 1]));
							}
						}
						break;
					case MeshTopology.LineStrip:
						Logger.Warning(LogCategory.Export, "LineStrip is not supported for GLB mesh export.");
						break;
					case MeshTopology.Points:
						{
							for (int p = 0; p < indexCount; p++)
							{
								primitiveBuilder.AddPoint(GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + p]));
							}
						}
						break;
				}

				NodeBuilder subMeshNode = rootNodeForMesh.CreateNode($"SubMesh_{submeshIndex}");
				sceneBuilder.AddRigidMesh(meshBuilder, subMeshNode);
			}
		}

		private static VertexBuilder<TvG, TvM, TvS> GetVertex<TvG, TvM, TvS>(MeshData meshData, uint index)
			where TvG : unmanaged, IVertexGeometry
			where TvM : unmanaged, IVertexMaterial
			where TvS : unmanaged, IVertexSkinning
		{
			TvG geometry = GetGeometry<TvG>(meshData, index);
			TvM material = GetMaterial<TvM>(meshData, index);
			TvS skin = GetSkin<TvS>(meshData, index);
			return new VertexBuilder<TvG, TvM, TvS>(geometry, material, skin);
		}

		private static TvG GetGeometry<TvG>(MeshData meshData, uint index) where TvG : unmanaged, IVertexGeometry
		{
			if (typeof(TvG) == typeof(VertexPosition))
			{
				return Cast<VertexPosition, TvG>(new VertexPosition(meshData.TryGetVertexAtIndex(index)));
			}
			else if (typeof(TvG) == typeof(VertexPositionNormal))
			{
				return Cast<VertexPositionNormal, TvG>(new VertexPositionNormal(meshData.TryGetVertexAtIndex(index), meshData.TryGetNormalAtIndex(index)));
			}
			else if (typeof(TvG) == typeof(VertexPositionNormalTangent))
			{
				return Cast<VertexPositionNormalTangent, TvG>(new VertexPositionNormalTangent(meshData.TryGetVertexAtIndex(index), meshData.TryGetNormalAtIndex(index), meshData.TryGetTangentAtIndex(index)));
			}
			else
			{
				return default;
			}
		}

		private static TvM GetMaterial<TvM>(MeshData meshData, uint index) where TvM : unmanaged, IVertexMaterial
		{
			if (typeof(TvM) == typeof(VertexTexture1))
			{
				return Cast<VertexTexture1, TvM>(new VertexTexture1(meshData.TryGetUV0AtIndex(index)));
			}
			else if (typeof(TvM) == typeof(VertexTexture2))
			{
				return Cast<VertexTexture2, TvM>(new VertexTexture2(meshData.TryGetUV0AtIndex(index), meshData.TryGetUV1AtIndex(index)));
			}
			else if (typeof(TvM) == typeof(VertexColor1Texture1))
			{
				return Cast<VertexColor1Texture1, TvM>(new VertexColor1Texture1(meshData.TryGetColorAtIndex(index).Vector, meshData.TryGetUV0AtIndex(index)));
			}
			else if (typeof(TvM) == typeof(VertexColor1Texture2))
			{
				return Cast<VertexColor1Texture2, TvM>(new VertexColor1Texture2(meshData.TryGetColorAtIndex(index).Vector, meshData.TryGetUV0AtIndex(index), meshData.TryGetUV1AtIndex(index)));
			}
			else
			{
				return default;
			}
		}

		private static TvS GetSkin<TvS>(MeshData meshData, uint index) where TvS : unmanaged, IVertexSkinning
		{
			if (typeof(TvS) == typeof(VertexJoints4))
			{
				return default;
			}
			else
			{
				return default;
			}
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
#if DEBUG
				throw new InvalidCastException();
#else
				return default;
#endif
			}
		}
		
		private readonly record struct MeshData(
			Vector3[] Vertices,
			Vector3[]? Normals,
			Vector4[]? Tangents,
			ColorFloat[]? Colors,
			Vector2[]? UV0,
			Vector2[]? UV1,
			uint[] ProcessedIndexBuffer,
			IMesh Mesh)
		{
			public Vector3 TryGetVertexAtIndex(uint index) => Vertices[index];
			public Vector3 TryGetNormalAtIndex(uint index) => TryGetAtIndex(Normals, index);
			public Vector4 TryGetTangentAtIndex(uint index)
			{
				Vector4 v = TryGetAtIndex(Tangents, index);
				//Unity documentation claims W should always be 1 or -1, but it's not always the case.
				return v.W switch
				{
					-1 or 1 => v,
					< 0 => new Vector4(v.X, v.Y, v.Z, -1),
					_ => new Vector4(v.X, v.Y, v.Z, 1)
				};
			}
			public ColorFloat TryGetColorAtIndex(uint index) => TryGetAtIndex(Colors, index);
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
