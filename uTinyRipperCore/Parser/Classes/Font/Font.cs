using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.Classes.Fonts;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper;

namespace uTinyRipper.Classes
{
	public sealed class Font : NamedObject
	{
		public Font(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
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

		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool HasFontCount(Version version) => version.IsLess(4);
		/// <summary>
		/// Less than 5.3.0
		/// </summary>
		public static bool HasKerning(Version version) => version.IsLess(5, 3);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasTracking(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasCharacterSpacing(Version version) => version.IsGreaterEqual(4);
		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool HasPerCharacterKerning(Version version) => version.IsLess(4);
		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool HasGridFont(Version version) => version.IsLess(4);
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool IsBytePerCharacterKerning(Version version) => version.IsLess(2, 1);
		/// <summary>
		/// Less than 1.6.0
		/// </summary>
		public static bool IsByteKerningValues(Version version) => version.IsLess(1, 6);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasPixelScale(Version version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasFontData(Version version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasDescent(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasDefaultStyle(Version version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasFallbackFonts(Version version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 5.6.x-2017.x and greater
		/// </summary>
		public static bool HasUseLegacyBoundsCalculation(Version version)
		{
			if (version.IsGreaterEqual(2017, 3, 0, VersionType.Beta, 11))
			{
				return true;
			}
			if (version.IsGreaterEqual(2017, 2, 0, VersionType.Patch, 2))
			{
				return version.IsEqual(2017, 2);
			}
			if (version.IsGreaterEqual(2017, 1, 2, VersionType.Patch))
			{
				return version.IsEqual(2017, 1);
			}
			if (version.IsGreaterEqual(5, 6, 4, VersionType.Patch))
			{
				return version.IsEqual(5);
			}
			return false;
		}
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasShouldRoundAdvanceValue(Version version) => version.IsGreaterEqual(2018, 1);

		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		private static bool IsFontGrouped(Version version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// Less than 2.1.0
		/// </summary>
		private static bool IsShortAsciiStartOffset(Version version) => version.IsLess(2, 1);
		/// <summary>
		/// Less than 2.1.0
		/// </summary>
		private static bool IsGridFontFirst(Version version) => version.IsLess(2, 1);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		private static bool IsAlign(Version version) => version.IsGreaterEqual(3);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			bool isGrouped = IsFontGrouped(reader.Version);
			if (isGrouped)
			{
				LineSpacing = reader.ReadSingle();
				DefaultMaterial.Read(reader);
				FontSize = reader.ReadSingle();
				Texture.Read(reader);
				reader.AlignStream();
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
				if (HasFontCount(reader.Version))
				{
					FontCountX = reader.ReadInt32();
					FontCountY = reader.ReadInt32();
				}
			}

			if (HasKerning(reader.Version))
			{
				Kerning = reader.ReadSingle();
			}
			if (HasTracking(reader.Version))
			{
				Tracking = reader.ReadSingle();
			}

			if (!isGrouped)
			{
				LineSpacing = reader.ReadSingle();
			}

			if (HasCharacterSpacing(reader.Version))
			{
				CharacterSpacing = reader.ReadInt32();
				CharacterPadding = reader.ReadInt32();
			}

			if (HasPerCharacterKerning(reader.Version))
			{
				if (IsBytePerCharacterKerning(reader.Version))
				{
					PerCharacterKerningByte = reader.ReadTupleByteSingleArray();
				}
				else
				{
					PerCharacterKerning = reader.ReadTupleIntSingleArray();
				}
			}

			ConvertCase = reader.ReadInt32();
			if (!isGrouped)
			{
				DefaultMaterial.Read(reader);
			}
			CharacterRects = reader.ReadAssetArray<CharacterInfo>();
			if (!isGrouped)
			{
				Texture.Read(reader);
			}

			if (HasGridFont(reader.Version))
			{
				if (IsGridFontFirst(reader.Version))
				{
					GridFont = reader.ReadBoolean();
				}
			}

#warning TODO: create a dictionary with non unique keys
			if (IsByteKerningValues(reader.Version))
			{
				Dictionary<Tuple<byte, byte>, float> kerningValues = new Dictionary<Tuple<byte, byte>, float>();
				kerningValues.ReadSafe(reader);
				foreach (var kvp in kerningValues)
				{
					Tuple<ushort, ushort> key = new Tuple<ushort, ushort>(kvp.Key.Item1, kvp.Key.Item2);
					KerningValues.Add(key, kvp.Value);
				}
			}
			else
			{
				KerningValues.ReadSafe(reader);
			}

			if (HasPixelScale(reader.Version))
			{
				PixelScale = reader.ReadSingle();
				reader.AlignStream();
			}

			if (HasGridFont(reader.Version))
			{
				if (!IsGridFontFirst(reader.Version))
				{
					GridFont = reader.ReadBoolean();
					if (IsAlign(reader.Version))
					{
						reader.AlignStream();
					}
				}
			}

			if (HasFontData(reader.Version))
			{
				FontData = reader.ReadByteArray();
				reader.AlignStream();

				if (!isGrouped)
				{
					FontSize = reader.ReadSingle();
				}
				Ascent = reader.ReadSingle();
			}
			if (HasDescent(reader.Version))
			{
				Descent = reader.ReadSingle();
			}
			if (HasDefaultStyle(reader.Version))
			{
				DefaultStyle = (FontStyle)reader.ReadUInt32();
				FontNames = reader.ReadStringArray();
			}

			if (HasFallbackFonts(reader.Version))
			{
				FallbackFonts = reader.ReadAssetArray<PPtr<Font>>();
				reader.AlignStream();

				FontRenderingMode = (FontRenderingMode)reader.ReadInt32();
			}

			if (HasUseLegacyBoundsCalculation(reader.Version))
			{
				UseLegacyBoundsCalculation = reader.ReadBoolean();
			}
			if (HasShouldRoundAdvanceValue(reader.Version))
			{
				ShouldRoundAdvanceValue = reader.ReadBoolean();
			}
		}

		public override void ExportBinary(IExportContainer container, Stream stream)
		{
			if (HasFontData(container.Version))
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					writer.Write(FontData);
				}
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(DefaultMaterial, DefaultMaterialName);
			yield return context.FetchDependency(Texture, TextureName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
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
			node.Add(KerningValuesName, KerningValues.ExportYAML());
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
			return HasTracking(version) ? Tracking : 1.0f;
		}
		private int GetCharacterPadding(Version version)
		{
			return HasCharacterSpacing(version) ? CharacterPadding : 1;
		}
		private float GetPixelScale(Version version)
		{
			return HasPixelScale(version) ? PixelScale : 0.1f;
		}
		private byte[] GetFontData(Version version)
		{
			return HasFontData(version) ? FontData : Array.Empty<byte>();
		}
		private IReadOnlyList<string> GetFontNames(Version version)
		{
			return HasDefaultStyle(version) ? FontNames : new string[] { Name };
		}
		private IReadOnlyList<PPtr<Font>> GetFallbackFonts(Version version)
		{
			return HasFallbackFonts(version) ? FallbackFonts : Array.Empty<PPtr<Font>>();
		}
		private bool GetShouldRoundAdvanceValue(Version version)
		{
			return HasShouldRoundAdvanceValue(version) ? ShouldRoundAdvanceValue : true;
		}

		public bool IsValidData => FontData != null && FontData.Length > 0;

		public int AsciiStartOffset { get; set; }
		public int FontCountX { get; set; }
		public int FontCountY { get; set; }
		public float Kerning { get; set; }
		public float Tracking { get; set; }
		public float LineSpacing { get; set; }
		public int CharacterSpacing { get; set; }
		public int CharacterPadding { get; set; }
		public Tuple<byte, float>[] PerCharacterKerningByte { get; set; }
		public Tuple<int, float>[] PerCharacterKerning { get; set; }
		public int ConvertCase { get; set; }
		public CharacterInfo[] CharacterRects { get; set; }
		public bool GridFont { get; set; }
		public float PixelScale { get; set; }
		public Dictionary<Tuple<ushort, ushort>, float> KerningValues { get; set; } = new Dictionary<Tuple<ushort, ushort>, float>();
		public byte[] FontData { get; set; }
		public float FontSize { get; set; }
		public float Ascent { get; set; }
		public float Descent { get; set; }
		public FontStyle DefaultStyle {get; private set; }
		public string[] FontNames { get; set; }
		public PPtr<Font>[] FallbackFonts { get; set; }
		public FontRenderingMode FontRenderingMode { get; set; }
		public bool UseLegacyBoundsCalculation { get; set; }
		public bool ShouldRoundAdvanceValue { get; set; }

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
	}
}
