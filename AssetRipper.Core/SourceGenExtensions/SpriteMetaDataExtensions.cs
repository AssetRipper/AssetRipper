using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.SpriteMetaData;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class SpriteMetaDataExtensions
	{
		public static SpriteAlignment GetAlignment(this ISpriteMetaData data)
		{
			return (SpriteAlignment)data.Alignment;
		}
	}
}
