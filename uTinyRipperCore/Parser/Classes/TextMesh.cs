using uTinyRipper.YAML;
using uTinyRipper.Classes.Fonts;
using uTinyRipper.Classes.GUITexts;
using System.Collections.Generic;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public sealed class TextMesh : Component
	{
		public TextMesh(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadFontSize(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadRichText(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool IsReadColor(Version version)
		{
			return version.IsGreaterEqual(4, 2);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(1, 5))
			{
				return 3;
			}
			// min is 2
			return 2;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Text = reader.ReadString();
			OffsetZ = reader.ReadSingle();
			CharacterSize = reader.ReadSingle();
			LineSpacing = reader.ReadSingle();
			Anchor = (TextAnchor)reader.ReadInt16();
			Alignment = (TextAlignment)reader.ReadInt16();
			TabSize = reader.ReadSingle();
			if (IsReadFontSize(reader.Version))
			{
				FontSize = reader.ReadInt32();
				FontStyle = (FontStyle)reader.ReadInt32();
			}
			if (IsReadRichText(reader.Version))
			{
				RichText = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			Font.Read(reader);
			if (IsReadColor(reader.Version))
			{
				Color.Read(reader);
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(Font, FontName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(TextName, Text);
			node.Add(OffsetZName, OffsetZ);
			node.Add(CharacterSizeName, CharacterSize);
			node.Add(LineSpacingName, LineSpacing);
			node.Add(AnchorName, (short)Anchor);
			node.Add(AlignmentName, (short)Alignment);
			node.Add(TabSizeName, TabSize);
			if (IsReadFontSize(container.ExportVersion))
			{
				node.Add(FontSizeName, FontSize);
				node.Add(FontStyleName, (int)FontStyle);
			}
			if (IsReadRichText(container.ExportVersion))
			{
				node.Add(RichTextName, GetRichText(container.Version));
			}
			node.Add(FontName, Font.ExportYAML(container));
			if (IsReadColor(container.ExportVersion))
			{
				node.Add(ColorName, GetColor(container.Version).ExportYAML(container));
			}
			return node;
		}

		private bool GetRichText(Version version)
		{
			return IsReadRichText(version) ? RichText : true;
		}
		private ColorRGBA32 GetColor(Version version)
		{
			return IsReadFontSize(version) ? Color : ColorRGBA32.White;
		}

		public string Text { get; private set; }
		public float OffsetZ { get; private set; }
		public float CharacterSize { get; private set; }
		public float LineSpacing { get; private set; }
		public TextAnchor Anchor { get; private set; }
		public TextAlignment Alignment { get; private set; }
		public float TabSize { get; private set; }
		public int FontSize { get; private set; }
		public FontStyle FontStyle { get; private set; }
		public bool RichText { get; private set; }

		public const string TextName = "m_Text";
		public const string OffsetZName = "m_OffsetZ";
		public const string CharacterSizeName = "m_CharacterSize";
		public const string LineSpacingName = "m_LineSpacing";
		public const string AnchorName = "m_Anchor";
		public const string AlignmentName = "m_Alignment";
		public const string TabSizeName = "m_TabSize";
		public const string FontSizeName = "m_FontSize";
		public const string FontStyleName = "m_FontStyle";
		public const string RichTextName = "m_RichText";
		public const string FontName = "m_Font";
		public const string ColorName = "m_Color";

		public PPtr<Font> Font;
		public ColorRGBA32 Color;
	}
}