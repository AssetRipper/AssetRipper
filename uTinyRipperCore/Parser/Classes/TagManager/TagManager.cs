using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.TagManagers;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class TagManager : GlobalGameManager
	{
		public TagManager(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadSortingLayers(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		private static bool IsReadStaticArray(Version version)
		{
			return version.IsLess(5);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(5))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_tags = reader.ReadStringArray();
			if(IsReadStaticArray(reader.Version))
			{
				m_layers = new string[32];
				for(int i = 0; i < m_layers.Length; i++)
				{
					m_layers[i] = reader.ReadString();
				}
			}
			else
			{
				m_layers = reader.ReadStringArray();
			}
			if(IsReadSortingLayers(reader.Version))
			{
				m_sortingLayers = reader.ReadAssetArray<SortingLayerEntry>();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add("tags", Tags.ExportYAML());
			node.Add("layers", Layers.ExportYAML());
			node.Add("m_SortingLayers", GetSortingLayers(container.Version).ExportYAML(container));
			return node;
		}

		private IReadOnlyList<SortingLayerEntry> GetSortingLayers(Version version)
		{
			return IsReadSortingLayers(version) ? SortingLayers : new SortingLayerEntry[0];
		}

		public IReadOnlyList<string> Tags => m_tags;
		public IReadOnlyList<string> Layers => m_layers;
		public IReadOnlyList<SortingLayerEntry> SortingLayers => m_sortingLayers;

		private string[] m_tags;
		private string[] m_layers;
		private SortingLayerEntry[] m_sortingLayers;
	}
}
