using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	public struct SubMesh : IAsset
	{
		public static int ToSerializedVersion(Version version)
		{
			// IsTriStrip has been replaced by Topology
			if (version.IsGreaterEqual(4))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool HasTriangleCount(Version version) => version.IsLess(4);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasVertex(Version version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasBaseVertex(Version version) => version.IsGreaterEqual(2017, 3);
		
		public void Read(AssetReader reader)
		{
			FirstByte = (int)reader.ReadUInt32();
			IndexCount = (int)reader.ReadUInt32();
			Topology = (MeshTopology)reader.ReadInt32();
			if (HasTriangleCount(reader.Version))
			{
				TriangleCount = (int)reader.ReadUInt32();
			}
			if (HasBaseVertex(reader.Version))
			{
				BaseVertex = (int)reader.ReadUInt32();
			}
			if (HasVertex(reader.Version))
			{
				FirstVertex = (int)reader.ReadUInt32();
				VertexCount = (int)reader.ReadUInt32();
				LocalAABB.Read(reader);
			}
		}

		public void Write(AssetWriter writer)
		{
			writer.Write((uint)FirstByte);
			writer.Write((uint)IndexCount);
			writer.Write((int)Topology);
			if (HasTriangleCount(writer.Version))
			{
				writer.Write(TriangleCount);
			}
			if (HasBaseVertex(writer.Version))
			{
				writer.Write((uint)BaseVertex);
			}
			if (HasVertex(writer.Version))
			{
				writer.Write((uint)FirstVertex);
				writer.Write((uint)VertexCount);
				LocalAABB.Write(writer);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(FirstByteName, FirstByte);
			node.Add(IndexCountName, IndexCount);
			if (HasTriangleCount(container.ExportVersion))
			{
				node.Add(IsTriStripName, (int)Topology);
				node.Add(TriangleCountName, TriangleCount);
			}
			else
			{
				node.Add(TopologyName, (int)Topology);
			}
			if (HasBaseVertex(container.ExportVersion))
			{
				node.Add(BaseVertexName, BaseVertex);
			}
			if (HasVertex(container.ExportVersion))
			{
				node.Add(FirstVertexName, FirstVertex);
				node.Add(VertexCountName, VertexCount);
				node.Add(LocalAABBName, LocalAABB.ExportYAML(container));
			}
			return node;
		}

		public MeshTopology GetTopology(Version version)
		{
			if (ToSerializedVersion(version) >= 2)
			{
				return Topology;
			}
			else
			{
				return IsTriStrip == 0 ? MeshTopology.Triangles : MeshTopology.TriangleStrip;
			}
		}

		/// <summary>
		/// Offset in index buffer
		/// </summary>
		public int FirstByte { get; set; }
		public int IndexCount { get; set; }
		public uint IsTriStrip => (uint)Topology;
		public MeshTopology Topology { get; set; }
		public int TriangleCount { get; set; }
		public int BaseVertex { get; set; }
		/// <summary>
		/// Offset in Vertices
		/// </summary>
		public int FirstVertex { get; set; }
		public int VertexCount { get; set; }

		public const string FirstByteName = "firstByte";
		public const string IndexCountName = "indexCount";
		public const string IsTriStripName = "isTriStrip";
		public const string TopologyName = "topology";
		public const string TriangleCountName = "triangleCount";
		public const string BaseVertexName = "baseVertex";
		public const string FirstVertexName = "firstVertex";
		public const string VertexCountName = "vertexCount";
		public const string LocalAABBName = "localAABB";

		public AABB LocalAABB;
	}
}
