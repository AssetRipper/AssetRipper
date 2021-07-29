using AssetRipper.Classes.Shader.Parameters;
using AssetRipper.IO;

namespace AssetRipper.Reading.Classes
{
	public class SerializedProgramParameters
    {
        public VectorParameter[] m_VectorParams;
        public MatrixParameter[] m_MatrixParams;
        public TextureParameter[] m_TextureParams;
        public BufferBinding[] m_BufferParams;
        public ConstantBuffer[] m_ConstantBuffers;
        public BufferBinding[] m_ConstantBufferBindings;
        public UAVParameter[] m_UAVParams;
        public SamplerParameter[] m_Samplers;

        public SerializedProgramParameters(ObjectReader reader)
        {
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

            int numTextureParams = reader.ReadInt32();
            m_TextureParams = new TextureParameter[numTextureParams];
            for (int i = 0; i < numTextureParams; i++)
            {
                m_TextureParams[i] = new TextureParameter(reader);
            }

            int numBufferParams = reader.ReadInt32();
            m_BufferParams = new BufferBinding[numBufferParams];
            for (int i = 0; i < numBufferParams; i++)
            {
                m_BufferParams[i] = new BufferBinding(reader);
            }

            int numConstantBuffers = reader.ReadInt32();
            m_ConstantBuffers = new ConstantBuffer[numConstantBuffers];
            for (int i = 0; i < numConstantBuffers; i++)
            {
                m_ConstantBuffers[i] = new ConstantBuffer(reader);
            }

            int numConstantBufferBindings = reader.ReadInt32();
            m_ConstantBufferBindings = new BufferBinding[numConstantBufferBindings];
            for (int i = 0; i < numConstantBufferBindings; i++)
            {
                m_ConstantBufferBindings[i] = new BufferBinding(reader);
            }

            int numUAVParams = reader.ReadInt32();
            m_UAVParams = new UAVParameter[numUAVParams];
            for (int i = 0; i < numUAVParams; i++)
            {
                m_UAVParams[i] = new UAVParameter(reader);
            }

            int numSamplers = reader.ReadInt32();
            m_Samplers = new SamplerParameter[numSamplers];
            for (int i = 0; i < numSamplers; i++)
            {
                m_Samplers[i] = new SamplerParameter(reader);
            }
        }
    }
}
