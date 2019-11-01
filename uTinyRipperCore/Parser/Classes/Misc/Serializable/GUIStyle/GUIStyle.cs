using System.Collections.Generic;
using uTinyRipper.Classes.Fonts;
using uTinyRipper.Classes.GUIStyles;
using uTinyRipper.Classes.GUITexts;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper.Game.Assembly;

namespace uTinyRipper.Classes
{
	public struct GUIStyle : ISerializableStructure
	{
		public GUIStyle(bool _):
			this()
		{
			Normal = new GUIStyleState(true);
			Hover = new GUIStyleState(true);
			Active = new GUIStyleState(true);
			Focused = new GUIStyleState(true);
			OnNormal = new GUIStyleState(true);
			OnHover = new GUIStyleState(true);
			OnActive = new GUIStyleState(true);
			OnFocused = new GUIStyleState(true);
		}

		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsBuiltIn(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		private static bool IsReadFontSize(Version version)
		{
			return version.IsGreaterEqual(3, 0);
		}

		public ISerializableStructure CreateDuplicate()
		{
			return new GUIStyle();
		}

		public void Read(AssetReader reader)
		{
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
			if (IsBuiltIn(reader.Version))
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

			if (IsBuiltIn(reader.Version))
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
				if (IsReadFontSize(reader.Version))
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
			if (IsBuiltIn(writer.Version))
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

			if (IsBuiltIn(writer.Version))
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
				if (IsReadFontSize(writer.Version))
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
			node.Add(NameName, Name);
			node.Add(NormalName, Normal.ExportYAML(container));
			node.Add(HoverName, Hover.ExportYAML(container));
			node.Add(ActiveName, Active.ExportYAML(container));
			node.Add(FocusedName, Focused.ExportYAML(container));
			node.Add(OnNormalName, OnNormal.ExportYAML(container));
			node.Add(OnHoverName, OnHover.ExportYAML(container));
			node.Add(OnActiveName, OnActive.ExportYAML(container));
			node.Add(OnFocusedName, OnFocused.ExportYAML(container));
			node.Add(BorderName, Border.ExportYAML(container));
			node.Add(MarginName, Margin.ExportYAML(container));
			node.Add(PaddingName, Padding.ExportYAML(container));
			node.Add(OverflowName, Overflow.ExportYAML(container));
			node.Add(FontName, Font.ExportYAML(container));
			node.Add(FontSizeName, FontSize);
			node.Add(FontStyleName, (int)FontStyle);
			node.Add(AlignmentName, (int)Alignment);
			node.Add(WordWrapName, WordWrap);
			node.Add(RichTextName, RichText);
			node.Add(TextClippingName, (int)TextClipping);
			node.Add(ImagePositionName, (int)ImagePosition);
			node.Add(ContentOffsetName, ContentOffset.ExportYAML(container));
			node.Add(FixedWidthName, FixedWidth);
			node.Add(FixedHeightName, FixedHeight);
			node.Add(StretchWidthName, StretchWidth);
			node.Add(StretchHeightName, StretchHeight);
			return node;
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield break;
		}

		public string Name { get; private set; }
		public int FontSize { get; private set; }
		public FontStyle FontStyle { get; private set; }
		public TextAnchor Alignment { get; private set; }
		public bool WordWrap { get; private set; }
		public bool RichText { get; private set; }
		public TextClipping TextClipping { get; private set; }
		public ImagePosition ImagePosition { get; private set; }
		public float FixedWidth { get; private set; }
		public float FixedHeight { get; private set; }
		public bool StretchWidth { get; private set; }
		public bool StretchHeight { get; private set; }

		public const string NameName = "m_Name";
		public const string NormalName = "m_Normal";
		public const string HoverName = "m_Hover";
		public const string ActiveName = "m_Active";
		public const string FocusedName = "m_Focused";
		public const string OnNormalName = "m_OnNormal";
		public const string OnHoverName = "m_OnHover";
		public const string OnActiveName = "m_OnActive";
		public const string OnFocusedName = "m_OnFocused";
		public const string BorderName = "m_Border";
		public const string MarginName = "m_Margin";
		public const string PaddingName = "m_Padding";
		public const string OverflowName = "m_Overflow";
		public const string FontName = "m_Font";
		public const string FontSizeName = "m_FontSize";
		public const string FontStyleName = "m_FontStyle";
		public const string AlignmentName = "m_Alignment";
		public const string WordWrapName = "m_WordWrap";
		public const string RichTextName = "m_RichText";
		public const string TextClippingName = "m_TextClipping";
		public const string ImagePositionName = "m_ImagePosition";
		public const string ContentOffsetName = "m_ContentOffset";
		public const string FixedWidthName = "m_FixedWidth";
		public const string FixedHeightName = "m_FixedHeight";
		public const string StretchWidthName = "m_StretchWidth";
		public const string StretchHeightName = "m_StretchHeight";

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
