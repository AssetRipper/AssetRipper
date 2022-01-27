using AssetRipper.Core.Classes.Font;
using AssetRipper.Core.Classes.GUIText;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Misc.Serializable.GUIStyle
{
	/// <summary>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/IMGUI/GUIStyle.cs"/>
	/// </summary>
	public sealed class GUIStyle : IAsset
	{
		public GUIStyle()
		{
			Name = string.Empty;
			FontSize = 33;
			RichText = true;
			StretchWidth = true;
		}

		public GUIStyle(LayoutInfo layout) : this()
		{
			TextClipping = layout.Version.IsGreaterEqual(4) ? TextClipping.Overflow : TextClipping.Clip;
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
			if (IsBuiltinFormat(reader.Version))
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
			if (IsBuiltinFormat(reader.Version))
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
				if (HasFontSize(reader.Version))
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
			if (IsBuiltinFormat(writer.Version))
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
			if (IsBuiltinFormat(writer.Version))
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
				if (HasFontSize(writer.Version))
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
			if (IsBuiltinFormat(container.ExportVersion))
			{
				node.Add(MarginName, Margin.ExportYAML(container));
				node.Add(PaddingName, Padding.ExportYAML(container));
			}
			else
			{
				node.Add(PaddingName, Padding.ExportYAML(container));
				node.Add(MarginName, Margin.ExportYAML(container));
			}

			node.Add(OverflowName, Overflow.ExportYAML(container));
			node.Add(FontName, Font.ExportYAML(container));
			if (IsBuiltinFormat(container.ExportVersion))
			{
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
			}
			else
			{
				node.Add(ImagePositionName, (int)ImagePosition);
				node.Add(AlignmentName, (int)Alignment);
				node.Add(WordWrapName, WordWrap);
				node.Add(TextClippingName, (int)TextClipping);
				node.Add(ContentOffsetName, ContentOffset.ExportYAML(container));
				node.Add(ClipOffsetName, ClipOffset.ExportYAML(container));
				node.Add(FixedWidthName, FixedWidth);
				node.Add(FixedHeightName, FixedHeight);
				if (HasFontSize(container.ExportVersion))
				{
					node.Add(FontSizeName, FontSize);
					node.Add(FontStyleName, (int)FontStyle);
				}
				node.Add(StretchWidthName, StretchWidth);
				node.Add(StretchHeightName, StretchHeight);
			}
			return node;
		}

		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasFontSize(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasFontStyle(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasRichText(UnityVersion version) => version.IsGreaterEqual(4);
		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool HasClipOffset(UnityVersion version) => version.IsLess(4);

		/// <summary>
		/// 4.0.0 and greater
		/// GUIStyle became builtin serializable only in v4.0.0
		/// </summary>
		public static bool IsBuiltinFormat(UnityVersion version) => version.IsGreaterEqual(4);

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

		public GUIStyleState Normal = new();
		public GUIStyleState Hover = new();
		public GUIStyleState Active = new();
		public GUIStyleState Focused = new();
		public GUIStyleState OnNormal = new();
		public GUIStyleState OnHover = new();
		public GUIStyleState OnActive = new();
		public GUIStyleState OnFocused = new();
		public RectOffset Border = new();
		public RectOffset Margin = new();
		public RectOffset Padding = new();
		public RectOffset Overflow = new();
		public PPtr<IFont> Font = new();
		public Vector2f ContentOffset = new();
		public Vector2f ClipOffset = new();

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
		public const string ClipOffsetName = "m_ClipOffset";
		public const string FixedWidthName = "m_FixedWidth";
		public const string FixedHeightName = "m_FixedHeight";
		public const string StretchWidthName = "m_StretchWidth";
		public const string StretchHeightName = "m_StretchHeight";
	}
}
