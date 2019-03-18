using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	public struct CompressedMesh : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public static bool IsReadPlainColors(Version version)
		{
			return version.IsLess(3, 5);
		}
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool IsReadBindPoses(Version version)
		{
			return version.IsLess(5);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadFloatColors(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 3.5.0 to 5.0.0 exclusive
		/// </summary>
		public static bool IsReadColors(Version version)
		{
			return version.IsGreaterEqual(3, 5) && version.IsLess(5);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadUVInfo(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		public void Read(AssetReader reader)
		{
			Vertices.Read(reader);
			UV.Read(reader);
			if(IsReadBindPoses(reader.Version))
			{
				BindPoses.Read(reader);
			}
			Normals.Read(reader);
			Tangents.Read(reader);
			Weight.Read(reader);
			NormalSigns.Read(reader);
			TangentSigns.Read(reader);
			if(IsReadFloatColors(reader.Version))
			{
				FloatColors.Read(reader);
			}

			BoneIndices.Read(reader);
			Triangles.Read(reader);

			if (IsReadColors(reader.Version))
			{
				Colors.Read(reader);
			}
			if (IsReadUVInfo(reader.Version))
			{
				UVInfo = reader.ReadUInt32();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Vertices", Vertices.ExportYAML(container));
			node.Add("m_UV", UV.ExportYAML(container));
			node.Add("m_Normals", Normals.ExportYAML(container));
			node.Add("m_Tangents", Tangents.ExportYAML(container));
			node.Add("m_Weights", Weight.ExportYAML(container));
			node.Add("m_NormalSigns", NormalSigns.ExportYAML(container));
			node.Add("m_TangentSigns", TangentSigns.ExportYAML(container));
			node.Add("m_FloatColors", IsReadFloatColors(container.Version) ? FloatColors.ExportYAML(container) : default(PackedFloatVector).ExportYAML(container));
			node.Add("m_BoneIndices", BoneIndices.ExportYAML(container));
			node.Add("m_Triangles", Triangles.ExportYAML(container));
			node.Add("m_UVInfo", UVInfo);
			return node;
		}

		public uint UVInfo { get; private set; }

		public PackedFloatVector Vertices;
		public PackedFloatVector UV;
		public PackedFloatVector BindPoses;
		public PackedFloatVector Normals;
		public PackedFloatVector Tangents;
		public PackedIntVector Weight;
		public PackedIntVector NormalSigns;
		public PackedIntVector TangentSigns;
		public PackedFloatVector FloatColors;
		public PackedIntVector BoneIndices;
		public PackedIntVector Triangles;
		public PackedIntVector Colors;
	}
}
