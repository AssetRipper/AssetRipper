using AssetRipper.Core.IO;

namespace AssetRipper.Core.Reading.Classes
{
	public class SerializedProgram
	{
		public SerializedSubProgram[] m_SubPrograms;
		public SerializedProgramParameters m_CommonParameters;

		public SerializedProgram(ObjectReader reader)
		{
			var version = reader.version;

			int numSubPrograms = reader.ReadInt32();
			m_SubPrograms = new SerializedSubProgram[numSubPrograms];
			for (int i = 0; i < numSubPrograms; i++)
			{
				m_SubPrograms[i] = new SerializedSubProgram(reader);
			}

			if ((version[0] == 2020 && version[1] > 3) ||
			   (version[0] == 2020 && version[1] == 3 && version[2] > 0) ||
			   (version[0] == 2020 && version[1] == 3 && version[2] == 0 && version[3] >= 2) || //2020.3.0f2 to 2020.3.x
			   (version[0] == 2021 && version[1] > 1) ||
			   (version[0] == 2021 && version[1] == 1 && version[2] >= 4)) //2021.1.4f1 to 2021.1.x
			{
				m_CommonParameters = new SerializedProgramParameters(reader);
			}
		}
	}
}
