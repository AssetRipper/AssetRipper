using AssetRipper.Core.IO;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Reading.Classes
{
	public sealed class SpriteAtlas : NamedObject
	{
		public PPtr<Sprite>[] m_PackedSprites;
		public Dictionary<KeyValuePair<Guid, long>, SpriteAtlasData> m_RenderDataMap;

		public SpriteAtlas(ObjectReader reader) : base(reader)
		{
			var m_PackedSpritesSize = reader.ReadInt32();
			m_PackedSprites = new PPtr<Sprite>[m_PackedSpritesSize];
			for (int i = 0; i < m_PackedSpritesSize; i++)
			{
				m_PackedSprites[i] = new PPtr<Sprite>(reader);
			}

			var m_PackedSpriteNamesToIndex = reader.ReadStringArray();

			var m_RenderDataMapSize = reader.ReadInt32();
			m_RenderDataMap = new Dictionary<KeyValuePair<Guid, long>, SpriteAtlasData>(m_RenderDataMapSize);
			for (int i = 0; i < m_RenderDataMapSize; i++)
			{
				var first = new Guid(reader.ReadBytes(16));
				var second = reader.ReadInt64();
				var value = new SpriteAtlasData(reader);
				m_RenderDataMap.Add(new KeyValuePair<Guid, long>(first, second), value);
			}
			//string m_Tag
			//bool m_IsVariant
		}
	}
}
