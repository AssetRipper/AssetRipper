using AssetRipper.Core.Project;
using AssetRipper.Core.Classes.Misc.Serializable;
using AssetRipper.Core.Classes.Utils.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;
using AssetRipper.Core.IO;

namespace AssetRipper.Core.Classes.Mesh
{
	public struct SubMesh : IAsset
	{
		/// <summary>Offset in index buffer</summary>
		public uint FirstByte { get; set; }
		public uint IndexCount { get; set; }
		public MeshTopology Topology { get; set; }
		public uint TriangleCount { get; set; }
		public uint BaseVertex { get; set; }
		/// <summary>Offset in Vertices</summary>
		public uint FirstVertex { get; set; }
		public uint VertexCount { get; set; }
		public AABB LocalAABB { get; set; }

		public uint IsTriStrip => (uint)Topology;

		public SubMesh(ObjectReader reader)
		{
			var version = reader.version;

			FirstByte = reader.ReadUInt32();
			IndexCount = reader.ReadUInt32();
			Topology = (MeshTopology)reader.ReadInt32();

			if (version[0] < 4) //4.0 down
			{
				TriangleCount = reader.ReadUInt32();
			}
			else TriangleCount = 0;

			if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)) //2017.3 and up
			{
				BaseVertex = reader.ReadUInt32();
			}
			else BaseVertex = 0;

			if (version[0] >= 3) //3.0 and up
			{
				FirstVertex = reader.ReadUInt32();
				VertexCount = reader.ReadUInt32();
				LocalAABB = new AABB(reader);
			}
			else
			{
				FirstVertex = 0;
				VertexCount = 0;
				LocalAABB = default;
			}
		}

		public static int ToSerializedVersion(UnityVersion version)
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
		public static bool HasTriangleCount(UnityVersion version) => version.IsLess(4);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasVertex(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasBaseVertex(UnityVersion version) => version.IsGreaterEqual(2017, 3);

		public void Read(AssetReader reader)
		{
			FirstByte = reader.ReadUInt32();
			IndexCount = reader.ReadUInt32();
			Topology = (MeshTopology)reader.ReadInt32();
			if (HasTriangleCount(reader.Version))
			{
				TriangleCount = reader.ReadUInt32();
			}
			if (HasBaseVertex(reader.Version))
			{
				BaseVertex = reader.ReadUInt32();
			}
			if (HasVertex(reader.Version))
			{
				FirstVertex = reader.ReadUInt32();
				VertexCount = reader.ReadUInt32();
				LocalAABB.Read(reader);
			}
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(FirstByte);
			writer.Write(IndexCount);
			writer.Write((int)Topology);
			if (HasTriangleCount(writer.Version))
			{
				writer.Write(TriangleCount);
			}
			if (HasBaseVertex(writer.Version))
			{
				writer.Write(BaseVertex);
			}
			if (HasVertex(writer.Version))
			{
				writer.Write(FirstVertex);
				writer.Write(VertexCount);
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

		public MeshTopology GetTopology(UnityVersion version)
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


		public const string FirstByteName = "firstByte";
		public const string IndexCountName = "indexCount";
		public const string IsTriStripName = "isTriStrip";
		public const string TopologyName = "topology";
		public const string TriangleCountName = "triangleCount";
		public const string BaseVertexName = "baseVertex";
		public const string FirstVertexName = "firstVertex";
		public const string VertexCountName = "vertexCount";
		public const string LocalAABBName = "localAABB";
	}
}
