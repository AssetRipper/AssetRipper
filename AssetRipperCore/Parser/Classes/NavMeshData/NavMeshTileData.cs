using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes.Misc;
using AssetRipper.Parser.Files.File.Version;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.YAML;
using AssetRipper.YAML.Extensions;

namespace AssetRipper.Parser.Classes.NavMeshData
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
