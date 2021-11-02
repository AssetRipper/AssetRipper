using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Texture2D;

namespace AssetRipper.Core.Classes.Sprite
{
	public interface ISprite : INamedObject
	{
		PPtr<ITexture2D> TexturePtr { get; }
	}
}
