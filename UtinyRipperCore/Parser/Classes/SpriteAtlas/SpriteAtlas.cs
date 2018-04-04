using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.SpriteAtlases;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class SpriteAtlas : NamedObject
	{
		public SpriteAtlas(AssetInfo assetInfo):
			base(assetInfo)
		{
		}
		
		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			m_packedSprites = stream.ReadArray<PPtr<Sprite>>();
			m_packedSpriteNamesToIndex = stream.ReadStringArray();
			m_renderDataMap.Read(stream);
			Tag = stream.ReadStringAligned();
			IsVariant = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
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

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<PPtr<Sprite>> PackedSprites => m_packedSprites;
		public IReadOnlyList<string> PackedSpriteNamesToIndex => m_packedSpriteNamesToIndex;
		public IReadOnlyDictionary<Tuple<UtinyGUID, long>, SpriteAtlasData> RenderDataMap => m_renderDataMap;
		public string Tag { get; private set; }
		public bool IsVariant { get; private set; }

		private readonly Dictionary<Tuple<UtinyGUID, long>, SpriteAtlasData> m_renderDataMap = new Dictionary<Tuple<UtinyGUID, long>, SpriteAtlasData>();

		private PPtr<Sprite>[] m_packedSprites;
		private string[] m_packedSpriteNamesToIndex;
	}
}
