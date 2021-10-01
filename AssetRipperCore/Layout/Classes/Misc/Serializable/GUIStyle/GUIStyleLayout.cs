using AssetRipper.Core.Classes.Font;
using AssetRipper.Core.Math;

namespace AssetRipper.Core.Layout.Classes.Misc.Serializable.GUIStyle
{
	public sealed class GUIStyleLayout
	{
		public GUIStyleLayout(LayoutInfo info)
		{
			GUIStyleState = new GUIStyleStateLayout(info);

			if (info.Version.IsGreaterEqual(3))
			{
				HasFontSize = true;
				HasFontStyle = true;
			}
			if (info.Version.IsGreaterEqual(4))
			{
				HasRichText = true;
			}
			if (info.Version.IsLess(4))
			{
				HasClipOffset = true;
			}

			IsBuiltinFormat = info.Version.IsGreaterEqual(4);
		}

		public GUIStyleStateLayout GUIStyleState { get; }

		public int Version => 1;

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasName => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasNormal => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasHover => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasActive => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasFocused => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasOnNormal => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasOnHover => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasOnActive => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasOnFocused => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasBorder => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasMargin => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasPadding => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasOverflow => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasFont => true;
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public bool HasFontSize { get; }
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public bool HasFontStyle { get; }
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasAlignment => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasWordWrap => true;
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public bool HasRichText { get; }
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasTextClipping => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasImagePosition => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasContentOffset => true;
		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public bool HasClipOffset { get; }
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasFixedWidth => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasFixedHeight => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasStretchWidth => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasStretchHeight => true;

		/// <summary>
		/// 4.0.0 and greater
		/// GUIStyle became builtin serializable only in v4.0.0
		/// </summary>
		public bool IsBuiltinFormat { get; }

		public string NameName => "m_Name";
		public string NormalName => "m_Normal";
		public string HoverName => "m_Hover";
		public string ActiveName => "m_Active";
		public string FocusedName => "m_Focused";
		public string OnNormalName => "m_OnNormal";
		public string OnHoverName => "m_OnHover";
		public string OnActiveName => "m_OnActive";
		public string OnFocusedName => "m_OnFocused";
		public string BorderName => "m_Border";
		public string MarginName => "m_Margin";
		public string PaddingName => "m_Padding";
		public string OverflowName => "m_Overflow";
		public string FontName => "m_Font";
		public string FontSizeName => "m_FontSize";
		public string FontStyleName => "m_FontStyle";
		public string AlignmentName => "m_Alignment";
		public string WordWrapName => "m_WordWrap";
		public string RichTextName => "m_RichText";
		public string TextClippingName => "m_TextClipping";
		public string ImagePositionName => "m_ImagePosition";
		public string ContentOffsetName => "m_ContentOffset";
		public string ClipOffsetName => "m_ClipOffset";
		public string FixedWidthName => "m_FixedWidth";
		public string FixedHeightName => "m_FixedHeight";
		public string StretchWidthName => "m_StretchWidth";
		public string StretchHeightName => "m_StretchHeight";
	}
}
