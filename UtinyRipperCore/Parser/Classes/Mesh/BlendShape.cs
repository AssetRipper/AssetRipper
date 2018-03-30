using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Meshes
{
	/// <summary>
	/// MeshBlendShape previously
	/// </summary>
	public struct BlendShape : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// Less than 4.3.0
		/// </summary>
		public static bool IsReadName(Version version)
		{
			return version.IsLess(4, 3);
		}
		/// <summary>
		/// Less than 4.3
		/// </summary>
		public static bool IsReadAABB(Version version)
		{
			return version.IsLess(4, 3);
		}

		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}

		public void Read(AssetStream stream)
		{
			if (IsReadName(stream.Version))
			{
				Name = stream.ReadString();
			}
			FirstVertex = stream.ReadUInt32();
			VertexCount = stream.ReadUInt32();
			if (IsReadAABB(stream.Version))
			{
				AabbMinDelta.Read(stream);
				AabbMaxDelta.Read(stream);
			}
			HasNormals = stream.ReadBoolean();
			HasTangents = stream.ReadBoolean();
			if (IsAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("firstVertex", FirstVertex);
			node.Add("vertexCount", VertexCount);
			node.Add("hasNormals", HasNormals);
			node.Add("hasTangents", HasTangents);
			return node;
		}

		public string Name { get; private set; }
		public uint FirstVertex { get; private set; }
		public uint VertexCount { get; private set; }
		public bool HasNormals { get; private set; }
		public bool HasTangents { get; private set; }

		public Vector3f AabbMinDelta;
		public Vector3f AabbMaxDelta;
	}
}
