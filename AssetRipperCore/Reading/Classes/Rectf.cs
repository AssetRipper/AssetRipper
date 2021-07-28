using System.IO;

namespace AssetRipper.Reading.Classes
{
	public class Rectf
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public Rectf(BinaryReader reader)
        {
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            width = reader.ReadSingle();
            height = reader.ReadSingle();
        }
    }
}
