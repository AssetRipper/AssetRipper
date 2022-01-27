using AssetRipper.Core.Classes.Font;
using AssetRipper.Core.Classes.GUIText;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
{
	public sealed class TextMesh : Component
	{
		public TextMesh(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(1, 5))
			{
				return 3;
			}
			// min is 2
			return 2;
		}

		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasFontSize(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasRichText(UnityVersion version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool HasColorRGBAf(UnityVersion version) => version.IsGreaterEqual(4, 2);

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
			if (HasFontSize(reader.Version))
			{
				FontSize = reader.ReadInt32();
				FontStyle = (FontStyle)reader.ReadInt32();
			}
			if (HasRichText(reader.Version))
			{
				RichText = reader.ReadBoolean();
				reader.AlignStream();
			}
			Font.Read(reader);
			if (HasColorRGBAf(reader.Version))
			{
				Color.Read(reader);
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(Font, FontName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(TextName, Text);
			node.Add(OffsetZName, OffsetZ);
			node.Add(CharacterSizeName, CharacterSize);
			node.Add(LineSpacingName, LineSpacing);
			node.Add(AnchorName, (short)Anchor);
			node.Add(AlignmentName, (short)Alignment);
			node.Add(TabSizeName, TabSize);
			if (HasFontSize(container.ExportVersion))
			{
				node.Add(FontSizeName, FontSize);
				node.Add(FontStyleName, (int)FontStyle);
			}
			if (HasRichText(container.ExportVersion))
			{
				node.Add(RichTextName, GetRichText(container.Version));
			}
			node.Add(FontName, Font.ExportYAML(container));
			if (HasColorRGBAf(container.ExportVersion))
			{
				node.Add(ColorName, GetColorRGBAf(container.Version).ExportYAML(container));
			}
			return node;
		}

		private bool GetRichText(UnityVersion version)
		{
			return HasRichText(version) ? RichText : true;
		}
		private ColorRGBA32 GetColorRGBAf(UnityVersion version)
		{
			return HasFontSize(version) ? Color : ColorRGBA32.White;
		}

		public string Text { get; set; }
		public float OffsetZ { get; set; }
		public float CharacterSize { get; set; }
		public float LineSpacing { get; set; }
		public TextAnchor Anchor { get; set; }
		public TextAlignment Alignment { get; set; }
		public float TabSize { get; set; }
		public int FontSize { get; set; }
		public FontStyle FontStyle { get; set; }
		public bool RichText { get; set; }

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

		public PPtr<Font.Font> Font = new();
		public ColorRGBA32 Color = new();
	}
}
