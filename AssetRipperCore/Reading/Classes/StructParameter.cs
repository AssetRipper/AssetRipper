using System.IO;

namespace AssetRipper.Reading.Classes
{
	public class StructParameter
    {
        public MatrixParameter[] m_MatrixParams;
        public VectorParameter[] m_VectorParams;

        public StructParameter(BinaryReader reader)
        {
            var m_NameIndex = reader.ReadInt32();
            var m_Index = reader.ReadInt32();
            var m_ArraySize = reader.ReadInt32();
            var m_StructSize = reader.ReadInt32();

            int numVectorParams = reader.ReadInt32();
            m_VectorParams = new VectorParameter[numVectorParams];
            for (int i = 0; i < numVectorParams; i++)
            {
                m_VectorParams[i] = new VectorParameter(reader);
            }

            int numMatrixParams = reader.ReadInt32();
            m_MatrixParams = new MatrixParameter[numMatrixParams];
            for (int i = 0; i < numMatrixParams; i++)
            {
                m_MatrixParams[i] = new MatrixParameter(reader);
            }
        }
    }
}
