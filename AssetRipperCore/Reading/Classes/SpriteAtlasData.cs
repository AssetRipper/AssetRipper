using AssetRipper.IO;
using AssetRipper.IO.Extensions;
using AssetRipper.Math;

namespace AssetRipper.Reading.Classes
{
	public class SpriteAtlasData
    {
        public PPtr<Texture2D> texture;
        public PPtr<Texture2D> alphaTexture;
        public Rectf textureRect;
        public Vector2f textureRectOffset;
        public Vector2f atlasRectOffset;
        public Vector4f uvTransform;
        public float downscaleMultiplier;
        public SpriteSettings settingsRaw;
        public SecondarySpriteTexture[] secondaryTextures;

        public SpriteAtlasData(ObjectReader reader)
        {
            var version = reader.version;
            texture = new PPtr<Texture2D>(reader);
            alphaTexture = new PPtr<Texture2D>(reader);
            textureRect = new Rectf(reader);
            textureRectOffset = reader.ReadVector2f();
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 2)) //2017.2 and up
            {
                atlasRectOffset = reader.ReadVector2f();
            }
            uvTransform = reader.ReadVector4f();
            downscaleMultiplier = reader.ReadSingle();
            settingsRaw = new SpriteSettings(reader);
            if (version[0] > 2020 || (version[0] == 2020 && version[1] >= 2)) //2020.2 and up
            {
                var secondaryTexturesSize = reader.ReadInt32();
                secondaryTextures = new SecondarySpriteTexture[secondaryTexturesSize];
                for (int i = 0; i < secondaryTexturesSize; i++)
                {
                    secondaryTextures[i] = new SecondarySpriteTexture(reader);
                }
                reader.AlignStream();
            }
        }
    }
}
