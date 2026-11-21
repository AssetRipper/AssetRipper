using AsmResolver.DotNet;

namespace AssetRipper.Export.UnityProjects.Scripts;

/// <summary>
/// Unity calls <c>UnitySourceGeneratedAssemblyMonoScriptTypes_v1.Get()</c> while scanning
/// assemblies during project import. Assemblies reconstructed from IL2CPP metadata can contain
/// that type without the <c>Get</c> method, because it is editor-only and stripped from player
/// builds. Invoking the missing method crashes the Unity editor, so remove such incomplete types.
/// </summary>
public static class UnityMonoScriptInfoPatcher
{
	private const string GeneratedTypeName = "UnitySourceGeneratedAssemblyMonoScriptTypes_v1";
	private const string GeneratedDataName = "MonoScriptData";

	/// <summary>
	/// Removes <c>UnitySourceGeneratedAssemblyMonoScriptTypes_v1</c> if it cannot satisfy
	/// Unity's <c>MonoScriptData Get()</c> call.
	/// </summary>
	/// <returns>True if a type was removed, in which case the assembly must be rewritten.</returns>
	public static bool TryRemoveIncompleteGeneratedType(AssemblyDefinition assembly)
	{
		ModuleDefinition? module = assembly.ManifestModule;
		if (module is null)
		{
			return false;
		}

		TypeDefinition? incompleteType = null;
		foreach (TypeDefinition type in module.TopLevelTypes)
		{
			if (type.Name == GeneratedTypeName && !HasScriptDataMethod(type))
			{
				incompleteType = type;
				break;
			}
		}

		if (incompleteType is null)
		{
			return false;
		}

		return module.TopLevelTypes.Remove(incompleteType);
	}

	private static bool HasScriptDataMethod(TypeDefinition type)
	{
		foreach (MethodDefinition method in type.Methods)
		{
			if (method.IsStatic
				&& method.Parameters.Count == 0
				&& method.Name == "Get"
				&& method.Signature?.ReturnType is { } returnType
				&& returnType.Name == GeneratedDataName)
			{
				return true;
			}
		}
		return false;
	}
}
