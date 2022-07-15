using AssetRipper.Core.Classes.Font;
using AssetRipper.Core.Classes.GUIText;
using AssetRipper.SourceGenerated.Classes.ClassID_132;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class GUITextExtensions
	{
		public static TextAnchor GetAnchor(this IGUIText text)
		{
			return (TextAnchor)text.Anchor_C132;
		}

		public static TextAlignment GetAlignment(this IGUIText text)
		{
			return (TextAlignment)text.Alignment_C132;
		}

		public static FontStyle GetFontStyle(this IGUIText text)
		{
			return (FontStyle)text.FontStyle_C132;
		}
	}
}
