using AssetRipper.IO;
using AssetRipper.IO.Extensions;

namespace AssetRipper.Reading.Classes
{
	public class GenericBinding
    {
        public uint path;
        public uint attribute;
        public PPtr<Classes.Object> script;
        public ClassIDType typeID;
        public byte customType;
        public byte isPPtrCurve;

        public GenericBinding() { }

        public GenericBinding(ObjectReader reader)
        {
            var version = reader.version;
            path = reader.ReadUInt32();
            attribute = reader.ReadUInt32();
            script = new PPtr<Classes.Object>(reader);
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                typeID = (ClassIDType)reader.ReadInt32();
            }
            else
            {
                typeID = (ClassIDType)reader.ReadUInt16();
            }
            customType = reader.ReadByte();
            isPPtrCurve = reader.ReadByte();
            reader.AlignStream();
        }
    }
}
