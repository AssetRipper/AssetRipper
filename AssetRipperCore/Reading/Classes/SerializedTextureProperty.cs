using AssetRipper.Classes.Texture2D;
using AssetRipper.IO.Extensions;
using System.IO;

namespace AssetRipper.Reading.Classes
{
	public class SerializedTextureProperty
    {
        public string m_DefaultName;
        public TextureDimension m_TexDim;

        public SerializedTextureProperty(BinaryReader reader)
        {
            m_DefaultName = reader.ReadAlignedString();
            m_TexDim = (TextureDimension)reader.ReadInt32();
        }
    }
}
