using System.Text.RegularExpressions;

namespace AssetRipper.Export.UnityProjects.Scripts;

public static partial class ReferenceAssemblies
{
	[GeneratedRegex(@"^Unity(\.[0-9a-zA-Z]+)*(\.dll)?$")]
	private static partial Regex UnityRegex { get; }

	[GeneratedRegex(@"^UnityEngine(\.[0-9a-zA-Z]+)*(\.dll)?$")]
	private static partial Regex UnityEngineRegex { get; }

	[GeneratedRegex(@"^UnityEditor(\.[0-9a-zA-Z]+)*(\.dll)?$")]
	private static partial Regex UnityEditorRegex { get; }

	[GeneratedRegex(@"^System(\.[0-9a-zA-Z]+)*(\.dll)?$")]
	private static partial Regex SystemRegex { get; }

	private static HashSet<string> WhitelistAssemblies { get; } =
	[
		"UnityEngine.UI.dll",
		"UnityEngine.UI",
	];

	private static HashSet<string> BlacklistAssemblies { get; } =
	[
		"mscorlib.dll",
		"mscorlib",
		"netstandard.dll",
		"netstandard",
		"Mono.Security.dll",
		"Mono.Security"
	];

	private static HashSet<string> PredefinedAssemblies { get; } =
	[
		"Assembly-CSharp.dll",
		"Assembly-CSharp",
		"Assembly-CSharp-firstpass.dll",
		"Assembly-CSharp-firstpass",
		"Assembly-CSharp-Editor.dll",
		"Assembly-CSharp-Editor",
		"Assembly-CSharp-Editor-firstpass.dll",
		"Assembly-CSharp-Editor-firstpass",
		"Assembly-UnityScript.dll",
		"Assembly-UnityScript",
		"Assembly-UnityScript-firstpass.dll",
		"Assembly-UnityScript-firstpass"
	];

	public static bool IsPredefinedAssembly(string assemblyName)
	{
		ArgumentNullException.ThrowIfNull(assemblyName);
		return PredefinedAssemblies.Contains(assemblyName);
	}

	public static bool IsReferenceAssembly(string assemblyName)
	{
		ArgumentNullException.ThrowIfNull(assemblyName);

		if (IsWhiteListAssembly(assemblyName))
		{
			return false;
		}

		return IsBlackListAssembly(assemblyName)
			|| IsUnityEngineAssembly(assemblyName)
			//|| IsUnityAssembly(assemblyName)
			|| IsSystemAssembly(assemblyName)
			|| IsUnityEditorAssembly(assemblyName);
	}

	private static bool IsUnityAssembly(string assemblyName) => UnityRegex.IsMatch(assemblyName);
	public static bool IsUnityEngineAssembly(string assemblyName) => UnityEngineRegex.IsMatch(assemblyName);
	private static bool IsUnityEditorAssembly(string assemblyName) => UnityEditorRegex.IsMatch(assemblyName);
	private static bool IsSystemAssembly(string assemblyName) => SystemRegex.IsMatch(assemblyName);
	private static bool IsWhiteListAssembly(string assemblyName) => WhitelistAssemblies.Contains(assemblyName);
	private static bool IsBlackListAssembly(string assemblyName) => BlacklistAssemblies.Contains(assemblyName);
}
