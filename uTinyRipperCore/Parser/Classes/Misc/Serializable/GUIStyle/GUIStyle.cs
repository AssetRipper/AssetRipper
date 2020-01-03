using uTinyRipper.Classes.Fonts;
using uTinyRipper.Classes.GUIStyles;
using uTinyRipper.Classes.GUITexts;
using uTinyRipper.Converters;
using uTinyRipper.Layout;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public struct GUIStyle : IAsset
	{
		public GUIStyle(AssetLayout layout) :
			this()
		{
			Name = string.Empty;
			Normal = new GUIStyleState(layout);
			Hover = new GUIStyleState(layout);
			Active = new GUIStyleState(layout);
			Focused = new GUIStyleState(layout);
			OnNormal = new GUIStyleState(layout);
			OnHover = new GUIStyleState(layout);
			OnActive = new GUIStyleState(layout);
			OnFocused = new GUIStyleState(layout);
			FontSize = 33;
			RichText = true;
			TextClipping = layout.Info.Version.IsGreaterEqual(4) ? TextClipping.Overflow : TextClipping.Clip;
			StretchWidth = true;
		}

		public void Read(AssetReader reader)
		{
			GUIStyleLayout layout = reader.Layout.Serialized.GUIStyle;
			Name = reader.ReadString();
			Normal.Read(reader);
			Hover.Read(reader);
			Active.Read(reader);
			Focused.Read(reader);
			OnNormal.Read(reader);
			OnHover.Read(reader);
			OnActive.Read(reader);
			OnFocused.Read(reader);
			Border.Read(reader);
			if (layout.IsBuiltinFormat)
			{
				Margin.Read(reader);
				Padding.Read(reader);
			}
			else
			{
				Padding.Read(reader);
				Margin.Read(reader);
			}

			Overflow.Read(reader);
			Font.Read(reader);
			if (layout.IsBuiltinFormat)
			{
				FontSize = reader.ReadInt32();
				FontStyle = (FontStyle)reader.ReadInt32();
				Alignment = (TextAnchor)reader.ReadInt32();
				WordWrap = reader.ReadBoolean();
				RichText = reader.ReadBoolean();
				reader.AlignStream();

				TextClipping = (TextClipping)reader.ReadInt32();
				ImagePosition = (ImagePosition)reader.ReadInt32();
				ContentOffset.Read(reader);
				FixedWidth = reader.ReadSingle();
				FixedHeight = reader.ReadSingle();
				StretchWidth = reader.ReadBoolean();
				StretchHeight = reader.ReadBoolean();
				reader.AlignStream();
			}
			else
			{
				ImagePosition = (ImagePosition)reader.ReadInt32();
				Alignment = (TextAnchor)reader.ReadInt32();
				WordWrap = reader.ReadBoolean();
				reader.AlignStream();

				TextClipping = (TextClipping)reader.ReadInt32();
				ContentOffset.Read(reader);
				ClipOffset.Read(reader);
				FixedWidth = reader.ReadSingle();
				FixedHeight = reader.ReadSingle();
				if (layout.HasFontSize)
				{
					FontSize = reader.ReadInt32();
					FontStyle = (FontStyle)reader.ReadInt32();
				}
				StretchWidth = reader.ReadBoolean();
				reader.AlignStream();
				StretchHeight = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		public void Write(AssetWriter writer)
		{
			GUIStyleLayout layout = writer.Layout.Serialized.GUIStyle;
			writer.Write(Name);
			Normal.Write(writer);
			Hover.Write(writer);
			Active.Write(writer);
			Focused.Write(writer);
			OnNormal.Write(writer);
			OnHover.Write(writer);
			OnActive.Write(writer);
			OnFocused.Write(writer);
			Border.Write(writer);
			if (layout.IsBuiltinFormat)
			{
				Margin.Write(writer);
				Padding.Write(writer);
			}
			else
			{
				Padding.Write(writer);
				Margin.Write(writer);
			}

			Overflow.Write(writer);
			Font.Write(writer);
			if (layout.IsBuiltinFormat)
			{
				writer.Write(FontSize);
				writer.Write((int)FontStyle);
				writer.Write((int)Alignment);
				writer.Write(WordWrap);
				writer.Write(RichText);
				writer.AlignStream();

				writer.Write((int)TextClipping);
				writer.Write((int)ImagePosition);
				ContentOffset.Write(writer);
				writer.Write(FixedWidth);
				writer.Write(FixedHeight);
				writer.Write(StretchWidth);
				writer.Write(StretchHeight);
				writer.AlignStream();
			}
			else
			{
				writer.Write((int)ImagePosition);
				writer.Write((int)Alignment);
				writer.Write(WordWrap);
				writer.AlignStream();

				writer.Write((int)TextClipping);
				ContentOffset.Write(writer);
				ClipOffset.Write(writer);
				writer.Write(FixedWidth);
				writer.Write(FixedHeight);
				if (layout.HasFontSize)
				{
					writer.Write(FontSize);
					writer.Write((int)FontStyle);
				}
				writer.Write(StretchWidth);
				writer.AlignStream();
				writer.Write(StretchHeight);
				writer.AlignStream();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			GUIStyleLayout layout = container.ExportLayout.Serialized.GUIStyle;
			node.Add(layout.NameName, Name);
			node.Add(layout.NormalName, Normal.ExportYAML(container));
			node.Add(layout.HoverName, Hover.ExportYAML(container));
			node.Add(layout.ActiveName, Active.ExportYAML(container));
			node.Add(layout.FocusedName, Focused.ExportYAML(container));
			node.Add(layout.OnNormalName, OnNormal.ExportYAML(container));
			node.Add(layout.OnHoverName, OnHover.ExportYAML(container));
			node.Add(layout.OnActiveName, OnActive.ExportYAML(container));
			node.Add(layout.OnFocusedName, OnFocused.ExportYAML(container));
			node.Add(layout.BorderName, Border.ExportYAML(container));
			if (layout.IsBuiltinFormat)
			{
				node.Add(layout.MarginName, Margin.ExportYAML(container));
				node.Add(layout.PaddingName, Padding.ExportYAML(container));
			}
			else
			{
				node.Add(layout.PaddingName, Padding.ExportYAML(container));
				node.Add(layout.MarginName, Margin.ExportYAML(container));
			}

			node.Add(layout.OverflowName, Overflow.ExportYAML(container));
			node.Add(layout.FontName, Font.ExportYAML(container));
			if (layout.IsBuiltinFormat)
			{
				node.Add(layout.FontSizeName, FontSize);
				node.Add(layout.FontStyleName, (int)FontStyle);
				node.Add(layout.AlignmentName, (int)Alignment);
				node.Add(layout.WordWrapName, WordWrap);
				node.Add(layout.RichTextName, RichText);
				node.Add(layout.TextClippingName, (int)TextClipping);
				node.Add(layout.ImagePositionName, (int)ImagePosition);
				node.Add(layout.ContentOffsetName, ContentOffset.ExportYAML(container));
				node.Add(layout.FixedWidthName, FixedWidth);
				node.Add(layout.FixedHeightName, FixedHeight);
				node.Add(layout.StretchWidthName, StretchWidth);
				node.Add(layout.StretchHeightName, StretchHeight);
			}
			else
			{
				node.Add(layout.ImagePositionName, (int)ImagePosition);
				node.Add(layout.AlignmentName, (int)Alignment);
				node.Add(layout.WordWrapName, WordWrap);
				node.Add(layout.TextClippingName, (int)TextClipping);
				node.Add(layout.ContentOffsetName, ContentOffset.ExportYAML(container));
				node.Add(layout.ClipOffsetName, ClipOffset.ExportYAML(container));
				node.Add(layout.FixedWidthName, FixedWidth);
				node.Add(layout.FixedHeightName, FixedHeight);
				if (layout.HasFontSize)
				{
					node.Add(layout.FontSizeName, FontSize);
					node.Add(layout.FontStyleName, (int)FontStyle);
				}
				node.Add(layout.StretchWidthName, StretchWidth);
				node.Add(layout.StretchHeightName, StretchHeight);
			}
			return node;
		}

		public string Name { get; set; }
		public int FontSize { get; set; }
		public FontStyle FontStyle { get; set; }
		public TextAnchor Alignment { get; set; }
		public bool WordWrap { get; set; }
		public bool RichText { get; set; }
		public TextClipping TextClipping { get; set; }
		public ImagePosition ImagePosition { get; set; }
		public float FixedWidth { get; set; }
		public float FixedHeight { get; set; }
		public bool StretchWidth { get; set; }
		public bool StretchHeight { get; set; }

		public GUIStyleState Normal;
		public GUIStyleState Hover;
		public GUIStyleState Active;
		public GUIStyleState Focused;
		public GUIStyleState OnNormal;
		public GUIStyleState OnHover;
		public GUIStyleState OnActive;
		public GUIStyleState OnFocused;
		public RectOffset Border;
		public RectOffset Margin;
		public RectOffset Padding;
		public RectOffset Overflow;
		public PPtr<Font> Font;
		public Vector2f ContentOffset;
		public Vector2f ClipOffset;
	}
}
