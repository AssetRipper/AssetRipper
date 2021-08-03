using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Extensions;

namespace AssetRipper.Core.Reading.Classes
{
	public class ConstantBuffer
    {
        public int m_NameIndex;
        public MatrixParameter[] m_MatrixParams;
        public VectorParameter[] m_VectorParams;
        public StructParameter[] m_StructParams;
        public int m_Size;
        public bool m_IsPartialCB;

        public ConstantBuffer(ObjectReader reader)
        {
            var version = reader.version;

            m_NameIndex = reader.ReadInt32();

            int numMatrixParams = reader.ReadInt32();
            m_MatrixParams = new MatrixParameter[numMatrixParams];
            for (int i = 0; i < numMatrixParams; i++)
            {
                m_MatrixParams[i] = new MatrixParameter(reader);
            }

            int numVectorParams = reader.ReadInt32();
            m_VectorParams = new VectorParameter[numVectorParams];
            for (int i = 0; i < numVectorParams; i++)
            {
                m_VectorParams[i] = new VectorParameter(reader);
            }
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)) //2017.3 and up
            {
                int numStructParams = reader.ReadInt32();
                m_StructParams = new StructParameter[numStructParams];
                for (int i = 0; i < numStructParams; i++)
                {
                    m_StructParams[i] = new StructParameter(reader);
                }
            }
            m_Size = reader.ReadInt32();

            if ((version[0] == 2020 && version[1] > 3) ||
               (version[0] == 2020 && version[1] == 3 && version[2] > 0) ||
               (version[0] == 2020 && version[1] == 3 && version[2] == 0 && version[3] >= 2) || //2020.3.0f2 to 2020.3.x
               (version[0] == 2021 && version[1] > 1) ||
               (version[0] == 2021 && version[1] == 1 && version[2] >= 4)) //2021.1.4f1 to 2021.1.x
            {
                m_IsPartialCB = reader.ReadBoolean();
                reader.AlignStream();
            }
        }
    }
}
