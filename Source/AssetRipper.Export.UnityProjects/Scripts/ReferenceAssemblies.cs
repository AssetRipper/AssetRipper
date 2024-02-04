using System.Text.RegularExpressions;

namespace AssetRipper.Export.UnityProjects.Scripts
{
	public static class ReferenceAssemblies
	{
		private static readonly Regex unityRegex = new Regex(@"^Unity(\.[0-9a-zA-Z]+)*(\.dll)?$", RegexOptions.Compiled);
		private static readonly Regex unityEngineRegex = new Regex(@"^UnityEngine(\.[0-9a-zA-Z]+)*(\.dll)?$", RegexOptions.Compiled);
		private static readonly Regex unityEditorRegex = new Regex(@"^UnityEditor(\.[0-9a-zA-Z]+)*(\.dll)?$", RegexOptions.Compiled);
		private static readonly Regex systemRegex = new Regex(@"^System(\.[0-9a-zA-Z]+)*(\.dll)?$", RegexOptions.Compiled);
		private static readonly HashSet<string> whitelistAssemblies = new HashSet<string>()
		{

		};
		private static readonly HashSet<string> blacklistAssemblies = new HashSet<string>()
		{
			"mscorlib.dll",
			"mscorlib",
			"netstandard.dll",
			"netstandard",
			"Mono.Security.dll",
			"Mono.Security"
		};
		private static readonly HashSet<string> predefinedAssemblies = new()
		{
			"Assembly-CSharp.dll",
			"Assembly-CSharp",
			"Assembly-CSharp-firstpass.dll",
			"Assembly-CSharp-firstpass",
			"Assembly-UnityScript.dll",
			"Assembly-UnityScript",
			"Assembly-UnityScript-firstpass.dll",
			"Assembly-UnityScript-firstpass"
		};

		public static bool IsPredefinedAssembly(string assemblyName)
		{
			ArgumentNullException.ThrowIfNull(assemblyName);
			return predefinedAssemblies.Contains(assemblyName);
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

		private static bool IsUnityAssembly(string assemblyName) => unityRegex.IsMatch(assemblyName);
		public static bool IsUnityEngineAssembly(string assemblyName) => unityEngineRegex.IsMatch(assemblyName);
		private static bool IsUnityEditorAssembly(string assemblyName) => unityEditorRegex.IsMatch(assemblyName);
		private static bool IsSystemAssembly(string assemblyName) => systemRegex.IsMatch(assemblyName);
		private static bool IsWhiteListAssembly(string assemblyName) => whitelistAssemblies.Contains(assemblyName);
		private static bool IsBlackListAssembly(string assemblyName) => blacklistAssemblies.Contains(assemblyName);
	}
}
