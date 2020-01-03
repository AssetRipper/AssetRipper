using System.Collections.Generic;
using uTinyRipper.Classes.Fonts;
using uTinyRipper.Classes.GUITexts;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class GUIText : GUIElement
	{
		public GUIText(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(1, 5))
			{
				return 3;
			}
			// min is 2
			// LineSpacing has been changed
			return 2;
		}

		/// <summary>
		/// 1.5.0 and greater
		/// </summary>
		public static bool HasPixelOffset(Version version) => version.IsGreaterEqual(1, 5);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasFontSize(Version version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool HasColor(Version version) => version.IsGreaterEqual(4, 2);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasRichText(Version version) => version.IsGreaterEqual(4);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Text = reader.ReadString();
			Anchor = (TextAnchor)reader.ReadInt16();
			Alignment = (TextAlignment)reader.ReadInt16();
			if (HasPixelOffset(reader.Version))
			{
				PixelOffset.Read(reader);
			}
			LineSpacing = reader.ReadSingle();
			TabSize = reader.ReadSingle();
			Font.Read(reader);
			Material.Read(reader);
			if (HasFontSize(reader.Version))
			{
				FontSize = reader.ReadInt32();
				FontStyle = (FontStyle)reader.ReadInt32();
			}
			if (HasColor(reader.Version))
			{
				Color.Read(reader);
			}
			PixelCorrect = reader.ReadBoolean();
			if (HasRichText(reader.Version))
			{
				RichText = reader.ReadBoolean();
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(Font, FontName);
			yield return context.FetchDependency(Material, MaterialName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(TextName, Text);
			node.Add(AnchorName, (short)Anchor);
			node.Add(AlignmentName, (short)Alignment);
			node.Add(PixelOffsetName, PixelOffset.ExportYAML(container));
			node.Add(LineSpacingName, LineSpacing);
			node.Add(TabSizeName, TabSize);
			node.Add(FontName, Font.ExportYAML(container));
			node.Add(MaterialName, Material.ExportYAML(container));
			node.Add(FontSizeName, FontSize);
			node.Add(FontStyleName, (int)FontStyle);
			node.Add(ColorName, GetColor(container.Version).ExportYAML(container));
			node.Add(PixelCorrectName, PixelCorrect);
			node.Add(RichTextName, GetRichText(container.Version));
			return node;
		}

		private ColorRGBA32 GetColor(Version version)
		{
			return HasFontSize(version) ? Color : ColorRGBA32.White;
		}
		private bool GetRichText(Version version)
		{
			return HasRichText(version) ? RichText : true;
		}

		public string Text { get; set; }
		public TextAnchor Anchor { get; set; }
		public TextAlignment Alignment { get; set; }
		public float LineSpacing { get; set; }
		public float TabSize { get; set; }
		public int FontSize { get; set; }
		public FontStyle FontStyle { get; set; }
		public bool PixelCorrect { get; set; }
		public bool RichText { get; set; }

		public const string TextName = "m_Text";
		public const string AnchorName = "m_Anchor";
		public const string AlignmentName = "m_Alignment";
		public const string PixelOffsetName = "m_PixelOffset";
		public const string LineSpacingName = "m_LineSpacing";
		public const string TabSizeName = "m_TabSize";
		public const string FontName = "m_Font";
		public const string MaterialName = "m_Material";
		public const string FontSizeName = "m_FontSize";
		public const string FontStyleName = "m_FontStyle";
		public const string ColorName = "m_Color";
		public const string PixelCorrectName = "m_PixelCorrect";
		public const string RichTextName = "m_RichText";

		public Vector2f PixelOffset;
		public PPtr<Font> Font;
		public PPtr<Material> Material;
		public ColorRGBA32 Color;
	}
}
