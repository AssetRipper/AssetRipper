using AssetRipper.Core.IO.Extensions;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Core.Reading.Classes
{
	public class SerializedTagMap
    {
        public KeyValuePair<string, string>[] tags;

        public SerializedTagMap(BinaryReader reader)
        {
            int numTags = reader.ReadInt32();
            tags = new KeyValuePair<string, string>[numTags];
            for (int i = 0; i < numTags; i++)
            {
                tags[i] = new KeyValuePair<string, string>(reader.ReadAlignedString(), reader.ReadAlignedString());
            }
        }
    }
}
