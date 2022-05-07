using System;
using System.IO;

namespace AssetRipper.Core.Parser.Utils
{
	public static class FilenameUtils
	{
		public static bool IsEngineResource(string fileName)
		{
			return IsDefaultResource(fileName) || IsEditorResource(fileName);
		}

		public static bool IsDefaultResource(string fileName)
		{
			return fileName == DefaultResourceName1 || fileName == DefaultResourceName2;
		}

		public static bool IsEditorResource(string fileName)
		{
			return fileName == EditorResourceName;
		}

		public static bool IsBuiltinExtra(string fileName)
		{
			return fileName == BuiltinExtraName1 || fileName == BuiltinExtraName2;
		}

		public static bool IsEngineGeneratedF(string fileName)
		{
			return fileName == EngineGeneratedF;
		}

		public static string FixFileIdentifier(string name)
		{
			name = name.ToLower();
			name = FixDependencyName(name);
			name = FixResourcePath(name);
			if (IsDefaultResource(name))
			{
				name = DefaultResourceName1;
			}
			else if (IsBuiltinExtra(name))
			{
				name = BuiltinExtraName1;
			}
			return name;
		}

		public static string FixDependencyName(string dependency)
		{
			if (dependency.StartsWith(LibraryFolder, StringComparison.Ordinal))
			{
				return dependency.Substring(LibraryFolder.Length);
			}
			else if (dependency.StartsWith(ResourcesFolder, StringComparison.Ordinal))
			{
				return dependency.Substring(ResourcesFolder.Length);
			}
			return dependency;
		}

		public static string FixResourcePath(string resourcePath)
		{
			const string archivePrefix = "archive:/";
			if (resourcePath.StartsWith(archivePrefix, StringComparison.Ordinal))
			{
				resourcePath = Path.GetFileName(resourcePath);
			}
			return resourcePath;
		}

		public static string FixAssemblyName(string assembly)
		{
			if (IsAssemblyIdentifier(assembly))
			{
				assembly = $"Assembly - {assembly}";
			}
			assembly = FixAssemblyEndian(assembly);
			return assembly;
		}

		public static string FixAssemblyEndian(string assembly)
		{
			if (assembly.EndsWith(AssemblyExtension, StringComparison.Ordinal))
			{
				return assembly.Substring(0, assembly.Length - AssemblyExtension.Length);
			}
			return assembly;
		}

		public static bool IsProjectAssembly(string assembly)
		{
			const string PrefixName = "Assembly";
			return assembly.StartsWith($"{PrefixName} - ", StringComparison.Ordinal) || assembly.StartsWith($"{PrefixName}-", StringComparison.Ordinal);
		}

		private static bool IsAssemblyIdentifier(string assembly)
		{
			switch (assembly)
			{
				case "Boo":
				case "Boo - first pass":
				case "CSharp":
				case "CSharp - first pass":
				case "UnityScript":
				case "UnityScript - first pass":
					return true;

				default:
					return false;
			}
		}

		public const string LibraryFolder = "library/";
		public const string ResourcesFolder = "resources/";
		public const string DefaultResourceName1 = "unity default resources";
		public const string DefaultResourceName2 = "unity_default_resources";
		public const string EditorResourceName = "unity editor resources";
		public const string BuiltinExtraName1 = "unity builtin extra";
		public const string BuiltinExtraName2 = "unity_builtin_extra";
		public const string EngineGeneratedF = "0000000000000000f000000000000000";
		public const string AssemblyExtension = ".dll";
	}
}
