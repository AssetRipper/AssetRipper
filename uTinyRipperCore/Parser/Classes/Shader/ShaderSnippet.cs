using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Shaders
{
	public struct ShaderSnippet : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadHardwareTierVariantsMask(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadStartLine(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool IsReadCodeHash(Version version)
		{
			return version.IsGreaterEqual(2018);
		}
		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public static bool IsReadTarget(Version version)
		{
			return version.IsLess(5, 6);
		}
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		public static bool IsReadIsGLSL(Version version)
		{
			return version.IsLess(5, 5);
		}
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadForceSyncCompilation(Version version)
		{
			return version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadLanguage(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool IsReadKeywordCombinations(Version version)
		{
			return version.IsLess(5);
		}
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadVariantsUserLocal(Version version)
		{
			return version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 5.0.0 to 5.6.0 exclusive
		/// </summary>
		public static bool IsReadTargetVariants(Version version)
		{
			return version.IsGreaterEqual(5) && version.IsLess(5, 6);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadBaseRequirements(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadNonStrippedUserKeywords(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		private static bool IsReadTargetFirst(Version version)
		{
			return version.IsLess(5);
		}

		private static int GetSerializedVersion(Version version)
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

		public void Read(AssetReader reader)
		{
			Code = reader.ReadString();
			AssetPath = reader.ReadString();
			PlatformMask = reader.ReadUInt32();
			if (IsReadHardwareTierVariantsMask(reader.Version))
			{
				HardwareTierVariantsMask = reader.ReadUInt32();
			}
			if (IsReadStartLine(reader.Version))
			{
				StartLine = reader.ReadInt32();
			}
			TypesMask = reader.ReadUInt32();
			IncludesHash.Read(reader);
			if (IsReadCodeHash(reader.Version))
			{
				CodeHash.Read(reader);
			}
			if (IsReadTarget(reader.Version))
			{
				if (IsReadTargetFirst(reader.Version))
				{
					Target = reader.ReadInt32();
				}
			}
			if (IsReadIsGLSL(reader.Version))
			{
				bool IsGLSL = reader.ReadBoolean();
				Language = IsGLSL ? 1 : 0;
			}
			FromOther = reader.ReadBoolean();
			if (IsReadForceSyncCompilation(reader.Version))
			{
				ForceSyncCompilation = reader.ReadBoolean();
			}
			reader.AlignStream(AlignType.Align4);

			if (IsReadLanguage(reader.Version))
			{
				Language = reader.ReadInt32();
			}

			if (IsReadKeywordCombinations(reader.Version))
			{
				m_keywordCombinations0 = reader.ReadStringArrayArray();
				m_keywordCombinations1 = reader.ReadStringArrayArray();
				m_keywordCombinations2 = reader.ReadStringArrayArray();
				m_keywordCombinations3 = reader.ReadStringArrayArray();
				m_keywordCombinations4 = reader.ReadStringArrayArray();
				m_keywordCombinations5 = reader.ReadStringArrayArray();
			}
			else
			{
				m_variantsUserGlobal0 = reader.ReadStringArrayArray();
				m_variantsUserGlobal1 = reader.ReadStringArrayArray();
				m_variantsUserGlobal2 = reader.ReadStringArrayArray();
				m_variantsUserGlobal3 = reader.ReadStringArrayArray();
				m_variantsUserGlobal4 = reader.ReadStringArrayArray();
				m_variantsUserGlobal5 = reader.ReadStringArrayArray();

				if (IsReadVariantsUserLocal(reader.Version))
				{
					m_variantsUserLocal0 = reader.ReadStringArrayArray();
					m_variantsUserLocal1 = reader.ReadStringArrayArray();
					m_variantsUserLocal2 = reader.ReadStringArrayArray();
					m_variantsUserLocal3 = reader.ReadStringArrayArray();
					m_variantsUserLocal4 = reader.ReadStringArrayArray();
					m_variantsUserLocal5 = reader.ReadStringArrayArray();
				}

				m_variantsBuiltin0 = reader.ReadStringArrayArray();
				m_variantsBuiltin1 = reader.ReadStringArrayArray();
				m_variantsBuiltin2 = reader.ReadStringArrayArray();
				m_variantsBuiltin3 = reader.ReadStringArrayArray();
				m_variantsBuiltin4 = reader.ReadStringArrayArray();
				m_variantsBuiltin5 = reader.ReadStringArrayArray();
			}

			if (IsReadTarget(reader.Version))
			{
				if (!IsReadTargetFirst(reader.Version))
				{
					Target = reader.ReadInt32();
				}
			}

			if (IsReadTargetVariants(reader.Version))
			{
				m_targetVariants0 = reader.ReadStringArray();
				m_targetVariants1 = reader.ReadStringArray();
				m_targetVariants2 = reader.ReadStringArray();
				m_targetVariants3 = reader.ReadStringArray();
				m_targetVariants4 = reader.ReadStringArray();
				m_targetVariants5 = reader.ReadStringArray();
			}

			if (IsReadBaseRequirements(reader.Version))
			{
				BaseRequirements = reader.ReadInt32();
				m_keywordTargetInfo = reader.ReadAssetArray<KeywordTargetInfo>();
			}
			if (IsReadNonStrippedUserKeywords(reader.Version))
			{
				NonStrippedUserKeywords = reader.ReadString();
				BuiltinKeywords = reader.ReadString();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(CodeName, Code);
			node.Add(AssetPathName, AssetPath);
			node.Add(PlatformMaskName, PlatformMask);
			node.Add(HardwareTierVariantsMaskName, HardwareTierVariantsMask);
			node.Add(StartLineName, StartLine);
			node.Add(TypesMaskName, TypesMask);
			node.Add(IncludesHashName, IncludesHash.ExportYAML(container));
			node.Add(CodeHashName, CodeHash.ExportYAML(container));
			node.Add(FromOtherName, FromOther);
			if (IsReadForceSyncCompilation(container.ExportVersion))
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
			return IsReadKeywordCombinations(version) ? new string[0][] : m_variantsUserGlobal0;
		}
		private string[][] GetVariantsUser1(Version version)
		{
			return IsReadKeywordCombinations(version) ? new string[0][] : m_variantsUserGlobal1;
		}
		private string[][] GetVariantsUser2(Version version)
		{
			return IsReadKeywordCombinations(version) ? new string[0][] : m_variantsUserGlobal2;
		}
		private string[][] GetVariantsUser3(Version version)
		{
			return IsReadKeywordCombinations(version) ? new string[0][] : m_variantsUserGlobal3;
		}
		private string[][] GetVariantsUser4(Version version)
		{
			return IsReadKeywordCombinations(version) ? new string[0][] : m_variantsUserGlobal4;
		}
		private string[][] GetVariantsUser5(Version version)
		{
			return IsReadKeywordCombinations(version) ? new string[0][] : m_variantsUserGlobal5;
		}
		private string[][] GetVariantsBuiltin0(Version version)
		{
			return IsReadKeywordCombinations(version) ? new string[0][] : m_variantsBuiltin0;
		}
		private string[][] GetVariantsBuiltin1(Version version)
		{
			return IsReadKeywordCombinations(version) ? new string[0][] : m_variantsBuiltin1;
		}
		private string[][] GetVariantsBuiltin2(Version version)
		{
			return IsReadKeywordCombinations(version) ? new string[0][] : m_variantsBuiltin2;
		}
		private string[][] GetVariantsBuiltin3(Version version)
		{
			return IsReadKeywordCombinations(version) ? new string[0][] : m_variantsBuiltin3;
		}
		private string[][] GetVariantsBuiltin4(Version version)
		{
			return IsReadKeywordCombinations(version) ? new string[0][] : m_variantsBuiltin4;
		}
		private string[][] GetVariantsBuiltin5(Version version)
		{
			return IsReadKeywordCombinations(version) ? new string[0][] : m_variantsBuiltin5;
		}

		private int GetBaseRequirements(Version version)
		{
			return IsReadBaseRequirements(version) ? BaseRequirements : 33;
		}

		private IReadOnlyList<KeywordTargetInfo> GetKeywordTargetInfo(Version version)
		{
			if (IsReadBaseRequirements(version))
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
			return IsReadNonStrippedUserKeywords(version) ? NonStrippedUserKeywords : "FOG_EXP FOG_EXP2 FOG_LINEAR";
		}

		private string GetBuiltinKeywords(Version version)
		{
			return IsReadBaseRequirements(version) ? BuiltinKeywords : string.Empty;
		}

		public string Code { get; private set; }
		public string AssetPath { get; private set; }
		public uint PlatformMask { get; private set; }
		public uint HardwareTierVariantsMask { get; private set; }
		public int StartLine { get; private set; }
		public uint TypesMask { get; private set; }
		public int Target { get; private set; }
		public bool FromOther { get; private set; }
		public bool ForceSyncCompilation { get; private set; }
		public int Language { get; private set; }

		public IReadOnlyList<IReadOnlyList<string>> KeywordCombinations0 => m_keywordCombinations0;
		public IReadOnlyList<IReadOnlyList<string>> KeywordCombinations1 => m_keywordCombinations1;
		public IReadOnlyList<IReadOnlyList<string>> KeywordCombinations2 => m_keywordCombinations2;
		public IReadOnlyList<IReadOnlyList<string>> KeywordCombinations3 => m_keywordCombinations3;
		public IReadOnlyList<IReadOnlyList<string>> KeywordCombinations4 => m_keywordCombinations4;
		public IReadOnlyList<IReadOnlyList<string>> KeywordCombinations5 => m_keywordCombinations5;
		/// <summary>
		/// VariantsUser0 previously
		/// </summary>
		public IReadOnlyList<IReadOnlyList<string>> VariantsUserGlobal0 => m_variantsUserGlobal0;
		/// <summary>
		/// VariantsUser1 previously
		/// </summary>
		public IReadOnlyList<IReadOnlyList<string>> VariantsUserGlobal1 => m_variantsUserGlobal1;
		/// <summary>
		/// VariantsUser2 previously
		/// </summary>
		public IReadOnlyList<IReadOnlyList<string>> VariantsUserGlobal2 => m_variantsUserGlobal2;
		/// <summary>
		/// VariantsUser3 previously
		/// </summary>
		public IReadOnlyList<IReadOnlyList<string>> VariantsUserGlobal3 => m_variantsUserGlobal3;
		/// <summary>
		/// VariantsUser4 previously
		/// </summary>
		public IReadOnlyList<IReadOnlyList<string>> VariantsUserGlobal4 => m_variantsUserGlobal4;
		/// <summary>
		/// VariantsUser5 previously
		/// </summary>
		public IReadOnlyList<IReadOnlyList<string>> VariantsUserGlobal5 => m_variantsUserGlobal5;
		public IReadOnlyList<IReadOnlyList<string>> VariantsUserLocal0 => m_variantsUserLocal0;
		public IReadOnlyList<IReadOnlyList<string>> VariantsUserLocal1 => m_variantsUserLocal1;
		public IReadOnlyList<IReadOnlyList<string>> VariantsUserLocal2 => m_variantsUserLocal2;
		public IReadOnlyList<IReadOnlyList<string>> VariantsUserLocal3 => m_variantsUserLocal3;
		public IReadOnlyList<IReadOnlyList<string>> VariantsUserLocal4 => m_variantsUserLocal4;
		public IReadOnlyList<IReadOnlyList<string>> VariantsUserLocal5 => m_variantsUserLocal5;
		public IReadOnlyList<IReadOnlyList<string>> VariantsBuiltin0 => m_variantsBuiltin0;
		public IReadOnlyList<IReadOnlyList<string>> VariantsBuiltin1 => m_variantsBuiltin1;
		public IReadOnlyList<IReadOnlyList<string>> VariantsBuiltin2 => m_variantsBuiltin2;
		public IReadOnlyList<IReadOnlyList<string>> VariantsBuiltin3 => m_variantsBuiltin3;
		public IReadOnlyList<IReadOnlyList<string>> VariantsBuiltin4 => m_variantsBuiltin4;
		public IReadOnlyList<IReadOnlyList<string>> VariantsBuiltin5 => m_variantsBuiltin5;
		public IReadOnlyList<string> TargetVariants0 => m_targetVariants0;
		public IReadOnlyList<string> TargetVariants1 => m_targetVariants1;
		public IReadOnlyList<string> TargetVariants2 => m_targetVariants2;
		public IReadOnlyList<string> TargetVariants3 => m_targetVariants3;
		public IReadOnlyList<string> TargetVariants4 => m_targetVariants4;
		public IReadOnlyList<string> TargetVariants5 => m_targetVariants5;
		public int BaseRequirements { get; private set; }
		public IReadOnlyList<KeywordTargetInfo> KeywordTargetInfo => m_keywordTargetInfo;
		public string NonStrippedUserKeywords { get; private set; }
		public string BuiltinKeywords { get; private set; }

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

		private string[][] m_keywordCombinations0;
		private string[][] m_keywordCombinations1;
		private string[][] m_keywordCombinations2;
		private string[][] m_keywordCombinations3;
		private string[][] m_keywordCombinations4;
		private string[][] m_keywordCombinations5;
		private string[][] m_variantsUserGlobal0;
		private string[][] m_variantsUserGlobal1;
		private string[][] m_variantsUserGlobal2;
		private string[][] m_variantsUserGlobal3;
		private string[][] m_variantsUserGlobal4;
		private string[][] m_variantsUserGlobal5;
		private string[][] m_variantsUserLocal0;
		private string[][] m_variantsUserLocal1;
		private string[][] m_variantsUserLocal2;
		private string[][] m_variantsUserLocal3;
		private string[][] m_variantsUserLocal4;
		private string[][] m_variantsUserLocal5;
		private string[][] m_variantsBuiltin0;
		private string[][] m_variantsBuiltin1;
		private string[][] m_variantsBuiltin2;
		private string[][] m_variantsBuiltin3;
		private string[][] m_variantsBuiltin4;
		private string[][] m_variantsBuiltin5;
		private string[] m_targetVariants0;
		private string[] m_targetVariants1;
		private string[] m_targetVariants2;
		private string[] m_targetVariants3;
		private string[] m_targetVariants4;
		private string[] m_targetVariants5;
		private KeywordTargetInfo[] m_keywordTargetInfo;
	}
}
