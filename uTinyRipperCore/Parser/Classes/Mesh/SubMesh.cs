using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	public struct SubMesh : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool IsReadTriangleCount(Version version)
		{
			return version.IsLess(4);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadVertex(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadBaseVertex(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(4))
			{
				return 2;
			}
			return 1;
		}

		public MeshTopology GetTopology(Version version)
		{
			if (GetSerializedVersion(version) >= 2)
			{
				return Topology;
			}
			else
			{
				return Topology == MeshTopology.Triangles ? MeshTopology.Triangles : MeshTopology.Deprecated;
			}
		}

		public void Read(AssetReader reader)
		{
			FirstByte = (int)reader.ReadUInt32();
			IndexCount = (int)reader.ReadUInt32();
			Topology = (MeshTopology)reader.ReadInt32();

			if (IsReadTriangleCount(reader.Version))
			{
				TriangleCount = (int)reader.ReadUInt32();
			}
			if (IsReadBaseVertex(reader.Version))
			{
				BaseVertex = reader.ReadUInt32();
			}
			if (IsReadVertex(reader.Version))
			{
				FirstVertex = reader.ReadUInt32();
				VertexCount = reader.ReadUInt32();
				LocalAABB.Read(reader);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(FirstByteName, FirstByte);
			node.Add(IndexCountName, IndexCount);
			node.Add(TopologyName, (int)GetTopology(container.Version));
			node.Add(BaseVertexName, BaseVertex);
			node.Add(FirstVertexName, FirstVertex);
			node.Add(VertexCountName, VertexCount);
			node.Add(LocalAABBName, LocalAABB.ExportYAML(container));
			return node;
		}

		public int FirstByte { get; private set; }
		public int IndexCount { get; private set; }
		/// <summary>
		/// IsTriStrip previously
		/// </summary>
		public MeshTopology Topology { get; private set; }
		public int TriangleCount { get; private set; }
		public uint BaseVertex { get; private set; }
		public uint FirstVertex { get; private set; }
		public uint VertexCount { get; private set; }

		public const string FirstByteName = "firstByte";
		public const string IndexCountName = "indexCount";
		public const string TopologyName = "topology";
		public const string BaseVertexName = "baseVertex";
		public const string FirstVertexName = "firstVertex";
		public const string VertexCountName = "vertexCount";
		public const string LocalAABBName = "localAABB";

		public AABB LocalAABB;
	}
}
