using SevenZip;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
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

		public bool IsCRCMatch(uint crc)
		{
			return CRC.VerifyDigestUTF8(Name, crc);
		}

		public void Read(AssetReader reader)
		{
			if (IsReadName(reader.Version))
			{
				Name = reader.ReadString();
			}
			FirstVertex = reader.ReadUInt32();
			VertexCount = reader.ReadUInt32();
			if (IsReadAABB(reader.Version))
			{
				AabbMinDelta.Read(reader);
				AabbMaxDelta.Read(reader);
			}
			HasNormals = reader.ReadBoolean();
			HasTangents = reader.ReadBoolean();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(FirstVertexName, FirstVertex);
			node.Add(VertexCountName, VertexCount);
			node.Add(HasNormalsName, HasNormals);
			node.Add(HasTangentsName, HasTangents);
			return node;
		}

		public string Name { get; private set; }
		public uint FirstVertex { get; private set; }
		public uint VertexCount { get; private set; }
		public bool HasNormals { get; private set; }
		public bool HasTangents { get; private set; }

		public const string FirstVertexName = "firstVertex";
		public const string VertexCountName = "vertexCount";
		public const string HasNormalsName = "hasNormals";
		public const string HasTangentsName = "hasTangents";

		public Vector3f AabbMinDelta;
		public Vector3f AabbMaxDelta;
	}
}
