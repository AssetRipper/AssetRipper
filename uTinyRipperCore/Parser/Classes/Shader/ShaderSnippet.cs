using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper.Classes.Misc;
using uTinyRipper;
using System;

namespace uTinyRipper.Classes.Shaders
{
	public struct ShaderSnippet : IAssetReadable, IYAMLExportable
	{
		public static int ToSerializedVersion(Version version)
		{
			// VariantsUsers has been renamed to VariantsUserGlobals
			if (version.IsGreaterEqual(3))
			{
				return 3;
			}
			// IsGLSL has been converted to Language
			if (version.IsGreaterEqual(5, 5))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasHardwareTierVariantsMask(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasStartLine(Version version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasCodeHash(Version version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// Less or equal to 5.6.0b1
		/// </summary>
		public static bool HasTarget(Version version) => version.IsLessEqual(5, 6, 0, VersionType.Beta, 1);
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		public static bool HasIsGLSL(Version version) => version.IsLess(5, 5);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasForceSyncCompilation(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasLanguage(Version version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasKeywordCombinations(Version version) => version.IsLess(5);
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasVariant6(Version version) => version.IsGreaterEqual(2019, 3);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasVariantsUserLocal(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 5.0.0 to 5.6.0b1
		/// </summary>
		public static bool HasTargetVariants(Version version) => version.IsGreaterEqual(5) && version.IsLessEqual(5, 6, 0, VersionType.Beta, 1);
		/// <summary>
		/// 5.6.0b2 and greater
		/// </summary>
		public static bool HasBaseRequirements(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 2);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasNonStrippedUserKeywords(Version version) => version.IsGreaterEqual(5);

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		private static bool HasTargetFirst(Version version) => version.IsLess(5);

		public void Read(AssetReader reader)
		{
			Code = reader.ReadString();
			AssetPath = reader.ReadString();
			PlatformMask = reader.ReadUInt32();
			if (HasHardwareTierVariantsMask(reader.Version))
			{
				HardwareTierVariantsMask = reader.ReadUInt32();
			}
			if (HasStartLine(reader.Version))
			{
				StartLine = reader.ReadInt32();
			}
			TypesMask = reader.ReadUInt32();
			IncludesHash.Read(reader);
			if (HasCodeHash(reader.Version))
			{
				CodeHash.Read(reader);
			}
			if (HasTarget(reader.Version))
			{
				if (HasTargetFirst(reader.Version))
				{
					Target = reader.ReadInt32();
				}
			}
			if (HasIsGLSL(reader.Version))
			{
				bool IsGLSL = reader.ReadBoolean();
				Language = IsGLSL ? 1 : 0;
			}
			FromOther = reader.ReadBoolean();
			if (HasForceSyncCompilation(reader.Version))
			{
				ForceSyncCompilation = reader.ReadBoolean();
			}
			reader.AlignStream();

			if (HasLanguage(reader.Version))
			{
				Language = reader.ReadInt32();
			}

			if (HasKeywordCombinations(reader.Version))
			{
				KeywordCombinations0 = reader.ReadStringArrayArray();
				KeywordCombinations1 = reader.ReadStringArrayArray();
				KeywordCombinations2 = reader.ReadStringArrayArray();
				KeywordCombinations3 = reader.ReadStringArrayArray();
				KeywordCombinations4 = reader.ReadStringArrayArray();
				KeywordCombinations5 = reader.ReadStringArrayArray();
			}
			else
			{
				VariantsUserGlobal0 = reader.ReadStringArrayArray();
				VariantsUserGlobal1 = reader.ReadStringArrayArray();
				VariantsUserGlobal2 = reader.ReadStringArrayArray();
				VariantsUserGlobal3 = reader.ReadStringArrayArray();
				VariantsUserGlobal4 = reader.ReadStringArrayArray();
				VariantsUserGlobal5 = reader.ReadStringArrayArray();
				if (HasVariant6(reader.Version))
				{
					VariantsUserGlobal6 = reader.ReadStringArrayArray();
				}

				if (HasVariantsUserLocal(reader.Version))
				{
					VariantsUserLocal0 = reader.ReadStringArrayArray();
					VariantsUserLocal1 = reader.ReadStringArrayArray();
					VariantsUserLocal2 = reader.ReadStringArrayArray();
					VariantsUserLocal3 = reader.ReadStringArrayArray();
					VariantsUserLocal4 = reader.ReadStringArrayArray();
					VariantsUserLocal5 = reader.ReadStringArrayArray();
					if (HasVariant6(reader.Version))
					{
						VariantsUserLocal6 = reader.ReadStringArrayArray();
					}
				}

				VariantsBuiltin0 = reader.ReadStringArrayArray();
				VariantsBuiltin1 = reader.ReadStringArrayArray();
				VariantsBuiltin2 = reader.ReadStringArrayArray();
				VariantsBuiltin3 = reader.ReadStringArrayArray();
				VariantsBuiltin4 = reader.ReadStringArrayArray();
				VariantsBuiltin5 = reader.ReadStringArrayArray();
				if (HasVariant6(reader.Version))
				{
					VariantsBuiltin6 = reader.ReadStringArrayArray();
				}
			}

			if (HasTarget(reader.Version))
			{
				if (!HasTargetFirst(reader.Version))
				{
					Target = reader.ReadInt32();
				}
			}

			if (HasTargetVariants(reader.Version))
			{
				TargetVariants0 = reader.ReadStringArray();
				TargetVariants1 = reader.ReadStringArray();
				TargetVariants2 = reader.ReadStringArray();
				TargetVariants3 = reader.ReadStringArray();
				TargetVariants4 = reader.ReadStringArray();
				TargetVariants5 = reader.ReadStringArray();
			}

			if (HasBaseRequirements(reader.Version))
			{
				BaseRequirements = reader.ReadInt32();
				KeywordTargetInfo = reader.ReadAssetArray<KeywordTargetInfo>();
			}
			if (HasNonStrippedUserKeywords(reader.Version))
			{
				NonStrippedUserKeywords = reader.ReadString();
				BuiltinKeywords = reader.ReadString();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(CodeName, Code);
			node.Add(AssetPathName, AssetPath);
			node.Add(PlatformMaskName, PlatformMask);
			node.Add(HardwareTierVariantsMaskName, HardwareTierVariantsMask);
			node.Add(StartLineName, StartLine);
			node.Add(TypesMaskName, TypesMask);
			node.Add(IncludesHashName, IncludesHash.ExportYAML(container));
			node.Add(CodeHashName, CodeHash.ExportYAML(container));
			node.Add(FromOtherName, FromOther);
			if (HasForceSyncCompilation(container.ExportVersion))
			{
				node.Add(ForceSyncCompilationName, ForceSyncCompilation);
			}
			node.Add(LanguageName, Language);
			node.Add(VariantsUser0Name, GetVariantsUser0(container.Version).ExportYAML());
			node.Add(VariantsUser1Name, GetVariantsUser1(container.Version).ExportYAML());
			node.Add(VariantsUser2Name, GetVariantsUser2(container.Version).ExportYAML());
			node.Add(VariantsUser3Name, GetVariantsUser3(container.Version).ExportYAML());
			node.Add(VariantsUser4Name, GetVariantsUser4(container.Version).ExportYAML());
			node.Add(VariantsUser5Name, GetVariantsUser5(container.Version).ExportYAML());
			node.Add(VariantsBuiltin0Name, GetVariantsBuiltin0(container.Version).ExportYAML());
			node.Add(VariantsBuiltin1Name, GetVariantsBuiltin1(container.Version).ExportYAML());
			node.Add(VariantsBuiltin2Name, GetVariantsBuiltin2(container.Version).ExportYAML());
			node.Add(VariantsBuiltin3Name, GetVariantsBuiltin3(container.Version).ExportYAML());
			node.Add(VariantsBuiltin4Name, GetVariantsBuiltin4(container.Version).ExportYAML());
			node.Add(VariantsBuiltin5Name, GetVariantsBuiltin5(container.Version).ExportYAML());
			node.Add(BaseRequirementsName, GetBaseRequirements(container.Version));
			node.Add(KeywordTargetInfoName, GetKeywordTargetInfo(container.Version).ExportYAML(container));
			node.Add(NonStrippedUserKeywordsName, GetNonStrippedUserKeywords(container.Version));
			node.Add(BuiltinKeywordsName, GetBuiltinKeywords(container.Version));
			return node;
		}

		private string[][] GetVariantsUser0(Version version)
		{
			return HasKeywordCombinations(version) ? Array.Empty<string[]>() : VariantsUserGlobal0;
		}
		private string[][] GetVariantsUser1(Version version)
		{
			return HasKeywordCombinations(version) ? Array.Empty<string[]>() : VariantsUserGlobal1;
		}
		private string[][] GetVariantsUser2(Version version)
		{
			return HasKeywordCombinations(version) ? Array.Empty<string[]>() : VariantsUserGlobal2;
		}
		private string[][] GetVariantsUser3(Version version)
		{
			return HasKeywordCombinations(version) ? Array.Empty<string[]>() : VariantsUserGlobal3;
		}
		private string[][] GetVariantsUser4(Version version)
		{
			return HasKeywordCombinations(version) ? Array.Empty<string[]>() : VariantsUserGlobal4;
		}
		private string[][] GetVariantsUser5(Version version)
		{
			return HasKeywordCombinations(version) ? Array.Empty<string[]>() : VariantsUserGlobal5;
		}
		private string[][] GetVariantsBuiltin0(Version version)
		{
			return HasKeywordCombinations(version) ? Array.Empty<string[]>() : VariantsBuiltin0;
		}
		private string[][] GetVariantsBuiltin1(Version version)
		{
			return HasKeywordCombinations(version) ? Array.Empty<string[]>() : VariantsBuiltin1;
		}
		private string[][] GetVariantsBuiltin2(Version version)
		{
			return HasKeywordCombinations(version) ? Array.Empty<string[]>() : VariantsBuiltin2;
		}
		private string[][] GetVariantsBuiltin3(Version version)
		{
			return HasKeywordCombinations(version) ? Array.Empty<string[]>() : VariantsBuiltin3;
		}
		private string[][] GetVariantsBuiltin4(Version version)
		{
			return HasKeywordCombinations(version) ? Array.Empty<string[]>() : VariantsBuiltin4;
		}
		private string[][] GetVariantsBuiltin5(Version version)
		{
			return HasKeywordCombinations(version) ? Array.Empty<string[]>() : VariantsBuiltin5;
		}

		private int GetBaseRequirements(Version version)
		{
			return HasBaseRequirements(version) ? BaseRequirements : 33;
		}

		private IReadOnlyList<KeywordTargetInfo> GetKeywordTargetInfo(Version version)
		{
			if (HasBaseRequirements(version))
			{
				return KeywordTargetInfo;
			}
			else
			{
				KeywordTargetInfo[] targetInfos = new KeywordTargetInfo[9];
				targetInfos[0] = new KeywordTargetInfo("SHADOWS_SOFT", 227);
				targetInfos[1] = new KeywordTargetInfo("DIRLIGHTMAP_COMBINED", 227);
				targetInfos[2] = new KeywordTargetInfo("DIRLIGHTMAP_SEPARATE", 227);
				targetInfos[3] = new KeywordTargetInfo("DYNAMICLIGHTMAP_ON", 227);
				targetInfos[4] = new KeywordTargetInfo("SHADOWS_SCREEN", 227);
				targetInfos[5] = new KeywordTargetInfo("INSTANCING_ON", 2048);
				targetInfos[6] = new KeywordTargetInfo("PROCEDURAL_INSTANCING_ON", 16384);
				targetInfos[7] = new KeywordTargetInfo("STEREO_MULTIVIEW_ON", 3819);
				targetInfos[8] = new KeywordTargetInfo("STEREO_INSTANCING_ON", 3819);
				return targetInfos;
			}
		}

		private string GetNonStrippedUserKeywords(Version version)
		{
			return HasNonStrippedUserKeywords(version) ? NonStrippedUserKeywords : "FOG_EXP FOG_EXP2 FOG_LINEAR";
		}

		private string GetBuiltinKeywords(Version version)
		{
			return HasBaseRequirements(version) ? BuiltinKeywords : string.Empty;
		}

		public string Code { get; set; }
		public string AssetPath { get; set; }
		public uint PlatformMask { get; set; }
		public uint HardwareTierVariantsMask { get; set; }
		public int StartLine { get; set; }
		public uint TypesMask { get; set; }
		public int Target { get; set; }
		public bool FromOther { get; set; }
		public bool ForceSyncCompilation { get; set; }
		public int Language { get; set; }

		public string[][] KeywordCombinations0 { get; set; }
		public string[][] KeywordCombinations1 { get; set; }
		public string[][] KeywordCombinations2 { get; set; }
		public string[][] KeywordCombinations3 { get; set; }
		public string[][] KeywordCombinations4 { get; set; }
		public string[][] KeywordCombinations5 { get; set; }
		/// <summary>
		/// VariantsUser0 previously
		/// </summary>
		public string[][] VariantsUserGlobal0 { get; set; }
		/// <summary>
		/// VariantsUser1 previously
		/// </summary>
		public string[][] VariantsUserGlobal1 { get; set; }
		/// <summary>
		/// VariantsUser2 previously
		/// </summary>
		public string[][] VariantsUserGlobal2 { get; set; }
		/// <summary>
		/// VariantsUser3 previously
		/// </summary>
		public string[][] VariantsUserGlobal3 { get; set; }
		/// <summary>
		/// VariantsUser4 previously
		/// </summary>
		public string[][] VariantsUserGlobal4 { get; set; }
		/// <summary>
		/// VariantsUser5 previously
		/// </summary>
		public string[][] VariantsUserGlobal5 { get; set; }
		public string[][] VariantsUserGlobal6 { get; set; }
		public string[][] VariantsUserLocal0 { get; set; }
		public string[][] VariantsUserLocal1 { get; set; }
		public string[][] VariantsUserLocal2 { get; set; }
		public string[][] VariantsUserLocal3 { get; set; }
		public string[][] VariantsUserLocal4 { get; set; }
		public string[][] VariantsUserLocal5 { get; set; }
		public string[][] VariantsUserLocal6 { get; set; }
		public string[][] VariantsBuiltin0 { get; set; }
		public string[][] VariantsBuiltin1 { get; set; }
		public string[][] VariantsBuiltin2 { get; set; }
		public string[][] VariantsBuiltin3 { get; set; }
		public string[][] VariantsBuiltin4 { get; set; }
		public string[][] VariantsBuiltin5 { get; set; }
		public string[][] VariantsBuiltin6 { get; set; }
		public string[] TargetVariants0 { get; set; }
		public string[] TargetVariants1 { get; set; }
		public string[] TargetVariants2 { get; set; }
		public string[] TargetVariants3 { get; set; }
		public string[] TargetVariants4 { get; set; }
		public string[] TargetVariants5 { get; set; }
		public int BaseRequirements { get; set; }
		public KeywordTargetInfo[] KeywordTargetInfo { get; set; }
		public string NonStrippedUserKeywords { get; set; }
		public string BuiltinKeywords { get; set; }

		public const string CodeName = "m_Code";
		public const string AssetPathName = "m_AssetPath";
		public const string PlatformMaskName = "m_PlatformMask";
		public const string HardwareTierVariantsMaskName = "m_HardwareTierVariantsMask";
		public const string StartLineName = "m_StartLine";
		public const string TypesMaskName = "m_TypesMask";
		public const string IncludesHashName = "m_IncludesHash";
		public const string CodeHashName = "m_CodeHash";
		public const string TargetName = "m_Target";
		public const string IsGLSLName = "m_IsGLSL";
		public const string FromOtherName = "m_FromOther";
		public const string ForceSyncCompilationName = "m_ForceSyncCompilation";
		public const string LanguageName = "m_Language";		
		public const string KeywordCombinations0Name = "m_KeywordCombinations[0]";
		public const string KeywordCombinations1Name = "m_KeywordCombinations[1]";
		public const string KeywordCombinations2Name = "m_KeywordCombinations[2]";
		public const string KeywordCombinations3Name = "m_KeywordCombinations[3]";
		public const string KeywordCombinations4Name = "m_KeywordCombinations[4]";
		public const string KeywordCombinations5Name = "m_KeywordCombinations[5]";
		public const string TargetVariants0Name = "m_TargetVariants0";
		public const string TargetVariants1Name = "m_TargetVariants1";
		public const string TargetVariants2Name = "m_TargetVariants2";
		public const string TargetVariants3Name = "m_TargetVariants3";
		public const string TargetVariants4Name = "m_TargetVariants4";
		public const string TargetVariants5Name = "m_TargetVariants5";
		public const string VariantsUser0Name = "m_VariantsUser0";
		public const string VariantsUser1Name = "m_VariantsUser1";
		public const string VariantsUser2Name = "m_VariantsUser2";
		public const string VariantsUser3Name = "m_VariantsUser3";
		public const string VariantsUser4Name = "m_VariantsUser4";
		public const string VariantsUser5Name = "m_VariantsUser5";
		public const string VariantsUserGlobal0Name = "m_VariantsUserGlobal0";
		public const string VariantsUserGlobal1Name = "m_VariantsUserGlobal1";
		public const string VariantsUserGlobal2Name = "m_VariantsUserGlobal2";
		public const string VariantsUserGlobal3Name = "m_VariantsUserGlobal3";
		public const string VariantsUserGlobal4Name = "m_VariantsUserGlobal4";
		public const string VariantsUserGlobal5Name = "m_VariantsUserGlobal5";
		public const string VariantsUserLocal0Name = "m_VariantsUserLocal0";
		public const string VariantsUserLocal1Name = "m_VariantsUserLocal1";
		public const string VariantsUserLocal2Name = "m_VariantsUserLocal2";
		public const string VariantsUserLocal3Name = "m_VariantsUserLocal3";
		public const string VariantsUserLocal4Name = "m_VariantsUserLocal4";
		public const string VariantsUserLocal5Name = "m_VariantsUserLocal5";
		public const string VariantsBuiltin0Name = "m_VariantsBuiltin0";
		public const string VariantsBuiltin1Name = "m_VariantsBuiltin1";
		public const string VariantsBuiltin2Name = "m_VariantsBuiltin2";
		public const string VariantsBuiltin3Name = "m_VariantsBuiltin3";
		public const string VariantsBuiltin4Name = "m_VariantsBuiltin4";
		public const string VariantsBuiltin5Name = "m_VariantsBuiltin5";
		public const string BaseRequirementsName = "m_BaseRequirements";
		public const string KeywordTargetInfoName = "m_KeywordTargetInfo";
		public const string NonStrippedUserKeywordsName = "m_NonStrippedUserKeywords";
		public const string BuiltinKeywordsName = "m_BuiltinKeywords";

		public Hash128 IncludesHash;
		public Hash128 CodeHash;
	}
}
