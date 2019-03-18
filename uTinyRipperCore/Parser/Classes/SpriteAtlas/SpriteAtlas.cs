using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.SpriteAtlases;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

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

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			if (IsReadEditorData(file.Flags))
			{
				foreach (Object asset in EditorData.FetchDependencies(file))
				{
					yield return asset;
				}
				yield return MasterAtlas.FetchDependency(file, isLog, () => nameof(SpriteAtlas), nameof(MasterAtlas));
			}
			foreach (PPtr<Sprite> sprite in PackedSprites)
			{
				yield return sprite.FetchDependency(file, isLog, ToLogString, "PackedSprite");
			}
			foreach (SpriteAtlasData atlasData in RenderDataMap.Values)
			{
				foreach (Object asset in atlasData.FetchDependencies(file))
				{
					yield return asset;
				}
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
		public IReadOnlyDictionary<Tuple<EngineGUID, long>, SpriteAtlasData> RenderDataMap => m_renderDataMap;
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

		private readonly Dictionary<Tuple<EngineGUID, long>, SpriteAtlasData> m_renderDataMap = new Dictionary<Tuple<EngineGUID, long>, SpriteAtlasData>();

		private PPtr<Sprite>[] m_packedSprites;
		private string[] m_packedSpriteNamesToIndex;
	}
}
