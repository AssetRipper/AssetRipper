using AssetRipper.IO.Extensions;
using System.IO;

namespace AssetRipper.Reading.Classes
{
	public class SerializedShaderDependency
    {
        public string from;
        public string to;

        public SerializedShaderDependency(BinaryReader reader)
        {
            from = reader.ReadAlignedString();
            to = reader.ReadAlignedString();
        }
    }
}
