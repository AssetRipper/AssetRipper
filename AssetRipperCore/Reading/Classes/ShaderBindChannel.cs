using System.IO;

namespace AssetRipper.Reading.Classes
{
	public class ShaderBindChannel
    {
        public sbyte source;
        public sbyte target;

        public ShaderBindChannel(BinaryReader reader)
        {
            source = reader.ReadSByte();
            target = reader.ReadSByte();
        }
    }
}
