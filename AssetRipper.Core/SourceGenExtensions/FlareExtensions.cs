using AssetRipper.Core.Classes.Flare;
using AssetRipper.SourceGenerated.Classes.ClassID_121;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class FlareExtensions
	{
		public static TextureLayout GetTextureLayout(this IFlare flare)
		{
			return (TextureLayout)flare.TextureLayout_C121;
		}
	}
}
