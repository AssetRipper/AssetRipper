using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.NavMeshDatas
{
	public struct NavMeshTileData : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadHash(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}

		public void Read(AssetReader reader)
		{
			m_meshData = reader.ReadByteArray();
			if (IsReadHash(reader.Version))
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

		public IReadOnlyList<byte> MeshData => m_meshData;

		public const string MeshDataName = "m_MeshData";
		public const string HashName = "m_Hash";

		public Hash128 Hash;

		private byte[] m_meshData;
	}
}
