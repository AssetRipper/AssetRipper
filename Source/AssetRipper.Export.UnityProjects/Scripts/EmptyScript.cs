using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Scripts;

public static class EmptyScript
{
	public static string GetContent(IMonoScript script)
	{
		return GetContent(script.Namespace.String, script.ClassName_R.String);
	}

	/// <summary>
	/// Generates an empty script with the correct base type when the base type is known.
	/// </summary>
	/// <param name="script">The MonoScript to generate content for.</param>
	/// <param name="baseTypeName">
	/// The fully qualified base type name (e.g. "UnityEngine.ScriptableObject", "UnityEditor.Editor").
	/// When null or empty, defaults to MonoBehaviour.
	/// </param>
	public static string GetContent(IMonoScript script, string? baseTypeName)
	{
		return GetContent(script.Namespace.String, script.ClassName_R.String, baseTypeName);
	}

	internal static string GetContent(MonoScriptInfo script)
	{
		return GetContent(script.Namespace, script.Class);
	}

	public static string GetContent(string? @namespace, string name)
	{
		return GetContent(@namespace, name, null);
	}

	public static string GetContent(string? @namespace, string name, string? baseTypeName)
	{
		if (MonoScriptExtensions.IsGeneric(name, out string genericName, out int genericCount))
		{
			string genericParams = string.Join(", ", Enumerable.Range(1, genericCount).Select(i => $"T{i}"));
			name = $"{genericName}<{genericParams}>";
		}

		ResolveBaseType(baseTypeName, out string usingDirective, out string baseClass);

		if (string.IsNullOrEmpty(@namespace))
		{
			//Indented so that the numerical section can be easily copy-pasted when needed.
			{
				return $$"""
					{{usingDirective}}

					public class {{name}} : {{baseClass}}
					{
						/*
						Dummy class. This could have happened for several reasons:
					
						1. No dll files were provided to AssetRipper.
					
							Unity asset bundles and serialized files do not contain script information to decompile.
								* For Mono games, that information is contained in .NET dll files.
								* For Il2Cpp games, that information is contained in compiled C++ assemblies and the global metadata.
								
							AssetRipper usually expects games to conform to a normal file structure for Unity games of that platform.
							A unexpected file structure could cause AssetRipper to not find the required files.
					
						2. Incorrect dll files were provided to AssetRipper.
					
							Any of the following could cause this:
								* Il2CppInterop assemblies
								* Deobfuscated assemblies
								* Older assemblies (compared to when the bundle was built)
								* Newer assemblies (compared to when the bundle was built)
					
							Note: Although assembly publicizing is bad, it alone cannot cause empty scripts. See: https://github.com/AssetRipper/AssetRipper/issues/653
					
						3. Assembly Reconstruction has not been implemented.
					
							Asset bundles contain a small amount of information about the script content.
							This information can be used to recover the serializable fields of a script.
					
							See: https://github.com/AssetRipper/AssetRipper/issues/655
					
						4. This script is unnecessary.
					
							If this script has no asset or script references, it can be deleted.
							Be sure to resolve any compile errors before deleting because they can hide references.
					
						5. Script Content Level 0
					
							AssetRipper was set to not load any script information.
					
						6. Cpp2IL failed to decompile Il2Cpp data
					
							If this happened, there will be errors in the AssetRipper.log indicating that it happened.
							This is an upstream problem, and the AssetRipper developer has very little control over it.
							Please post a GitHub issue at: https://github.com/SamboyCoding/Cpp2IL/issues
				
						7. An incorrect path was provided to AssetRipper.
				
							This is characterized by "Mixed game structure has been found at" in the AssetRipper.log file.
							AssetRipper expects games to conform to a normal file structure for Unity games of that platform.
							An unexpected file structure could cause AssetRipper to not find the required files for script decompilation.
							Generally, AssetRipper expects users to provide the root folder of the game. For example:
								* Windows: the folder containing the game's .exe file
								* Mac: the .app file/folder
								* Linux: the folder containing the game's executable file
								* Android: the apk file
								* iOS: the ipa file
								* Switch: the folder containing exefs and romfs
					
						*/
					}
					""";
			}
		}
		else
		{
			return $$"""
				{{usingDirective}}

				namespace {{@namespace}}
				{
					public class {{name}} : {{baseClass}}
					{
						/*
						Dummy class. This could have happened for several reasons:

						1. No dll files were provided to AssetRipper.

							Unity asset bundles and serialized files do not contain script information to decompile.
								* For Mono games, that information is contained in .NET dll files.
								* For Il2Cpp games, that information is contained in compiled C++ assemblies and the global metadata.
								
							AssetRipper usually expects games to conform to a normal file structure for Unity games of that platform.
							A unexpected file structure could cause AssetRipper to not find the required files.

						2. Incorrect dll files were provided to AssetRipper.

							Any of the following could cause this:
								* Il2CppInterop assemblies
								* Deobfuscated assemblies
								* Older assemblies (compared to when the bundle was built)
								* Newer assemblies (compared to when the bundle was built)

							Note: Although assembly publicizing is bad, it alone cannot cause empty scripts. See: https://github.com/AssetRipper/AssetRipper/issues/653

						3. Assembly Reconstruction has not been implemented.

							Asset bundles contain a small amount of information about the script content.
							This information can be used to recover the serializable fields of a script.

							See: https://github.com/AssetRipper/AssetRipper/issues/655
					
						4. This script is unnecessary.

							If this script has no asset or script references, it can be deleted.
							Be sure to resolve any compile errors before deleting because they can hide references.

						5. Script Content Level 0

							AssetRipper was set to not load any script information.

						6. Cpp2IL failed to decompile Il2Cpp data

							If this happened, there will be errors in the AssetRipper.log indicating that it happened.
							This is an upstream problem, and the AssetRipper developer has very little control over it.
							Please post a GitHub issue at: https://github.com/SamboyCoding/Cpp2IL/issues

						7. An incorrect path was provided to AssetRipper.

							This is characterized by "Mixed game structure has been found at" in the AssetRipper.log file.
							AssetRipper expects games to conform to a normal file structure for Unity games of that platform.
							An unexpected file structure could cause AssetRipper to not find the required files for script decompilation.
							Generally, AssetRipper expects users to provide the root folder of the game. For example:
								* Windows: the folder containing the game's .exe file
								* Mac: the .app file/folder
								* Linux: the folder containing the game's executable file
								* Android: the apk file
								* iOS: the ipa file
								* Switch: the folder containing exefs and romfs

						*/
					}
				}
				""";
		}
	}

	/// <summary>
	/// Resolves the base type name into the appropriate using directive and short class name
	/// for generating empty script stubs with the correct inheritance.
	/// </summary>
	/// <param name="baseTypeName">The fully qualified base type name, or null for default (MonoBehaviour).</param>
	/// <param name="usingDirective">The using directive(s) to include at the top of the file.</param>
	/// <param name="baseClass">The short class name to use after the colon in the class declaration.</param>
	private static void ResolveBaseType(string? baseTypeName, out string usingDirective, out string baseClass)
	{
		if (string.IsNullOrEmpty(baseTypeName))
		{
			usingDirective = "using UnityEngine;";
			baseClass = "MonoBehaviour";
			return;
		}

		// Map well-known Unity base types to their correct using directives and short names.
		// This ensures the generated empty script compiles correctly in Unity.
		switch (baseTypeName)
		{
			case "UnityEngine.ScriptableObject":
				usingDirective = "using UnityEngine;";
				baseClass = "ScriptableObject";
				return;
			case "UnityEngine.StateMachineBehaviour":
				usingDirective = "using UnityEngine;";
				baseClass = "StateMachineBehaviour";
				return;
			case "UnityEngine.NetworkBehaviour":
				usingDirective = "using UnityEngine;";
				baseClass = "NetworkBehaviour";
				return;
			case "UnityEngine.EventSystems.UIBehaviour":
				usingDirective = "using UnityEngine;\nusing UnityEngine.EventSystems;";
				baseClass = "UIBehaviour";
				return;
			case "UnityEditor.Editor":
				usingDirective = "using UnityEngine;\nusing UnityEditor;";
				baseClass = "Editor";
				return;
			case "UnityEditor.EditorWindow":
				usingDirective = "using UnityEngine;\nusing UnityEditor;";
				baseClass = "EditorWindow";
				return;
			case "UnityEditor.ScriptableWizard":
				usingDirective = "using UnityEngine;\nusing UnityEditor;";
				baseClass = "ScriptableWizard";
				return;
			case "UnityEditor.PropertyDrawer":
				usingDirective = "using UnityEngine;\nusing UnityEditor;";
				baseClass = "PropertyDrawer";
				return;
			case "UnityEditor.DecoratorDrawer":
				usingDirective = "using UnityEngine;\nusing UnityEditor;";
				baseClass = "DecoratorDrawer";
				return;
			case "UnityEditor.AssetPostprocessor":
				usingDirective = "using UnityEngine;\nusing UnityEditor;";
				baseClass = "AssetPostprocessor";
				return;
			case "UnityEditor.AssetModificationProcessor":
				usingDirective = "using UnityEngine;\nusing UnityEditor;";
				baseClass = "AssetModificationProcessor";
				return;
			default:
				// For types that inherit from known Unity base types further down the chain,
				// check if the base type itself derives from ScriptableObject or MonoBehaviour
				// by examining the namespace prefix.
				if (baseTypeName.StartsWith("UnityEditor.", StringComparison.Ordinal))
				{
					// Most UnityEditor types don't need MonoBehaviour inheritance
					string shortName = baseTypeName.Substring("UnityEditor.".Length);
					usingDirective = "using UnityEngine;\nusing UnityEditor;";
					baseClass = shortName;
					return;
				}

				// Default: assume MonoBehaviour for unknown types.
				// This is the safest default since most user scripts inherit from MonoBehaviour.
				usingDirective = "using UnityEngine;";
				baseClass = "MonoBehaviour";
				return;
		}
	}
}
