using AsmResolver.DotNet;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// Improves decompilation of obfuscated assemblies.
/// </summary>
public sealed class ObfuscationRepairProcessor : IAssetProcessor
{
	public void Process(GameData gameData) => Process(gameData.AssemblyManager);
	private static void Process(IAssemblyManager manager)
	{
		manager.ClearStreamCache();

		RemoveCompilerGeneratedAttributesFromSpeakableTypes(manager);
	}

	/// <summary>
	/// Removes compiler-generated attributes from types with speakable names.
	/// This prevents a decompiler from assuming that the type follows compiler-generated conventions
	/// when in fact the obfuscator may have modified it in a way that breaks those conventions.
	/// </summary>
	private static void RemoveCompilerGeneratedAttributesFromSpeakableTypes(IAssemblyManager manager)
	{
		foreach (TypeDefinition type in manager.GetAssemblies().SelectMany(a => a.Modules).SelectMany(m => m.GetAllTypes()))
		{
			string? name = type.Name;
			if (name is null || (name.Contains('<') && name.Contains('>')))
			{
				// Unspeakable name from the compiler. We should leave this type alone.
				continue;
			}

			for (int i = type.CustomAttributes.Count - 1; i >= 0; i--)
			{
				if (type.CustomAttributes[i].IsCompilerGeneratedAttribute())
				{
					type.CustomAttributes.RemoveAt(i);
				}
			}
		}
	}
}
