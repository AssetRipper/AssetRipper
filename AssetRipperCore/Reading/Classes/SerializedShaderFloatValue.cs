using AssetRipper.IO.Extensions;
using System.IO;

namespace AssetRipper.Reading.Classes
{
	public class SerializedShaderFloatValue
    {
        public float val;
        public string name;

        public SerializedShaderFloatValue(BinaryReader reader)
        {
            val = reader.ReadSingle();
            name = reader.ReadAlignedString();
        }
    }
}
