using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;

namespace AssetRipper.Core.Reading.Classes
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
