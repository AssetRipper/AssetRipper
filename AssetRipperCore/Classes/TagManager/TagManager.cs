using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.TagManager
{
	public sealed class TagManager : GlobalGameManager, ITagManager
	{
		public TagManager(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(5))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasSortingLayers(UnityVersion version) => version.IsGreaterEqual(4, 3);

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		private static bool IsStaticArray(UnityVersion version) => version.IsLess(5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Tags = reader.ReadAssetArray<Utf8StringLegacy>();
			if (IsStaticArray(reader.Version))
			{
				Layers = new string[32];
				for (int i = 0; i < Layers.Length; i++)
				{
					Layers[i] = reader.ReadString();
				}
			}
			else
			{
				Layers = reader.ReadStringArray();
			}
			if (HasSortingLayers(reader.Version))
			{
				SortingLayers = reader.ReadAssetArray<SortingLayerEntry>();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(TagsName, Tags.ExportYAML(container));
			node.Add(LayersName, Layers.ExportYAML());
			node.Add(SortingLayersName, GetSortingLayers(container.Version).ExportYAML(container));
			return node;
		}

		private IReadOnlyList<SortingLayerEntry> GetSortingLayers(UnityVersion version)
		{
			return HasSortingLayers(version) ? SortingLayers : System.Array.Empty<SortingLayerEntry>();
		}

		public Utf8StringBase[] Tags { get; set; }
		public string[] Layers { get; set; }
		public SortingLayerEntry[] SortingLayers { get; set; }

		public const string TagsName = "tags";
		public const string LayersName = "layers";
		public const string SortingLayersName = "m_SortingLayers";
	}
}
