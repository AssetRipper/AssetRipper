using AssetRipper.IO;
using AssetRipper.IO.Extensions;
using AssetRipper.Math;
using System;
using System.Collections.Generic;

namespace AssetRipper.Reading.Classes
{
	public sealed class Sprite : NamedObject
    {
        public Rectf m_Rect;
        public Vector2f m_Offset;
        public Vector4f m_Border;
        public float m_PixelsToUnits;
        public Vector2f m_Pivot;
        public uint m_Extrude;
        public bool m_IsPolygon;
        public KeyValuePair<Guid, long> m_RenderDataKey;
        public string[] m_AtlasTags;
        public PPtr<SpriteAtlas> m_SpriteAtlas;
        public SpriteRenderData m_RD;
        public Vector2f[][] m_PhysicsShape;

        public Sprite(ObjectReader reader) : base(reader)
        {
            m_Rect = new Rectf(reader);
            m_Offset = reader.ReadVector2f();
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 5)) //4.5 and up
            {
                m_Border = reader.ReadVector4f();
            }

            m_PixelsToUnits = reader.ReadSingle();
            if (version[0] > 5
                || (version[0] == 5 && version[1] > 4)
                || (version[0] == 5 && version[1] == 4 && version[2] >= 2)
                || (version[0] == 5 && version[1] == 4 && version[2] == 1 && buildType.IsPatch && version[3] >= 3)) //5.4.1p3 and up
            {
                m_Pivot = reader.ReadVector2f();
            }

            m_Extrude = reader.ReadUInt32();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 3)) //5.3 and up
            {
                m_IsPolygon = reader.ReadBoolean();
                reader.AlignStream();
            }

            if (version[0] >= 2017) //2017 and up
            {
                var first = new Guid(reader.ReadBytes(16));
                var second = reader.ReadInt64();
                m_RenderDataKey = new KeyValuePair<Guid, long>(first, second);

                m_AtlasTags = reader.ReadStringArray();

                m_SpriteAtlas = new PPtr<SpriteAtlas>(reader);
            }

            m_RD = new SpriteRenderData(reader);

            if (version[0] >= 2017) //2017 and up
            {
                var m_PhysicsShapeSize = reader.ReadInt32();
                m_PhysicsShape = new Vector2f[m_PhysicsShapeSize][];
                for (int i = 0; i < m_PhysicsShapeSize; i++)
                {
                    m_PhysicsShape[i] = reader.ReadVector2Array();
                }
            }

            //vector m_Bones 2018 and up
        }
    }
}
