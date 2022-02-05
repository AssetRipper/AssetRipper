using AssetRipper.Core.Parser.Files;
using ICSharpCode.Decompiler.CSharp;
using System;

namespace AssetRipper.Library.Configuration
{
	public enum ScriptLanguageVersion
	{
		AutoExperimental = -2,
		AutoSafe = -1,
		CSharp1 = 1,
		CSharp2 = 2,
		CSharp3 = 3,
		CSharp4 = 4,
		CSharp5 = 5,
		CSharp6 = 6,
		CSharp7 = 7,
		CSharp7_1 = 701,
		CSharp7_2 = 702,
		CSharp7_3 = 703,
		CSharp8_0 = 800,
		CSharp9_0 = 900,
		CSharp10_0 = 1000,
		Latest = int.MaxValue
	}

	public static class ScriptLanguageVersionExtensions
	{
		public static LanguageVersion ToCSharpLanguageVersion(this ScriptLanguageVersion scriptLanguageVersion, UnityVersion unityVersion)
		{
			return scriptLanguageVersion switch
			{
				ScriptLanguageVersion.AutoExperimental => GetAutomaticCSharpLanguageVersion(unityVersion, true),
				ScriptLanguageVersion.AutoSafe => GetAutomaticCSharpLanguageVersion(unityVersion, false),
				ScriptLanguageVersion.CSharp1 => LanguageVersion.CSharp1,
				ScriptLanguageVersion.CSharp2 => LanguageVersion.CSharp2,
				ScriptLanguageVersion.CSharp3 => LanguageVersion.CSharp3,
				ScriptLanguageVersion.CSharp4 => LanguageVersion.CSharp4,
				ScriptLanguageVersion.CSharp5 => LanguageVersion.CSharp5,
				ScriptLanguageVersion.CSharp6 => LanguageVersion.CSharp6,
				ScriptLanguageVersion.CSharp7 => LanguageVersion.CSharp7,
				ScriptLanguageVersion.CSharp7_1 => LanguageVersion.CSharp7_1,
				ScriptLanguageVersion.CSharp7_2 => LanguageVersion.CSharp7_2,
				ScriptLanguageVersion.CSharp7_3 => LanguageVersion.CSharp7_3,
				ScriptLanguageVersion.CSharp8_0 => LanguageVersion.CSharp8_0,
				ScriptLanguageVersion.CSharp9_0 => LanguageVersion.CSharp9_0,
				ScriptLanguageVersion.CSharp10_0 => LanguageVersion.CSharp10_0,
				ScriptLanguageVersion.Latest => LanguageVersion.Latest,
				_ => throw new ArgumentOutOfRangeException(nameof(scriptLanguageVersion), $"{scriptLanguageVersion}"),
			};
		}

		private static LanguageVersion GetAutomaticCSharpLanguageVersion(UnityVersion unityVersion, bool experimental)
		{
			if (unityVersion.IsGreaterEqual(2021, 2))
				return LanguageVersion.CSharp9_0;
			else if (unityVersion.IsGreaterEqual(2020, 2))
				return LanguageVersion.CSharp8_0;
			else if (unityVersion.IsGreaterEqual(2019, 2))
				return LanguageVersion.CSharp7_3;
			else if (experimental && unityVersion.IsGreaterEqual(2018, 3))
				return LanguageVersion.CSharp7_3;
			else if (experimental && unityVersion.IsGreaterEqual(2017, 1))
				return LanguageVersion.CSharp6;
			else
				return LanguageVersion.CSharp4;
		}
	}
}
