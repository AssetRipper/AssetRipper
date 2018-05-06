using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Meshes
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

		public void Read(AssetStream stream)
		{
			Vertices.Read(stream);
			UV.Read(stream);
			if(IsReadBindPoses(stream.Version))
			{
				BindPoses.Read(stream);
			}
			Normals.Read(stream);
			Tangents.Read(stream);
			Weight.Read(stream);
			NormalSigns.Read(stream);
			TangentSigns.Read(stream);
			if(IsReadFloatColors(stream.Version))
			{
				FloatColors.Read(stream);
			}

			BoneIndices.Read(stream);
			Triangles.Read(stream);

			if (IsReadColors(stream.Version))
			{
				Colors.Read(stream);
			}
			if (IsReadUVInfo(stream.Version))
			{
				UVInfo = stream.ReadUInt32();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
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
