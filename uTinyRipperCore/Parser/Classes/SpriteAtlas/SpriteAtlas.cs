using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.SpriteAtlases;
using uTinyRipper.Exporter.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class SpriteAtlas : NamedObject
	{
		public SpriteAtlas(AssetInfo assetInfo):
			base(assetInfo)
		{
		}
		
		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_packedSprites = reader.ReadArray<PPtr<Sprite>>();
			m_packedSpriteNamesToIndex = reader.ReadStringArray();
			m_renderDataMap.Read(reader);
			Tag = reader.ReadStringAligned();
			IsVariant = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			foreach(PPtr<Sprite> sprite in PackedSprites)
			{
				yield return sprite.FetchDependency(file, isLog, ToLogString, "PackedSprite");
			}
			foreach (SpriteAtlasData atlasData in RenderDataMap.Values)
			{
				foreach (Object @object in atlasData.FetchDependencies(file))
				{
					yield return @object;
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<PPtr<Sprite>> PackedSprites => m_packedSprites;
		public IReadOnlyList<string> PackedSpriteNamesToIndex => m_packedSpriteNamesToIndex;
		public IReadOnlyDictionary<Tuple<EngineGUID, long>, SpriteAtlasData> RenderDataMap => m_renderDataMap;
		public string Tag { get; private set; }
		public bool IsVariant { get; private set; }

		private readonly Dictionary<Tuple<EngineGUID, long>, SpriteAtlasData> m_renderDataMap = new Dictionary<Tuple<EngineGUID, long>, SpriteAtlasData>();

		private PPtr<Sprite>[] m_packedSprites;
		private string[] m_packedSpriteNamesToIndex;
	}
}
