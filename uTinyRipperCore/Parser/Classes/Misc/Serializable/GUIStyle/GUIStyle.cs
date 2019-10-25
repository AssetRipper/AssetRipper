using System.Collections.Generic;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Fonts;
using uTinyRipper.Classes.GUIStyles;
using uTinyRipper.Classes.GUITexts;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;

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
				reader.AlignStream(AlignType.Align4);

				TextClipping = (TextClipping)reader.ReadInt32();
				ImagePosition = (ImagePosition)reader.ReadInt32();
				ContentOffset.Read(reader);
				FixedWidth = reader.ReadSingle();
				FixedHeight = reader.ReadSingle();
				StretchWidth = reader.ReadBoolean();
				StretchHeight = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			else
			{
				ImagePosition = (ImagePosition)reader.ReadInt32();
				Alignment = (TextAnchor)reader.ReadInt32();
				WordWrap = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);

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
				reader.AlignStream(AlignType.Align4);
				StretchHeight = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Name", Name);
			node.Add("m_Normal", Normal.ExportYAML(container));
			node.Add("m_Hover", Hover.ExportYAML(container));
			node.Add("m_Active", Active.ExportYAML(container));
			node.Add("m_Focused", Focused.ExportYAML(container));
			node.Add("m_OnNormal", OnNormal.ExportYAML(container));
			node.Add("m_OnHover", OnHover.ExportYAML(container));
			node.Add("m_OnActive", OnActive.ExportYAML(container));
			node.Add("m_OnFocused", OnFocused.ExportYAML(container));
			node.Add("m_Border", Border.ExportYAML(container));
			node.Add("m_Margin", Margin.ExportYAML(container));
			node.Add("m_Padding", Padding.ExportYAML(container));
			node.Add("m_Overflow", Overflow.ExportYAML(container));
			node.Add("m_Font", Font.ExportYAML(container));
			node.Add("m_FontSize", FontSize);
			node.Add("m_FontStyle", (int)FontStyle);
			node.Add("m_Alignment", (int)Alignment);
			node.Add("m_WordWrap", WordWrap);
			node.Add("m_RichText", RichText);
			node.Add("m_TextClipping", (int)TextClipping);
			node.Add("m_ImagePosition", (int)ImagePosition);
			node.Add("m_ContentOffset", ContentOffset.ExportYAML(container));
			node.Add("m_FixedWidth", FixedWidth);
			node.Add("m_FixedHeight", FixedHeight);
			node.Add("m_StretchWidth", StretchWidth);
			node.Add("m_StretchHeight", StretchHeight);
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
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
