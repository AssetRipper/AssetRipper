using AssetRipper.IO.Extensions;

namespace AssetRipper.Reading.Classes
{
	public sealed class MovieTexture : Texture
    {
        public byte[] m_MovieData;
        public PPtr<AudioClip> m_AudioClip;

        public MovieTexture(ObjectReader reader) : base(reader)
        {
            var m_Loop = reader.ReadBoolean();
            reader.AlignStream();
            m_AudioClip = new PPtr<AudioClip>(reader);
            m_MovieData = reader.ReadUInt8Array();
        }
    }
}
