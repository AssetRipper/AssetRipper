using System.Collections.Generic;
using uTinyRipper.Classes.TagManagers;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper;

namespace uTinyRipper.Classes
{
	public sealed class TagManager : GlobalGameManager
	{
		public TagManager(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
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
		public static bool HasSortingLayers(Version version) => version.IsGreaterEqual(4, 3);

		/// <summary>
		/// 5.0.0 to 5.5.0 exclusive
		/// </summary>
		public static bool IsBrokenCustomTags(Version version) => version.IsGreaterEqual(5) && version.IsLess(5, 5);

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		private static bool IsStaticArray(Version version) => version.IsLess(5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Tags = reader.ReadStringArray();
			if (IsStaticArray(reader.Version))
			{
				Layers = new string[32];
				for(int i = 0; i < Layers.Length; i++)
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
			node.Add(TagsName, Tags.ExportYAML());
			node.Add(LayersName, Layers.ExportYAML());
			node.Add(SortingLayersName, GetSortingLayers(container.Version).ExportYAML(container));
			return node;
		}

		private IReadOnlyList<SortingLayerEntry> GetSortingLayers(Version version)
		{
			return HasSortingLayers(version) ? SortingLayers : System.Array.Empty<SortingLayerEntry>();
		}

		public string[] Tags { get; set; }
		public string[] Layers { get; set; }
		public SortingLayerEntry[] SortingLayers { get; set; }

		public const string UntaggedTag = "Untagged";
		public const string RespawnTag = "Respawn";
		public const string FinishTag = "Finish";
		public const string EditorOnlyTag = "EditorOnly";
		public const string MainCameraTag = "MainCamera";
		public const string PlayerTag = "Player";
		public const string GameControllerTag = "GameController";

		public const string TagsName = "tags";
		public const string LayersName = "layers";
		public const string SortingLayersName = "m_SortingLayers";
	}
}
