﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AssetRipper.Library.Exporters.Scripts
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

		public static bool IsReferenceAssembly(string assemblyName)
		{
			if (assemblyName is null)
			{
				throw new ArgumentNullException(assemblyName);
			}

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
