using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Transforms;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AssetRipper.Export.Modules.Models
{
	internal static class GlbSubMeshBuilder
	{
		public static IMeshBuilder<MaterialBuilder> BuildSubMeshes(ArraySegment<ValueTuple<ISubMesh, MaterialBuilder>> subMeshes, MeshData meshData, Transformation transform, Transformation inverseTransform)
		{
			BuildSubMeshParameters parameters = new BuildSubMeshParameters(subMeshes, meshData, transform, inverseTransform);
			switch (meshData.MeshType)
			{
				case GlbMeshType.Position | GlbMeshType.Empty | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPosition, VertexEmpty, VertexEmpty>(parameters);
				case GlbMeshType.PositionNormal | GlbMeshType.Empty | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPositionNormal, VertexEmpty, VertexEmpty>(parameters);
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Empty | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPositionNormalTangent, VertexEmpty, VertexEmpty>(parameters);

				case GlbMeshType.Position | GlbMeshType.Texture1 | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPosition, VertexTexture1, VertexEmpty>(parameters);
				case GlbMeshType.PositionNormal | GlbMeshType.Texture1 | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPositionNormal, VertexTexture1, VertexEmpty>(parameters);
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Texture1 | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPositionNormalTangent, VertexTexture1, VertexEmpty>(parameters);

				case GlbMeshType.Position | GlbMeshType.Texture2 | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPosition, VertexTexture2, VertexEmpty>(parameters);
				case GlbMeshType.PositionNormal | GlbMeshType.Texture2 | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPositionNormal, VertexTexture2, VertexEmpty>(parameters);
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Texture2 | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPositionNormalTangent, VertexTexture2, VertexEmpty>(parameters);

				case GlbMeshType.Position | GlbMeshType.Color1 | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPosition, VertexColor1, VertexEmpty>(parameters);
				case GlbMeshType.PositionNormal | GlbMeshType.Color1 | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPositionNormal, VertexColor1, VertexEmpty>(parameters);
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Color1 | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPositionNormalTangent, VertexColor1, VertexEmpty>(parameters);

				case GlbMeshType.Position | GlbMeshType.Color1Texture1 | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPosition, VertexColor1Texture1, VertexEmpty>(parameters);
				case GlbMeshType.PositionNormal | GlbMeshType.Color1Texture1 | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPositionNormal, VertexColor1Texture1, VertexEmpty>(parameters);
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Color1Texture1 | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPositionNormalTangent, VertexColor1Texture1, VertexEmpty>(parameters);

				case GlbMeshType.Position | GlbMeshType.Color1Texture2 | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPosition, VertexColor1Texture2, VertexEmpty>(parameters);
				case GlbMeshType.PositionNormal | GlbMeshType.Color1Texture2 | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPositionNormal, VertexColor1Texture2, VertexEmpty>(parameters);
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Color1Texture2 | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPositionNormalTangent, VertexColor1Texture2, VertexEmpty>(parameters);

				case GlbMeshType.Position | GlbMeshType.TextureN | GlbMeshType.Empty:
				case GlbMeshType.Position | GlbMeshType.Color1TextureN | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPosition, VertexColor1Texture8, VertexEmpty>(parameters);
				case GlbMeshType.PositionNormal | GlbMeshType.TextureN | GlbMeshType.Empty:
				case GlbMeshType.PositionNormal | GlbMeshType.Color1TextureN | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPositionNormal, VertexColor1Texture8, VertexEmpty>(parameters);
				case GlbMeshType.PositionNormalTangent | GlbMeshType.TextureN | GlbMeshType.Empty:
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Color1TextureN | GlbMeshType.Empty:
					return BuildSubMeshes<VertexPositionNormalTangent, VertexColor1Texture8, VertexEmpty>(parameters);

				case GlbMeshType.Position | GlbMeshType.Empty | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPosition, VertexEmpty, VertexJoints4>(parameters);
				case GlbMeshType.PositionNormal | GlbMeshType.Empty | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPositionNormal, VertexEmpty, VertexJoints4>(parameters);
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Empty | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPositionNormalTangent, VertexEmpty, VertexJoints4>(parameters);

				case GlbMeshType.Position | GlbMeshType.Texture1 | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPosition, VertexTexture1, VertexJoints4>(parameters);
				case GlbMeshType.PositionNormal | GlbMeshType.Texture1 | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPositionNormal, VertexTexture1, VertexJoints4>(parameters);
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Texture1 | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPositionNormalTangent, VertexTexture1, VertexJoints4>(parameters);

				case GlbMeshType.Position | GlbMeshType.Texture2 | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPosition, VertexTexture2, VertexJoints4>(parameters);
				case GlbMeshType.PositionNormal | GlbMeshType.Texture2 | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPositionNormal, VertexTexture2, VertexJoints4>(parameters);
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Texture2 | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPositionNormalTangent, VertexTexture2, VertexJoints4>(parameters);

				case GlbMeshType.Position | GlbMeshType.Color1 | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPosition, VertexColor1, VertexJoints4>(parameters);
				case GlbMeshType.PositionNormal | GlbMeshType.Color1 | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPositionNormal, VertexColor1, VertexJoints4>(parameters);
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Color1 | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPositionNormalTangent, VertexColor1, VertexJoints4>(parameters);

				case GlbMeshType.Position | GlbMeshType.Color1Texture1 | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPosition, VertexColor1Texture1, VertexJoints4>(parameters);
				case GlbMeshType.PositionNormal | GlbMeshType.Color1Texture1 | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPositionNormal, VertexColor1Texture1, VertexJoints4>(parameters);
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Color1Texture1 | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPositionNormalTangent, VertexColor1Texture1, VertexJoints4>(parameters);

				case GlbMeshType.Position | GlbMeshType.Color1Texture2 | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPosition, VertexColor1Texture2, VertexJoints4>(parameters);
				case GlbMeshType.PositionNormal | GlbMeshType.Color1Texture2 | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPositionNormal, VertexColor1Texture2, VertexJoints4>(parameters);
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Color1Texture2 | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPositionNormalTangent, VertexColor1Texture2, VertexJoints4>(parameters);

				case GlbMeshType.Position | GlbMeshType.TextureN | GlbMeshType.Joints4:
				case GlbMeshType.Position | GlbMeshType.Color1TextureN | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPosition, VertexColor1Texture8, VertexJoints4>(parameters);
				case GlbMeshType.PositionNormal | GlbMeshType.TextureN | GlbMeshType.Joints4:
				case GlbMeshType.PositionNormal | GlbMeshType.Color1TextureN | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPositionNormal, VertexColor1Texture8, VertexJoints4>(parameters);
				case GlbMeshType.PositionNormalTangent | GlbMeshType.TextureN | GlbMeshType.Joints4:
				case GlbMeshType.PositionNormalTangent | GlbMeshType.Color1TextureN | GlbMeshType.Joints4:
					return BuildSubMeshes<VertexPositionNormalTangent, VertexColor1Texture8, VertexJoints4>(parameters);

				default:
					throw new ArgumentOutOfRangeException(nameof(meshData), meshData.MeshType, "Mesh type not supported.");
			}
		}

		private static MeshBuilder<TvG, TvM, TvS> BuildSubMeshes<TvG, TvM, TvS>(BuildSubMeshParameters parameters)
			where TvG : unmanaged, IVertexGeometry
			where TvM : unmanaged, IVertexMaterial
			where TvS : unmanaged, IVertexSkinning
		{
			Transformation positionTransform = parameters.Transform;
			Transformation tangentTransform = positionTransform.RemoveTranslation();
			Transformation normalTransform = parameters.InverseTransform.Transpose();
			MeshBuilder<TvG, TvM, TvS> meshBuilder = VertexBuilder<TvG, TvM, TvS>.CreateCompatibleMesh();

			for (int i = 0; i < parameters.SubMeshes.Count; i++)
			{
				(ISubMesh subMesh, MaterialBuilder material) = parameters.SubMeshes[i];
				BuildSubMesh(meshBuilder, subMesh, material, parameters.MeshData, positionTransform, tangentTransform, normalTransform);
			}

			return meshBuilder;
		}

		private static PrimitiveBuilder<MaterialBuilder, TvG, TvM, TvS> BuildSubMesh<TvG, TvM, TvS>(
			MeshBuilder<TvG, TvM, TvS> meshBuilder,
			ISubMesh subMesh,
			MaterialBuilder material,
			MeshData meshData,
			Transformation positionTransform,
			Transformation tangentTransform,
			Transformation normalTransform)
			where TvG : unmanaged, IVertexGeometry
			where TvM : unmanaged, IVertexMaterial
			where TvS : unmanaged, IVertexSkinning
		{
			uint firstIndex = meshData.Mesh.Is16BitIndices() ? subMesh.FirstByte / sizeof(ushort) : subMesh.FirstByte / sizeof(uint);

			uint indexCount = subMesh.IndexCount;
			MeshTopology topology = subMesh.GetTopology();

			int primitiveVertexCount = topology switch
			{
				MeshTopology.Lines or MeshTopology.LineStrip => 2,
				MeshTopology.Points => 1,
				_ => 3
			};
			PrimitiveBuilder<MaterialBuilder, TvG, TvM, TvS> primitiveBuilder = meshBuilder.UsePrimitive(material, primitiveVertexCount);

			//Vertex order is flipped in Gltf
			switch (topology)
			{
				case MeshTopology.Triangles:
					{
						for (int i = 0; i < indexCount; i += 3)
						{
							primitiveBuilder.AddTriangle(
								GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + i + 2], positionTransform, normalTransform, tangentTransform),
								GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + i + 1], positionTransform, normalTransform, tangentTransform),
								GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + i], positionTransform, normalTransform, tangentTransform));
						}
					}
					break;

				case MeshTopology.TriangleStrip:
					{
						// de-stripify :
						uint triIndex = 0;
						for (int i = 0; i < indexCount - 2; i++)
						{
							uint a = meshData.ProcessedIndexBuffer[firstIndex + i + 2];
							uint b = meshData.ProcessedIndexBuffer[firstIndex + i + 1];
							uint c = meshData.ProcessedIndexBuffer[firstIndex + i];

							// skip degenerates
							if (a == b || a == c || b == c)
							{
								continue;
							}

							// do the winding flip-flop of strips :
							if ((i & 1) == 1)
							{
								primitiveBuilder.AddTriangle(
									GetVertex<TvG, TvM, TvS>(meshData, b, positionTransform, normalTransform, tangentTransform),
									GetVertex<TvG, TvM, TvS>(meshData, a, positionTransform, normalTransform, tangentTransform),
									GetVertex<TvG, TvM, TvS>(meshData, c, positionTransform, normalTransform, tangentTransform));
							}
							else
							{
								primitiveBuilder.AddTriangle(
									GetVertex<TvG, TvM, TvS>(meshData, a, positionTransform, normalTransform, tangentTransform),
									GetVertex<TvG, TvM, TvS>(meshData, b, positionTransform, normalTransform, tangentTransform),
									GetVertex<TvG, TvM, TvS>(meshData, c, positionTransform, normalTransform, tangentTransform));
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
								GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + q + 3], positionTransform, normalTransform, tangentTransform),
								GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + q + 2], positionTransform, normalTransform, tangentTransform),
								GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + q + 1], positionTransform, normalTransform, tangentTransform),
								GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + q], positionTransform, normalTransform, tangentTransform));
						}
					}
					break;

				case MeshTopology.Lines:
					{
						for (int l = 0; l < indexCount; l += 2)
						{
							primitiveBuilder.AddLine(
								GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + l + 1], positionTransform, normalTransform, tangentTransform),
								GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + l], positionTransform, normalTransform, tangentTransform));
							//The vertex order is currently flipped because triangle and quad vertices need their order flipped, but is that still true for lines?
						}
					}
					break;

				case MeshTopology.LineStrip:
					//First two indices form a line, and then each new index connects a new vertex to the existing line strip.
					//https://docs.unity3d.com/ScriptReference/MeshTopology.LineStrip.html
					if (indexCount > 1)
					{
						VertexBuilder<TvG, TvM, TvS> previousVertex = GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex], positionTransform, normalTransform, tangentTransform);
						for (int l = 1; l < indexCount; l++)
						{
							VertexBuilder<TvG, TvM, TvS> currentVertex = GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + l], positionTransform, normalTransform, tangentTransform);
							primitiveBuilder.AddLine(currentVertex, previousVertex);
							//The vertex order is currently flipped because triangle and quad vertices need their order flipped, but is that still true for lines?
							previousVertex = currentVertex;
						}
					}
					break;

				case MeshTopology.Points:
					{
						for (int p = 0; p < indexCount; p++)
						{
							primitiveBuilder.AddPoint(GetVertex<TvG, TvM, TvS>(meshData, meshData.ProcessedIndexBuffer[firstIndex + p], positionTransform, normalTransform, tangentTransform));
						}
					}
					break;
			}

			return primitiveBuilder;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static VertexBuilder<TvG, TvM, TvS> GetVertex<TvG, TvM, TvS>(MeshData meshData, uint index,
			Transformation positionTransform, Transformation normalTransform, Transformation tangentTransform)
			where TvG : unmanaged, IVertexGeometry
			where TvM : unmanaged, IVertexMaterial
			where TvS : unmanaged, IVertexSkinning
		{
			TvG geometry = GetGeometry<TvG>(meshData, index, positionTransform, normalTransform, tangentTransform);
			TvM material = GetMaterial<TvM>(meshData, index);
			TvS skin = GetSkin<TvS>(meshData, index);
			return new VertexBuilder<TvG, TvM, TvS>(geometry, material, skin);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static TvG GetGeometry<TvG>(MeshData meshData, uint index, Transformation positionTransform, Transformation normalTransform, Transformation tangentTransform)
			where TvG : unmanaged, IVertexGeometry
		{
			Vector3 position = GlbCoordinateConversion.ToGltfVector3Convert(meshData.TryGetVertexAtIndex(index) * positionTransform);
			if (typeof(TvG) == typeof(VertexPosition))
			{
				return Cast<VertexPosition, TvG>(new VertexPosition(position));
			}
			else if (typeof(TvG) == typeof(VertexPositionNormal))
			{
				Vector3 normal = GlbCoordinateConversion.ToGltfVector3Convert(Vector3.Normalize(meshData.TryGetNormalAtIndex(index) * normalTransform));
				return Cast<VertexPositionNormal, TvG>(new VertexPositionNormal(position, normal));
			}
			else if (typeof(TvG) == typeof(VertexPositionNormalTangent))
			{
				Vector3 normal = GlbCoordinateConversion.ToGltfVector3Convert(Vector3.Normalize(meshData.TryGetNormalAtIndex(index) * normalTransform));
				Vector4 originalTangent = meshData.TryGetTangentAtIndex(index);
				Vector3 transformedTangent = Vector3.Normalize(originalTangent.AsVector3() * tangentTransform);
				Vector4 tangent = GlbCoordinateConversion.ToGltfTangentConvert(new Vector4(transformedTangent, originalTangent.W));
				return Cast<VertexPositionNormalTangent, TvG>(new VertexPositionNormalTangent(position, normal, tangent));
			}
			else
			{
				return default;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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
			else if (typeof(TvM) == typeof(VertexColor1Texture8))
			{
				VertexColor1Texture8 result = new()
				{
					Color = meshData.TryGetColorAtIndex(index).Vector,
					TexCoord0 = meshData.TryGetUV0AtIndex(index),
					TexCoord1 = meshData.TryGetUV1AtIndex(index),
					TexCoord2 = meshData.TryGetUV2AtIndex(index),
					TexCoord3 = meshData.TryGetUV3AtIndex(index),
					TexCoord4 = meshData.TryGetUV4AtIndex(index),
					TexCoord5 = meshData.TryGetUV5AtIndex(index),
					TexCoord6 = meshData.TryGetUV6AtIndex(index),
					TexCoord7 = meshData.TryGetUV7AtIndex(index),
				};
				return Cast<VertexColor1Texture8, TvM>(result);
			}
			else
			{
				return default;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static TvS GetSkin<TvS>(MeshData meshData, uint index) where TvS : unmanaged, IVertexSkinning
		{
			if (typeof(TvS) == typeof(VertexJoints4))
			{
				BoneWeight4 skin = meshData.TryGetSkinAtIndex(index);
				Vector4 indices = new Vector4(skin.Index0, skin.Index1, skin.Index2, skin.Index3);
				Vector4 weights = new Vector4(skin.Weight0, skin.Weight1, skin.Weight2, skin.Weight3);
				SparseWeight8 sparseWeight = SparseWeight8.Create(indices, weights);
				return Cast<VertexJoints4, TvS>(new(sparseWeight));
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

		private readonly record struct BuildSubMeshParameters(
			ArraySegment<ValueTuple<ISubMesh, MaterialBuilder>> SubMeshes,
			MeshData MeshData,
			Transformation Transform,
			Transformation InverseTransform)
		{
		}
	}
}
