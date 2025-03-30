using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Scripts;

public static class EmptyScript
{
	public static string GetContent(IMonoScript script)
	{
		return GetContent(script.Namespace.String, script.ClassName_R.String);
	}

	internal static string GetContent(MonoScriptInfo script)
	{
		return GetContent(script.Namespace, script.Class);
	}

	public static string GetContent(string? @namespace, string name)
	{
		if (MonoScriptExtensions.IsGeneric(name, out string genericName, out int genericCount))
		{
			string genericParams = string.Join(", ", Enumerable.Range(1, genericCount).Select(i => $"T{i}"));
			name = $"{genericName}<{genericParams}>";
		}
		if (string.IsNullOrEmpty(@namespace))
		{
			//Indented so that the numerical section can be easily copy-pasted when needed.
			{
				return $$"""
					using UnityEngine;

					public class {{name}} : MonoBehaviour
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
				using UnityEngine;

				namespace {{@namespace}}
				{
					public class {{name}} : MonoBehaviour
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
}
