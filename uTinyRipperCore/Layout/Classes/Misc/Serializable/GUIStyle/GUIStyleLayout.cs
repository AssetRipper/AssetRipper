using uTinyRipper.Converters;

namespace uTinyRipper.Layout
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

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			GUIStyleLayout layout = context.Layout.Serialized.GUIStyle;
			context.AddNode(layout.Name, name);
			context.BeginChildren();
			context.AddString(layout.NameName);
			GUIStyleStateLayout.GenerateTypeTree(context, layout.NormalName);
			GUIStyleStateLayout.GenerateTypeTree(context, layout.HoverName);
			GUIStyleStateLayout.GenerateTypeTree(context, layout.ActiveName);
			GUIStyleStateLayout.GenerateTypeTree(context, layout.FocusedName);
			GUIStyleStateLayout.GenerateTypeTree(context, layout.OnNormalName);
			GUIStyleStateLayout.GenerateTypeTree(context, layout.OnHoverName);
			GUIStyleStateLayout.GenerateTypeTree(context, layout.OnActiveName);
			GUIStyleStateLayout.GenerateTypeTree(context, layout.OnFocusedName);
			RectOffsetLayout.GenerateTypeTree(context, layout.BorderName);
			if (layout.IsBuiltinFormat)
			{
				RectOffsetLayout.GenerateTypeTree(context, layout.MarginName);
				RectOffsetLayout.GenerateTypeTree(context, layout.PaddingName);
			}
			else
			{
				RectOffsetLayout.GenerateTypeTree(context, layout.PaddingName);
				RectOffsetLayout.GenerateTypeTree(context, layout.MarginName);
			}
			RectOffsetLayout.GenerateTypeTree(context, layout.OverflowName);
			context.AddPPtr(context.Layout.Font.Name, layout.FontName);
			if (layout.IsBuiltinFormat)
			{
				context.AddInt32(layout.FontSizeName);
				context.AddInt32(layout.FontStyleName);
				context.AddInt32(layout.AlignmentName);
				context.AddBool(layout.WordWrapName);
				context.AddBool(layout.RichTextName);
				context.AddInt32(layout.TextClippingName);
				context.AddInt32(layout.ImagePositionName);
				Vector2fLayout.GenerateTypeTree(context, layout.ContentOffsetName);
				context.AddSingle(layout.FixedWidthName);
				context.AddSingle(layout.FixedHeightName);
				context.AddBool(layout.StretchWidthName);
				context.AddBool(layout.StretchHeightName);
			}
			else
			{
				context.AddInt32(layout.ImagePositionName);
				context.AddInt32(layout.AlignmentName);
				context.AddBool(layout.WordWrapName);
				context.AddInt32(layout.TextClippingName);
				Vector2fLayout.GenerateTypeTree(context, layout.ContentOffsetName);
				Vector2fLayout.GenerateTypeTree(context, layout.ClipOffsetName);
				context.AddSingle(layout.FixedWidthName);
				context.AddSingle(layout.FixedHeightName);
				if (layout.HasFontSize)
				{
					context.AddInt32(layout.FontSizeName);
					context.AddInt32(layout.FontStyleName);
				}
				context.AddBool(layout.StretchWidthName);
				context.AddBool(layout.StretchHeightName);
			}
			context.EndChildren();
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

		public string Name => TypeTreeUtils.GUIStyleName;
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
