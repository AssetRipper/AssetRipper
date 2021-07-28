using AssetRipper.IO.Extensions;
using AssetRipper.Math;

namespace AssetRipper.Reading.Classes
{
	public class UnityTexEnv
    {
        public PPtr<Texture> m_Texture;
        public Vector2f m_Scale;
        public Vector2f m_Offset;

        public UnityTexEnv(ObjectReader reader)
        {
            m_Texture = new PPtr<Texture>(reader);
            m_Scale = reader.ReadVector2f();
            m_Offset = reader.ReadVector2f();
        }
    }
}
