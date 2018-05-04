using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.Fonts;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
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
		/// 5.6.3 to 2017 exclusive or 2017.2.1 and greater
		/// </summary>
		public static bool IsReadUseLegacyBoundsCalculation(Version version)
		{
			return version.IsGreaterEqual(5, 6, 3) && version.IsLess(2017) || version.IsGreaterEqual(2017, 2, 1);
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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadFontImpl(stream.Version))
			{
				LineSpacing = stream.ReadSingle();
				DefaultMaterial.Read(stream);
				FontSize = stream.ReadSingle();
				Texture.Read(stream);
				stream.AlignStream(AlignType.Align4);
			}

			if (IsShortAsciiStartOffset(stream.Version))
			{
				AsciiStartOffset = stream.ReadInt16();
				FontCountX = stream.ReadInt16();
				FontCountY = stream.ReadInt16();
			}
			else
			{
				AsciiStartOffset = stream.ReadInt32();
				if (IsReadFontCount(stream.Version))
				{
					FontCountX = stream.ReadInt32();
					FontCountY = stream.ReadInt32();
				}
			}

			if (IsReadKerning(stream.Version))
			{
				Kerning = stream.ReadSingle();
			}
			if (IsReadTracking(stream.Version))
			{
				Tracking = stream.ReadSingle();
			}

			if (!IsReadFontImpl(stream.Version))
			{
				LineSpacing = stream.ReadSingle();
			}

			if (IsReadCharacterSpacing(stream.Version))
			{
				CharacterSpacing = stream.ReadInt32();
				CharacterPadding = stream.ReadInt32();
			}

			if (IsReadPerCharacterKerning(stream.Version))
			{
				if (IsBytePerCharacterKerning(stream.Version))
				{
					m_perCharacterKerningByte = stream.ReadTupleByteSingleArray();
				}
				else
				{
					m_perCharacterKerning = stream.ReadTupleIntFloatArray();
				}
			}

			ConvertCase = stream.ReadInt32();
			if (!IsReadFontImpl(stream.Version))
			{
				DefaultMaterial.Read(stream);
			}
			m_characterRects = stream.ReadArray<CharacterInfo>();
			if (!IsReadFontImpl(stream.Version))
			{
				Texture.Read(stream);
			}

			if (IsReadGridFont(stream.Version))
			{
				if (IsGridFontFirst(stream.Version))
				{
					GridFont = stream.ReadBoolean();
				}
			}

			if (IsByteKerningValues(stream.Version))
			{
				m_kerningValuesByte = new Dictionary<Tuple<byte, byte>, float>();
				m_kerningValuesByte.Read(stream);
			}
			else
			{
				m_kerningValues.Read(stream);
			}

			if (IsReadPixelScale(stream.Version))
			{
				PixelScale = stream.ReadSingle();
				stream.AlignStream(AlignType.Align4);
			}
			
			if (IsReadGridFont(stream.Version))
			{
				if (!IsGridFontFirst(stream.Version))
				{
					GridFont = stream.ReadBoolean();
					if (IsAlign(stream.Version))
					{
						stream.AlignStream(AlignType.Align4);
					}
				}
			}

			if (IsReadFontData(stream.Version))
			{
				m_fontData = stream.ReadByteArray();
				stream.AlignStream(AlignType.Align4);

				if (!IsReadFontImpl(stream.Version))
				{
					FontSize = stream.ReadSingle();
				}
				Ascent = stream.ReadSingle();
			}
			if (IsReadDescent(stream.Version))
			{
				Descent = stream.ReadSingle();
			}
			if (IsReadDefaultStyle(stream.Version))
			{
				DefaultStyle = (FontStyle)stream.ReadUInt32();
				m_fontNames = stream.ReadStringArray();
			}

			if (IsReadFallbackFonts(stream.Version))
			{
				m_fallbackFonts = stream.ReadArray<PPtr<Font>>();
				stream.AlignStream(AlignType.Align4);

				FontRenderingMode = (FontRenderingMode)stream.ReadInt32();
			}

			if (IsReadUseLegacyBoundsCalculation(stream.Version))
			{
				UseLegacyBoundsCalculation = stream.ReadBoolean();
			}
			if(IsReadShouldRoundAdvanceValue(stream.Version))
			{
				ShouldRoundAdvanceValue = stream.ReadBoolean();
			}
		}

		public override void ExportBinary(IAssetsExporter exporter, Stream stream)
		{
			if (IsReadFontData(exporter.Version))
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

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			throw new NotSupportedException();
		}

		public override string ExportExtension
		{
			get
			{
				if (m_fontData == null || m_fontData.Length == 0)
				{
					return "ttf";
				}
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
