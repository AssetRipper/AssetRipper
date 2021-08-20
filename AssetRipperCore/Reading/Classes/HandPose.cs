using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO;

namespace AssetRipper.Core.Reading.Classes
{
	public class HandPose
	{
		public XForm m_GrabX;
		public float[] m_DoFArray;
		public float m_Override;
		public float m_CloseOpen;
		public float m_InOut;
		public float m_Grab;

		public HandPose(ObjectReader reader)
		{
			m_GrabX = new XForm(reader);
			m_DoFArray = reader.ReadSingleArray();
			m_Override = reader.ReadSingle();
			m_CloseOpen = reader.ReadSingle();
			m_InOut = reader.ReadSingle();
			m_Grab = reader.ReadSingle();
		}
	}
}
