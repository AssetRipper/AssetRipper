using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Fonts;
using uTinyRipper.Exporter.YAML;
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
		/// 5.6.5 to 2017 exclusive or 2017.2.1 and greater
		/// </summary>
		public static bool IsReadUseLegacyBoundsCalculation(Version version)
		{
			return version.IsGreaterEqual(5, 6, 5) && version.IsLess(2017) || version.IsGreaterEqual(2017, 2, 0, VersionType.Patch, 2);
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
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 5;
			}

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
			m_characterRects = reader.ReadArray<CharacterInfo>();
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
				m_fallbackFonts = reader.ReadArray<PPtr<Font>>();
				reader.AlignStream(AlignType.Align4);

				FontRenderingMode = (FontRenderingMode)reader.ReadInt32();
			}

			if (IsReadUseLegacyBoundsCalculation(reader.Version))
			{
				UseLegacyBoundsCalculation = reader.ReadBoolean();
			}
			if(IsReadShouldRoundAdvanceValue(reader.Version))
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
			foreach (Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}
			
			yield return DefaultMaterial.FetchDependency(file, isLog, ToLogString, "m_DefaultMaterial");
			yield return Texture.FetchDependency(file, isLog, ToLogString, "m_Texture");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public override bool IsValid => m_fontData != null && m_fontData.Length > 0;

		public override string ExportExtension
		{
			get
			{
				uint type = BitConverter.ToUInt32(m_fontData, 0);
				// OTTO ASCII
				if (type == 0x4F54544F)
				{
					return "otf";
				}
				return "ttf";
			}
		}

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
