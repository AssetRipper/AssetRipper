namespace AssetRipper.SerializationLogic.Extensions;

internal static class TypeDefinitionExtensions
{
	public static bool InheritsFromMonoBehaviour(this TypeDefinition type, RuntimeContext? runtimeContext)
	{
		return type.InheritsFrom(EngineTypePredicates.UnityEngineNamespace, EngineTypePredicates.MonoBehaviour, runtimeContext);
	}

	public static bool InheritsFromScriptableObject(this TypeDefinition type, RuntimeContext? runtimeContext)
	{
		return type.InheritsFrom(EngineTypePredicates.UnityEngineNamespace, EngineTypePredicates.ScriptableObject, runtimeContext);
	}

	public static bool InheritsFromObject(this TypeDefinition type, RuntimeContext? runtimeContext)
	{
		return type.InheritsFrom(EngineTypePredicates.UnityEngineNamespace, "Object", runtimeContext);
	}

	public static bool TryGetPrimitiveType(this TypeDefinition typeDefinition, out PrimitiveType primitiveType)
	{
		if ((typeDefinition.DeclaringModule?.Assembly?.IsCorLib() ?? false) && typeDefinition.ToTypeSignature() is CorLibTypeSignature corLibTypeSignature)
		{
			primitiveType = corLibTypeSignature.ToPrimitiveType();
			return primitiveType.IsCSharpPrimitive();
		}
		else
		{
			primitiveType = PrimitiveType.Complex;
			return false;
		}
	}

	private static bool IsCorLib(this AssemblyDefinition assembly)
	{
		return assembly.Name == "mscorlib" || assembly.Name == "System.Runtime";
	}
}
