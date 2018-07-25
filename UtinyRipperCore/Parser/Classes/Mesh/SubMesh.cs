using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Meshes
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

		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsNewTopologyFormat(Version version)
		{
			return version.IsGreaterEqual(4);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			if (version.IsGreaterEqual(4))
			{
				return 2;
			}
			return 1;
		}

		private uint GetExportTopology(Version version)
		{
			if (IsNewTopologyFormat(version))
			{
				return (uint)Topology;
			}
			else
			{
				return Topology == MeshTopology.Triangles ? (uint)MeshTopology.Triangles : (uint)MeshTopology.Deprecated;
			}
		}

		public void Read(AssetStream stream)
		{
			FirstByte = (int)stream.ReadUInt32();
			IndexCount = (int)stream.ReadUInt32();
			Topology = (MeshTopology)stream.ReadUInt32();

			if(IsReadTriangleCount(stream.Version))
			{
				TriangleCount = (int)stream.ReadUInt32();
			}
			if(IsReadBaseVertex(stream.Version))
			{
				BaseVertex = stream.ReadUInt32();
			}
			if(IsReadVertex(stream.Version))
			{
				FirstVertex = stream.ReadUInt32();
				VertexCount = stream.ReadUInt32();
				LocalAABB.Read(stream);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("firstByte", FirstByte);
			node.Add("indexCount", IndexCount);
			node.Add("topology", GetExportTopology(container.Version));
			node.Add("firstVertex", FirstVertex);
			node.Add("vertexCount", VertexCount);
			node.Add("localAABB", LocalAABB.ExportYAML(container));
			return node;
		}

		public int FirstByte { get; private set; }
		// What is this in case of triangle strips?
		public int IndexCount { get; private set; }
		/// <summary>
		/// isTriStrip previously
		/// </summary>
		public MeshTopology Topology { get; private set; }
		public int TriangleCount { get; private set; }
		public uint BaseVertex { get; private set; }
		public uint FirstVertex { get; private set; }
		public uint VertexCount { get; private set; }

		public AABB LocalAABB;
	}
}
