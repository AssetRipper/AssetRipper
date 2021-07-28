using AssetRipper.IO.Extensions;

namespace AssetRipper.Reading.Classes
{
	public class SecondarySpriteTexture
    {
        public PPtr<Texture2D> texture;
        public string name;

        public SecondarySpriteTexture(ObjectReader reader)
        {
            texture = new PPtr<Texture2D>(reader);
            name = reader.ReadStringToNull();
        }
    }
}
