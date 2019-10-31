using System;
using System.Collections.Generic;
using uTinyRipper.Classes.SpriteAtlases;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Classes.Misc;
using uTinyRipper;

namespace uTinyRipper.Classes
{
	public sealed class SpriteAtlas : NamedObject
	{
		public SpriteAtlas(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// Not Release
		/// </summary>
		public static bool IsReadEditorData(TransferInstructionFlags flags)
		{
			return !flags.IsRelease();
		}
		/// <summary>
		/// Release
		/// </summary>
		public static bool IsReadRenderDataMap(TransferInstructionFlags flags)
		{
			return flags.IsRelease();
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadEditorData(reader.Flags))
			{
				EditorData = reader.ReadAsset<SpriteAtlasEditorData>();
				MasterAtlas.Read(reader);
			}
			m_packedSprites = reader.ReadAssetArray<PPtr<Sprite>>();
			m_packedSpriteNamesToIndex = reader.ReadStringArray();
			if (IsReadRenderDataMap(reader.Flags))
			{
				m_renderDataMap.Read(reader);
			}
			Tag = reader.ReadString();
			IsVariant = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			if (IsReadEditorData(context.Flags))
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(EditorData, EditorDataName))
				{
					yield return asset;
				}
				yield return context.FetchDependency(MasterAtlas, MasterAtlasName);
			}
			foreach (PPtr<Object> asset in context.FetchDependencies(PackedSprites, PackedSpritesName))
			{
				yield return asset;
			}
			foreach (PPtr<Object> asset in context.FetchDependencies(RenderDataMap.Values, RenderDataMapName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			if (IsReadEditorData(container.ExportFlags))
			{
				node.Add(EditorDataName, GetEditorData(container.Flags).ExportYAML(container));
				node.Add(MasterAtlasName, MasterAtlas.ExportYAML(container));
			}
			node.Add(PackedSpritesName, PackedSprites.ExportYAML(container));
			node.Add(PackedSpriteNamesToIndexName, PackedSpriteNamesToIndex.ExportYAML());
			if (IsReadRenderDataMap(container.ExportFlags))
			{
				node.Add(RenderDataMapName, RenderDataMap.ExportYAML(container));
			}
			node.Add(TagName, Tag);
			node.Add(IsVariantName, IsVariant);
			return node;
		}

		public SpriteAtlasEditorData GetEditorData(TransferInstructionFlags flags)
		{
			if (IsReadEditorData(flags))
			{
				return EditorData;
			}
			return new SpriteAtlasEditorData(PackedSprites);
		}

		public override string ExportExtension => "spriteatlas";

		public SpriteAtlasEditorData EditorData { get; private set; }
		public IReadOnlyList<PPtr<Sprite>> PackedSprites => m_packedSprites;
		public IReadOnlyList<string> PackedSpriteNamesToIndex => m_packedSpriteNamesToIndex;
		public IReadOnlyDictionary<Tuple<GUID, long>, SpriteAtlasData> RenderDataMap => m_renderDataMap;
		public string Tag { get; private set; }
		public bool IsVariant { get; private set; }

		public const string EditorDataName = "m_EditorData";
		public const string MasterAtlasName = "m_MasterAtlas";
		public const string PackedSpritesName = "m_PackedSprites";
		public const string PackedSpriteNamesToIndexName = "m_PackedSpriteNamesToIndex";
		public const string RenderDataMapName = "m_RenderDataMap";
		public const string TagName = "m_Tag";
		public const string IsVariantName = "m_IsVariant";

		public PPtr<SpriteAtlas> MasterAtlas;

		private readonly Dictionary<Tuple<GUID, long>, SpriteAtlasData> m_renderDataMap = new Dictionary<Tuple<GUID, long>, SpriteAtlasData>();

		private PPtr<Sprite>[] m_packedSprites;
		private string[] m_packedSpriteNamesToIndex;
	}
}
