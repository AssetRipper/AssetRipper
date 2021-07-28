using AssetRipper.IO.Extensions;

namespace AssetRipper.Reading.Classes
{
	public class SerializedShader
    {
        public SerializedProperties m_PropInfo;
        public SerializedSubShader[] m_SubShaders;
        public string m_Name;
        public string m_CustomEditorName;
        public string m_FallbackName;
        public SerializedShaderDependency[] m_Dependencies;
        public SerializedCustomEditorForRenderPipeline[] m_CustomEditorForRenderPipelines;
        public bool m_DisableNoSubshadersMessage;

        public SerializedShader(ObjectReader reader)
        {
            var version = reader.version;

            m_PropInfo = new SerializedProperties(reader);

            int numSubShaders = reader.ReadInt32();
            m_SubShaders = new SerializedSubShader[numSubShaders];
            for (int i = 0; i < numSubShaders; i++)
            {
                m_SubShaders[i] = new SerializedSubShader(reader);
            }

            m_Name = reader.ReadAlignedString();
            m_CustomEditorName = reader.ReadAlignedString();
            m_FallbackName = reader.ReadAlignedString();

            int numDependencies = reader.ReadInt32();
            m_Dependencies = new SerializedShaderDependency[numDependencies];
            for (int i = 0; i < numDependencies; i++)
            {
                m_Dependencies[i] = new SerializedShaderDependency(reader);
            }

            if (version[0] >= 2021) //2021.1 and up
            {
                int m_CustomEditorForRenderPipelinesSize = reader.ReadInt32();
                m_CustomEditorForRenderPipelines = new SerializedCustomEditorForRenderPipeline[m_CustomEditorForRenderPipelinesSize];
                for (int i = 0; i < m_CustomEditorForRenderPipelinesSize; i++)
                {
                    m_CustomEditorForRenderPipelines[i] = new SerializedCustomEditorForRenderPipeline(reader);
                }
            }

            m_DisableNoSubshadersMessage = reader.ReadBoolean();
            reader.AlignStream();
        }
    }
}
