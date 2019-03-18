using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Fonts;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class Font : NamedObject
	{
		public Font(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool IsReadFontCount(Version version)
		{
			return version.IsLess(4);
		}
		/// <summary>
		/// Less than 5.3.0
		/// </summary>
		public static bool IsReadKerning(Version version)
		{
			return version.IsLess(5, 3);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadTracking(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadCharacterSpacing(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool IsReadPerCharacterKerning(Version version)
		{
			return version.IsLess(4);
		}
		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool IsReadGridFont(Version version)
		{
			return version.IsLess(4);
		}
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool IsBytePerCharacterKerning(Version version)
		{
			return version.IsLess(2, 1);
		}
		/// <summary>
		/// Less than 1.6.0
		/// </summary>
		public static bool IsByteKerningValues(Version version)
		{
			return version.IsLess(1, 6);
		}
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadPixelScale(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadFontData(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadDescent(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadDefaultStyle(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadFallbackFonts(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 5.6.4p1 to 2017 exclusive or 2017.2.1 and greater
		/// </summary>
		public static bool IsReadUseLegacyBoundsCalculation(Version version)
		{
			return version.IsGreaterEqual(5, 6, 4, VersionType.Patch) && version.IsLess(2017) ||
				version.IsGreaterEqual(2017, 1, 3) && version.IsLess(2017, 2) ||
				version.IsGreaterEqual(2017, 2, 0, VersionType.Patch, 2) && version.IsLess(2017, 3) ||
				version.IsGreaterEqual(2017, 3, 0, VersionType.Beta);
		}
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool IsReadShouldRoundAdvanceValue(Version version)
		{
			return version.IsGreaterEqual(2018, 1);
		}

		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		private static bool IsReadFontImpl(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// Less than 2.1.0
		/// </summary>
		private static bool IsShortAsciiStartOffset(Version version)
		{
			return version.IsLess(2, 1);
		}
		/// <summary>
		/// Less than 2.1.0
		/// </summary>
		private static bool IsGridFontFirst(Version version)
		{
			return version.IsLess(2, 1);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(3);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(5, 4))
			{
				return 5;
			}
			if (version.IsGreaterEqual(4))
			{
				return 4;
			}
			if (version.IsGreaterEqual(1, 6))
			{
				return 3;
			}
			// min version is 2nd
			return 2;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadFontImpl(reader.Version))
			{
				LineSpacing = reader.ReadSingle();
				DefaultMaterial.Read(reader);
				FontSize = reader.ReadSingle();
				Texture.Read(reader);
				reader.AlignStream(AlignType.Align4);
			}

			if (IsShortAsciiStartOffset(reader.Version))
			{
				AsciiStartOffset = reader.ReadInt16();
				FontCountX = reader.ReadInt16();
				FontCountY = reader.ReadInt16();
			}
			else
			{
				AsciiStartOffset = reader.ReadInt32();
				if (IsReadFontCount(reader.Version))
				{
					FontCountX = reader.ReadInt32();
					FontCountY = reader.ReadInt32();
				}
			}

			if (IsReadKerning(reader.Version))
			{
				Kerning = reader.ReadSingle();
			}
			if (IsReadTracking(reader.Version))
			{
				Tracking = reader.ReadSingle();
			}

			if (!IsReadFontImpl(reader.Version))
			{
				LineSpacing = reader.ReadSingle();
			}

			if (IsReadCharacterSpacing(reader.Version))
			{
				CharacterSpacing = reader.ReadInt32();
				CharacterPadding = reader.ReadInt32();
			}

			if (IsReadPerCharacterKerning(reader.Version))
			{
				if (IsBytePerCharacterKerning(reader.Version))
				{
					m_perCharacterKerningByte = reader.ReadTupleByteSingleArray();
				}
				else
				{
					m_perCharacterKerning = reader.ReadTupleIntFloatArray();
				}
			}

			ConvertCase = reader.ReadInt32();
			if (!IsReadFontImpl(reader.Version))
			{
				DefaultMaterial.Read(reader);
			}
			m_characterRects = reader.ReadAssetArray<CharacterInfo>();
			if (!IsReadFontImpl(reader.Version))
			{
				Texture.Read(reader);
			}

			if (IsReadGridFont(reader.Version))
			{
				if (IsGridFontFirst(reader.Version))
				{
					GridFont = reader.ReadBoolean();
				}
			}

			if (IsByteKerningValues(reader.Version))
			{
				m_kerningValuesByte = new Dictionary<Tuple<byte, byte>, float>();
				m_kerningValuesByte.Read(reader);
			}
			else
			{
				m_kerningValues.Read(reader);
			}

			if (IsReadPixelScale(reader.Version))
			{
				PixelScale = reader.ReadSingle();
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadGridFont(reader.Version))
			{
				if (!IsGridFontFirst(reader.Version))
				{
					GridFont = reader.ReadBoolean();
					if (IsAlign(reader.Version))
					{
						reader.AlignStream(AlignType.Align4);
					}
				}
			}

			if (IsReadFontData(reader.Version))
			{
				m_fontData = reader.ReadByteArray();
				reader.AlignStream(AlignType.Align4);

				if (!IsReadFontImpl(reader.Version))
				{
					FontSize = reader.ReadSingle();
				}
				Ascent = reader.ReadSingle();
			}
			if (IsReadDescent(reader.Version))
			{
				Descent = reader.ReadSingle();
			}
			if (IsReadDefaultStyle(reader.Version))
			{
				DefaultStyle = (FontStyle)reader.ReadUInt32();
				m_fontNames = reader.ReadStringArray();
			}

			if (IsReadFallbackFonts(reader.Version))
			{
				m_fallbackFonts = reader.ReadAssetArray<PPtr<Font>>();
				reader.AlignStream(AlignType.Align4);

				FontRenderingMode = (FontRenderingMode)reader.ReadInt32();
			}

			if (IsReadUseLegacyBoundsCalculation(reader.Version))
			{
				UseLegacyBoundsCalculation = reader.ReadBoolean();
			}
			if (IsReadShouldRoundAdvanceValue(reader.Version))
			{
				ShouldRoundAdvanceValue = reader.ReadBoolean();
			}
		}

		public override void ExportBinary(IExportContainer container, Stream stream)
		{
			if (IsReadFontData(container.Version))
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					writer.Write(m_fontData);
				}
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			yield return DefaultMaterial.FetchDependency(file, isLog, ToLogString, DefaultMaterialName);
			yield return Texture.FetchDependency(file, isLog, ToLogString, TextureName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(LineSpacingName, LineSpacing);
			node.Add(DefaultMaterialName, DefaultMaterial.ExportYAML(container));
			node.Add(FontSizeName, FontSize);
			node.Add(TextureName, Texture.ExportYAML(container));
			node.Add(AsciiStartOffsetName, AsciiStartOffset);
			node.Add(TrackingName, GetTracking(container.Version));
			node.Add(CharacterSpacingName, CharacterSpacing);
			node.Add(CharacterPaddingName, GetCharacterPadding(container.Version));
			node.Add(ConvertCaseName, ConvertCase);
			node.Add(CharacterRectsName, CharacterRects.ExportYAML(container));
			node.Add(KerningValuesName, KerningValues.ExportYAML(container));
			node.Add(PixelScaleName, GetPixelScale(container.Version));
			node.Add(FontDataName, GetFontData(container.Version).ExportYAML());
			node.Add(AscentName, Ascent);
			node.Add(DescentName, Descent);
			node.Add(DefaultStyleName, (int)DefaultStyle);
			node.Add(FontNamesName, GetFontNames(container.Version).ExportYAML());
			node.Add(FallbackFontsName, GetFallbackFonts(container.Version).ExportYAML(container));
			node.Add(FontRenderingModeName, (int)FontRenderingMode);
			node.Add(UseLegacyBoundsCalculationName, UseLegacyBoundsCalculation);
			node.Add(ShouldRoundAdvanceValueName, GetShouldRoundAdvanceValue(container.Version));
			return node;
		}

		private float GetTracking(Version version)
		{
			return IsReadTracking(version) ? Tracking : 1.0f;
		}
		private int GetCharacterPadding(Version version)
		{
			return IsReadCharacterSpacing(version) ? CharacterPadding : 1;
		}
		private float GetPixelScale(Version version)
		{
			return IsReadPixelScale(version) ? PixelScale : 0.1f;
		}
		private IReadOnlyList<byte> GetFontData(Version version)
		{
			return IsReadFontData(version) ? FontData : new byte[0];
		}
		private IReadOnlyList<string> GetFontNames(Version version)
		{
			return IsReadDefaultStyle(version) ? FontNames : new string[] { Name };
		}
		private IReadOnlyList<PPtr<Font>> GetFallbackFonts(Version version)
		{
			return IsReadFallbackFonts(version) ? FallbackFonts : new PPtr<Font>[0];
		}
		private bool GetShouldRoundAdvanceValue(Version version)
		{
			return IsReadShouldRoundAdvanceValue(version) ? ShouldRoundAdvanceValue : true;
		}

		public bool IsValidData => m_fontData != null && m_fontData.Length > 0;

		public int AsciiStartOffset { get; private set; }
		public int FontCountX { get; private set; }
		public int FontCountY { get; private set; }
		public float Kerning { get; private set; }
		public float Tracking { get; private set; }
		public float LineSpacing { get; private set; }
		public int CharacterSpacing { get; private set; }
		public int CharacterPadding { get; private set; }
		public IReadOnlyList<Tuple<byte, float>> PerCharacterKerningByte => m_perCharacterKerningByte;
		public IReadOnlyList<Tuple<int, float>> PerCharacterKerning => m_perCharacterKerning;
		public int ConvertCase { get; private set; }
		public IReadOnlyList<CharacterInfo> CharacterRects => m_characterRects;
		public bool GridFont { get; private set; }
		public float PixelScale { get; private set; }
		public IReadOnlyDictionary<Tuple<byte, byte>, float> KerningValuesByte => m_kerningValuesByte;
		public IReadOnlyDictionary<Tuple<ushort, ushort>, float> KerningValues => m_kerningValues;
		public IReadOnlyList<byte> FontData => m_fontData;
		public float FontSize { get; private set; }
		public float Ascent { get; private set; }
		public float Descent { get; private set; }
		public FontStyle DefaultStyle {get; private set; }
		public IReadOnlyList<string> FontNames => m_fontNames;
		public IReadOnlyList<PPtr<Font>> FallbackFonts => m_fallbackFonts;
		public FontRenderingMode FontRenderingMode { get; private set; }
		public bool UseLegacyBoundsCalculation { get; private set; }
		public bool ShouldRoundAdvanceValue { get; private set; }

		public const string LineSpacingName = "m_LineSpacing";
		public const string DefaultMaterialName = "m_DefaultMaterial";
		public const string FontSizeName = "m_FontSize";
		public const string TextureName = "m_Texture";
		public const string AsciiStartOffsetName = "m_AsciiStartOffset";
		public const string TrackingName = "m_Tracking";
		public const string CharacterSpacingName = "m_CharacterSpacing";
		public const string CharacterPaddingName = "m_CharacterPadding";
		public const string ConvertCaseName = "m_ConvertCase";
		public const string CharacterRectsName = "m_CharacterRects";
		public const string KerningValuesName = "m_KerningValues";
		public const string PixelScaleName = "m_PixelScale";
		public const string FontDataName = "m_FontData";
		public const string AscentName = "m_Ascent";
		public const string DescentName = "m_Descent";
		public const string DefaultStyleName = "m_DefaultStyle";
		public const string FontNamesName = "m_FontNames";
		public const string FallbackFontsName = "m_FallbackFonts";
		public const string FontRenderingModeName = "m_FontRenderingMode";
		public const string UseLegacyBoundsCalculationName = "m_UseLegacyBoundsCalculation";
		public const string ShouldRoundAdvanceValueName = "m_ShouldRoundAdvanceValue";

		public PPtr<Material> DefaultMaterial;
		public PPtr<Texture> Texture;

		private readonly Dictionary<Tuple<ushort, ushort>, float> m_kerningValues = new Dictionary<Tuple<ushort, ushort>, float>();

		private Tuple<byte, float>[] m_perCharacterKerningByte;
		private Tuple<int, float>[] m_perCharacterKerning;
		private CharacterInfo[] m_characterRects;
		private Dictionary<Tuple<byte, byte>, float> m_kerningValuesByte;
		private byte[] m_fontData;
		private string[] m_fontNames;
		private PPtr<Font>[] m_fallbackFonts;
	}
}
