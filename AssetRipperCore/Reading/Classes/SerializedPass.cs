using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Core.Reading.Classes
{
	public class SerializedPass
    {
        public Hash128[] m_EditorDataHash;
        public byte[] m_Platforms;
        public ushort[] m_LocalKeywordMask;
        public ushort[] m_GlobalKeywordMask;
        public KeyValuePair<string, int>[] m_NameIndices;
        public SerializedPassType m_Type;
        public SerializedShaderState m_State;
        public uint m_ProgramMask;
        public SerializedProgram progVertex;
        public SerializedProgram progFragment;
        public SerializedProgram progGeometry;
        public SerializedProgram progHull;
        public SerializedProgram progDomain;
        public SerializedProgram progRayTracing;
        public bool m_HasInstancingVariant;
        public string m_UseName;
        public string m_Name;
        public string m_TextureName;
        public SerializedTagMap m_Tags;

        public SerializedPass(ObjectReader reader)
        {
            var version = reader.version;

            if (version[0] > 2020 || (version[0] == 2020 && version[1] >= 2)) //2020.2 and up
            {
                int numEditorDataHash = reader.ReadInt32();
                m_EditorDataHash = new Hash128[numEditorDataHash];
                for (int i = 0; i < numEditorDataHash; i++)
                {
                    m_EditorDataHash[i] = new Hash128(reader);
                }
                reader.AlignStream();
                m_Platforms = reader.ReadUInt8Array();
                reader.AlignStream();
                m_LocalKeywordMask = reader.ReadUInt16Array();
                reader.AlignStream();
                m_GlobalKeywordMask = reader.ReadUInt16Array();
                reader.AlignStream();
            }

            int numIndices = reader.ReadInt32();
            m_NameIndices = new KeyValuePair<string, int>[numIndices];
            for (int i = 0; i < numIndices; i++)
            {
                m_NameIndices[i] = new KeyValuePair<string, int>(reader.ReadAlignedString(), reader.ReadInt32());
            }

            m_Type = (SerializedPassType)reader.ReadInt32();
            m_State = new SerializedShaderState(reader);
            m_ProgramMask = reader.ReadUInt32();
            progVertex = new SerializedProgram(reader);
            progFragment = new SerializedProgram(reader);
            progGeometry = new SerializedProgram(reader);
            progHull = new SerializedProgram(reader);
            progDomain = new SerializedProgram(reader);
            if (version[0] > 2019 || (version[0] == 2019 && version[1] >= 3)) //2019.3 and up
            {
                progRayTracing = new SerializedProgram(reader);
            }
            m_HasInstancingVariant = reader.ReadBoolean();
            if (version[0] >= 2018) //2018 and up
            {
                var m_HasProceduralInstancingVariant = reader.ReadBoolean();
            }
            reader.AlignStream();
            m_UseName = reader.ReadAlignedString();
            m_Name = reader.ReadAlignedString();
            m_TextureName = reader.ReadAlignedString();
            m_Tags = new SerializedTagMap(reader);
        }
    }
}
