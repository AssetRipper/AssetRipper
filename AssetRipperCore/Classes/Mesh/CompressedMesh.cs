using AssetRipper.Core.Converters.Mesh;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.PackedBitVectors;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Mesh
{
	public sealed class CompressedMesh : IAsset
	{
		public PackedFloatVector Vertices { get; set; } = new PackedFloatVector();
		public PackedFloatVector UV { get; set; } = new PackedFloatVector();
		public PackedFloatVector BindPoses { get; set; } = new PackedFloatVector();
		public PackedFloatVector Normals { get; set; } = new PackedFloatVector();
		public PackedFloatVector Tangents { get; set; } = new PackedFloatVector();
		public PackedIntVector Weights { get; set; } = new PackedIntVector();
		public PackedIntVector NormalSigns { get; set; } = new PackedIntVector();
		public PackedIntVector TangentSigns { get; set; } = new PackedIntVector();
		public PackedFloatVector FloatColors { get; set; } = new PackedFloatVector();
		public PackedIntVector BoneIndices { get; set; } = new PackedIntVector();
		public PackedIntVector Triangles { get; set; } = new PackedIntVector();
		public PackedIntVector Colors { get; set; } = new PackedIntVector();
		public uint UVInfo { get; set; }

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasBindPoses(UnityVersion version) => version.IsLess(5);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasFloatColors(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 3.5.0 to 5.0.0 exclusive
		/// </summary>
		public static bool HasColors(UnityVersion version) => version.IsGreaterEqual(3, 5) && version.IsLess(5);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasUVInfo(UnityVersion version) => version.IsGreaterEqual(5);

		public CompressedMesh Convert(IExportContainer container)
		{
			return CompressedMeshConverter.Convert(container, this);
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
	}
}
