namespace AssetRipper.Reading.Classes
{
	public class PPtrKeyframe
    {
        public float time;
        public PPtr<Classes.Object> value;


        public PPtrKeyframe(ObjectReader reader)
        {
            time = reader.ReadSingle();
            value = new PPtr<Classes.Object>(reader);
        }
    }
}
