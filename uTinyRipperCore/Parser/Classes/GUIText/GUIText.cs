using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Fonts;
using uTinyRipper.Classes.GUITexts;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class GUIText : GUIElement
	{
		public GUIText(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 1.5.0 and greater
		/// </summary>
		public static bool IsReadPixelOffset(Version version)
		{
			return version.IsGreaterEqual(1, 5);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadFontSize(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool IsReadColor(Version version)
		{
			return version.IsGreaterEqual(4, 2);
		}
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadRichText(Version version)
		{
			return version.IsGreaterEqual(4);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(1, 5))
			{
				return 3;
			}
			// min is 2
			// LineSpacing has been changed
			return 2;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Text = reader.ReadString();
			Anchor = (TextAnchor)reader.ReadInt16();
			Alignment = (TextAlignment)reader.ReadInt16();
			if (IsReadPixelOffset(reader.Version))
			{
				PixelOffset.Read(reader);
			}
			LineSpacing = reader.ReadSingle();
			TabSize = reader.ReadSingle();
			Font.Read(reader);
			Material.Read(reader);
			if (IsReadFontSize(reader.Version))
			{
				FontSize = reader.ReadInt32();
				FontStyle = (FontStyle)reader.ReadInt32();
			}
			if (IsReadColor(reader.Version))
			{
				Color.Read(reader);
			}
			PixelCorrect = reader.ReadBoolean();
			if (IsReadRichText(reader.Version))
			{
				RichText = reader.ReadBoolean();
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			yield return Font.FetchDependency(file, isLog, ToLogString, FontName);
			yield return Material.FetchDependency(file, isLog, ToLogString, MaterialName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
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
			return IsReadFontSize(version) ? Color : ColorRGBA32.White;
		}
		private bool GetRichText(Version version)
		{
			return IsReadRichText(version) ? RichText : true;
		}

		public string Text { get; private set; }
		public TextAnchor Anchor { get; private set; }
		public TextAlignment Alignment { get; private set; }
		public float LineSpacing { get; private set; }
		public float TabSize { get; private set; }
		public int FontSize { get; private set; }
		public FontStyle FontStyle { get; private set; }
		public bool PixelCorrect { get; private set; }
		public bool RichText { get; private set; }

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
