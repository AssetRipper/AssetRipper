using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;

using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Core.Classes.NavMeshData
{
	public sealed class NavMeshTileData : IAssetReadable, IYamlExportable
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(MeshDataName, MeshData.ExportYaml());
			node.Add(HashName, Hash.ExportYaml(container));
			return node;
		}

		public byte[] MeshData { get; set; }

		public const string MeshDataName = "m_MeshData";
		public const string HashName = "m_Hash";

		public Hash128 Hash = new();
	}
}
