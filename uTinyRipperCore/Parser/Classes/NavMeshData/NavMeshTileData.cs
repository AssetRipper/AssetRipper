using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Classes.NavMeshDatas
{
	public struct NavMeshTileData : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasHash(Version version) => version.IsGreaterEqual(5, 6);

		public void Read(AssetReader reader)
		{
			MeshData = reader.ReadByteArray();
			if (HasHash(reader.Version))
			{
				Hash.Read(reader);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(MeshDataName, MeshData.ExportYAML());
			node.Add(HashName, Hash.ExportYAML(container));
			return node;
		}

		public byte[] MeshData { get; set; }

		public const string MeshDataName = "m_MeshData";
		public const string HashName = "m_Hash";

		public Hash128 Hash;
	}
}
