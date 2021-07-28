using AssetRipper.IO.Extensions;

namespace AssetRipper.Reading.Classes
{
	public abstract class Behaviour : Component
    {
        public byte m_Enabled;

        protected Behaviour(ObjectReader reader) : base(reader)
        {
            m_Enabled = reader.ReadByte();
            reader.AlignStream();
        }
    }
}
