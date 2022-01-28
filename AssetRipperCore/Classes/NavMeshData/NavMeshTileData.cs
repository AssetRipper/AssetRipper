using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;

namespace AssetRipper.Core.Classes.NavMeshData
{
	public sealed class NavMeshTileData : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasHash(UnityVersion version) => version.IsGreaterEqual(5, 6);

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

		public Hash128 Hash = new();
	}
}
