using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.Converters.Meshes;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	public struct CompressedMesh : IAsset
	{
		public CompressedMesh(Version version)
		{
			Vertices = new PackedFloatVector(true);
			UV = new PackedFloatVector(true);
			if (HasBindPoses(version))
			{
				BindPoses = new PackedFloatVector(true);
			}
			else
			{
				BindPoses = default;
			}
			Normals = new PackedFloatVector(true);
			Tangents = new PackedFloatVector(true);
			Weights = new PackedIntVector(true);
			NormalSigns = new PackedIntVector(true);
			TangentSigns = new PackedIntVector(true);
			if (HasFloatColors(version))
			{
				FloatColors = new PackedFloatVector(true);
			}
			else
			{
				FloatColors = default;
			}
			BoneIndices = new PackedIntVector(true);
			Triangles = new PackedIntVector(true);
			if (HasColors(version))
			{
				Colors = new PackedIntVector(true);
			}
			else
			{
				Colors = default;
			}
			UVInfo = default;
		}

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasBindPoses(Version version) => version.IsLess(5);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasFloatColors(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 3.5.0 to 5.0.0 exclusive
		/// </summary>
		public static bool HasColors(Version version) => version.IsGreaterEqual(3, 5) && version.IsLess(5);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasUVInfo(Version version) => version.IsGreaterEqual(5);

		public CompressedMesh Convert(IExportContainer container)
		{
			return CompressedMeshConverter.Convert(container, ref this);
		}

		public void Read(AssetReader reader)
		{
			Vertices.Read(reader);
			UV.Read(reader);
			if (HasBindPoses(reader.Version))
			{
				BindPoses.Read(reader);
			}

			Normals.Read(reader);
			Tangents.Read(reader);
			Weights.Read(reader);
			NormalSigns.Read(reader);
			TangentSigns.Read(reader);
			if (HasFloatColors(reader.Version))
			{
				FloatColors.Read(reader);
			}

			BoneIndices.Read(reader);
			Triangles.Read(reader);
			if (HasColors(reader.Version))
			{
				Colors.Read(reader);
			}
			if (HasUVInfo(reader.Version))
			{
				UVInfo = reader.ReadUInt32();
			}
		}

		public void Write(AssetWriter writer)
		{
			Vertices.Write(writer);
			UV.Write(writer);
			if (HasBindPoses(writer.Version))
			{
				BindPoses.Write(writer);
			}

			Normals.Write(writer);
			Tangents.Write(writer);
			Weights.Write(writer);
			NormalSigns.Write(writer);
			TangentSigns.Write(writer);
			if (HasFloatColors(writer.Version))
			{
				FloatColors.Write(writer);
			}

			BoneIndices.Write(writer);
			Triangles.Write(writer);
			if (HasColors(writer.Version))
			{
				Colors.Write(writer);
			}
			if (HasUVInfo(writer.Version))
			{
				writer.Write(UVInfo);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(VerticesName, Vertices.ExportYAML(container));
			node.Add(UVName, UV.ExportYAML(container));
			if (HasBindPoses(container.ExportVersion))
			{
				node.Add(BindPosesName, BindPoses.ExportYAML(container));
			}

			node.Add(NormalsName, Normals.ExportYAML(container));
			node.Add(TangentsName, Tangents.ExportYAML(container));
			node.Add(WeightsName, Weights.ExportYAML(container));
			node.Add(NormalSignsName, NormalSigns.ExportYAML(container));
			node.Add(TangentSignsName, TangentSigns.ExportYAML(container));
			if (HasFloatColors(container.ExportVersion))
			{
				node.Add(FloatColorsName, FloatColors.ExportYAML(container));
			}

			node.Add(BoneIndicesName, BoneIndices.ExportYAML(container));
			node.Add(TrianglesName, Triangles.ExportYAML(container));
			if (HasColors(container.ExportVersion))
			{
				node.Add(ColorsName, Colors.ExportYAML(container));
			}
			if (HasUVInfo(container.ExportVersion))
			{
				node.Add(UVInfoName, UVInfo);
			}
			return node;
		}

		public uint UVInfo { get; set; }

		public const string VerticesName = "m_Vertices";
		public const string UVName = "m_UV";
		public const string BindPosesName = "m_BindPoses";
		public const string NormalsName = "m_Normals";
		public const string TangentsName = "m_Tangents";
		public const string WeightsName = "m_Weights";
		public const string NormalSignsName = "m_NormalSigns";
		public const string TangentSignsName = "m_TangentSigns";
		public const string FloatColorsName = "m_FloatColors";
		public const string BoneIndicesName = "m_BoneIndices";
		public const string TrianglesName = "m_Triangles";
		public const string ColorsName = "m_Colors";
		public const string UVInfoName = "m_UVInfo";

		public PackedFloatVector Vertices;
		public PackedFloatVector UV;
		public PackedFloatVector BindPoses;
		public PackedFloatVector Normals;
		public PackedFloatVector Tangents;
		public PackedIntVector Weights;
		public PackedIntVector NormalSigns;
		public PackedIntVector TangentSigns;
		public PackedFloatVector FloatColors;
		public PackedIntVector BoneIndices;
		public PackedIntVector Triangles;
		public PackedIntVector Colors;
	}
}
