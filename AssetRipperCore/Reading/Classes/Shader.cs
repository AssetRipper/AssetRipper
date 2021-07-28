using AssetRipper.IO.Extensions;
using System.Linq;

namespace AssetRipper.Reading.Classes
{
	public class Shader : NamedObject
    {
        public byte[] m_Script;
        //5.3 - 5.4
        public uint decompressedSize;
        public byte[] m_SubProgramBlob;
        //5.5 and up
        public SerializedShader m_ParsedForm;
        public ShaderCompilerPlatform[] platforms;
        public uint[] offsets;
        public uint[] compressedLengths;
        public uint[] decompressedLengths;
        public byte[] compressedBlob;

        public Shader(ObjectReader reader) : base(reader)
        {
            if (version[0] == 5 && version[1] >= 5 || version[0] > 5) //5.5 and up
            {
                m_ParsedForm = new SerializedShader(reader);
                platforms = reader.ReadUInt32Array().Select(x => (ShaderCompilerPlatform)x).ToArray();
                if (version[0] > 2019 || (version[0] == 2019 && version[1] >= 3)) //2019.3 and up
                {
                    offsets = reader.ReadUInt32ArrayArray().Select(x => x[0]).ToArray();
                    compressedLengths = reader.ReadUInt32ArrayArray().Select(x => x[0]).ToArray();
                    decompressedLengths = reader.ReadUInt32ArrayArray().Select(x => x[0]).ToArray();
                }
                else
                {
                    offsets = reader.ReadUInt32Array();
                    compressedLengths = reader.ReadUInt32Array();
                    decompressedLengths = reader.ReadUInt32Array();
                }
                compressedBlob = reader.ReadUInt8Array();
                reader.AlignStream();

                var m_DependenciesCount = reader.ReadInt32();
                for (int i = 0; i < m_DependenciesCount; i++)
                {
                    new PPtr<Shader>(reader);
                }

                if (version[0] >= 2018)
                {
                    var m_NonModifiableTexturesCount = reader.ReadInt32();
                    for (int i = 0; i < m_NonModifiableTexturesCount; i++)
                    {
                        var first = reader.ReadAlignedString();
                        new PPtr<Texture>(reader);
                    }
                }

                var m_ShaderIsBaked = reader.ReadBoolean();
                reader.AlignStream();
            }
            else
            {
                m_Script = reader.ReadUInt8Array();
                reader.AlignStream();
                var m_PathName = reader.ReadAlignedString();
                if (version[0] == 5 && version[1] >= 3) //5.3 - 5.4
                {
                    decompressedSize = reader.ReadUInt32();
                    m_SubProgramBlob = reader.ReadUInt8Array();
                }
            }
        }
    }
}
