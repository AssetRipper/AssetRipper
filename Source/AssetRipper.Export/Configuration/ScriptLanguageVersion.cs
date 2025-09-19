using AssetRipper.Primitives;
using ICSharpCode.Decompiler.CSharp;

namespace AssetRipper.Export.Configuration;

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
	CSharp11_0 = 1100,
	CSharp12_0 = 1200,
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
			ScriptLanguageVersion.CSharp11_0 => LanguageVersion.CSharp11_0,
			ScriptLanguageVersion.CSharp12_0 => LanguageVersion.CSharp12_0,
			ScriptLanguageVersion.Latest => LanguageVersion.Latest,
			_ => throw new ArgumentOutOfRangeException(nameof(scriptLanguageVersion), $"{scriptLanguageVersion}"),
		};
	}

	private static LanguageVersion GetAutomaticCSharpLanguageVersion(UnityVersion unityVersion, bool experimental)
	{
		if (HasCSharp9Support(unityVersion))
		{
			return LanguageVersion.CSharp9_0;
		}
		else if (unityVersion.GreaterThanOrEquals(2020, 2))
		{
			return LanguageVersion.CSharp8_0;
		}
		else if (unityVersion.GreaterThanOrEquals(2019, 2))
		{
			return LanguageVersion.CSharp7_3;
		}
		//.NET Standard 2.0 support was added in 2018.1,
		//But Roslyn and C# 7.3 weren't added until 2018.3.
		//https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#net-standard-versions
		else if (experimental && unityVersion.GreaterThanOrEquals(2018, 3))
		{
			//https://blog.unity.com/technology/introducing-unity-2018-3
			return LanguageVersion.CSharp7_3;
		}
		else if (experimental && unityVersion.GreaterThanOrEquals(2017, 1))
		{
			return LanguageVersion.CSharp6;
		}
		else
		{
			return LanguageVersion.CSharp4;
		}
	}

	/// <summary>
	/// Added in 2021.2.0b6 and 2022.1.0a3
	/// </summary>
	/// <remarks>
	/// These are also the versions that introduced .NET Standard 2.1 support.<br/>
	/// Despite being part of C# 8, support for
	/// <see href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/default-interface-methods">default interface methods</see>
	/// was added in this language support expansion. It was unavailable prior to these versions.<br/>
	/// Despite being part of C# 9, support for
	/// <see href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/covariant-returns">covariant return types</see>
	/// was not included in this language support expansion, nor any other C# 9 features requiring runtime support.<br/>
	/// <see href="https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-1#net-standard-versions"/><br/>
	/// <see href="https://forum.unity.com/threads/unity-future-net-development-status.1092205/"/>
	/// </remarks>
	/// <param name="unityVersion"></param>
	/// <returns></returns>
	private static bool HasCSharp9Support(UnityVersion unityVersion)
	{
		return unityVersion.GreaterThanOrEquals(2022, 1, 0, UnityVersionType.Alpha, 3)
			|| unityVersion.GreaterThanOrEquals(2021, 2, 0, UnityVersionType.Beta, 6) && unityVersion.Equals(2021);
	}
}
