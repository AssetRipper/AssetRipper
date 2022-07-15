using AssetRipper.Core.Classes.Font;
using AssetRipper.Core.Classes.GUIText;
using AssetRipper.SourceGenerated.Classes.ClassID_102;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class TextMeshExtensions
	{
		public static TextAnchor GetAnchor(this ITextMesh textMesh)
		{
			return (TextAnchor)textMesh.Anchor_C102;
		}
		public static TextAlignment GetAlignment(this ITextMesh textMesh)
		{
			return (TextAlignment)textMesh.Alignment_C102;
		}
		public static FontStyle GetFontStyle(this ITextMesh textMesh)
		{
			return (FontStyle)textMesh.FontStyle_C102;
		}
	}
}
