using AssetRipper.IO.Extensions;

namespace AssetRipper.Reading.Classes
{
	public class FloatCurve
    {
        public AnimationCurve<float> curve;
        public string attribute;
        public string path;
        public ClassIDType classID;
        public PPtr<MonoScript> script;


        public FloatCurve(ObjectReader reader)
        {
            curve = new AnimationCurve<float>(reader, reader.ReadSingle);
            attribute = reader.ReadAlignedString();
            path = reader.ReadAlignedString();
            classID = (ClassIDType)reader.ReadInt32();
            script = new PPtr<MonoScript>(reader);
        }
    }
}
