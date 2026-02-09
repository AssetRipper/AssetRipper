using AssetRipper.AssemblyDumper.InjectedTypes;
using AssetRipper.AssemblyDumper.Types;

namespace AssetRipper.AssemblyDumper.Passes;

public static class Pass502_FixGuidAndHashYaml
{
	public static void DoPass()
	{
		foreach (TypeDefinition guidType in SharedState.Instance.SubclassGroups["GUID"].Types)
		{
			MethodDefinition toStringMethod = guidType.AddGuidToStringOverride();
		}
		TypeDefinition helperType = SharedState.Instance.InjectHelperType(typeof(HashHelper));
		foreach (TypeDefinition hashType in SharedState.Instance.SubclassGroups["Hash128"].Types)
		{
			hashType.AddHashToStringOverride(helperType);
		}
	}

	private static MethodDefinition AddGuidToStringOverride(this TypeDefinition type)
	{
		MethodDefinition method = type.AddMethod(nameof(object.ToString), MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual, type.DeclaringModule!.CorLibTypeFactory.String);
		MethodDefinition conversionMethod = type.Methods.Single(m => m.Name == "op_Implicit");
		ITypeDefOrRef commonRef = SharedState.Instance.Importer.ImportType<UnityGuid>();
		IMethodDefOrRef toStringMethod = SharedState.Instance.Importer.ImportMethod<UnityGuid>(m => m.Name == nameof(UnityGuid.ToString) && m.Parameters.Count == 0);

		CilInstructionCollection instructions = method.GetInstructions();
		CilLocalVariable local = instructions.AddLocalVariable(commonRef.ToTypeSignature());
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Call, conversionMethod);
		instructions.Add(CilOpCodes.Stloc, local);
		instructions.Add(CilOpCodes.Ldloca, local);
		instructions.Add(CilOpCodes.Call, toStringMethod);
		instructions.Add(CilOpCodes.Ret);
		instructions.OptimizeMacros();

		return method;
	}

	private static MethodDefinition AddHashToStringOverride(this TypeDefinition type, TypeDefinition helperType)
	{
		MethodDefinition method = type.AddMethod(nameof(object.ToString), MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual, type.DeclaringModule!.CorLibTypeFactory.String);
		IMethodDefOrRef helperMethod = helperType.Methods.Single(m => m.Name == nameof(HashHelper.ToString));

		CilInstructionCollection instructions = method.GetInstructions();
		instructions.AddLoadAllHashFields(type);
		instructions.Add(CilOpCodes.Call, helperMethod);
		instructions.Add(CilOpCodes.Ret);
		instructions.OptimizeMacros();

		return method;
	}

	private static void AddLoadAllHashFields(this CilInstructionCollection instructions, TypeDefinition type)
	{
		for (int i = 0; i < 16; i++)
		{
			FieldDefinition field = type.GetFieldByName(GetHashFieldName(i));
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldfld, field);
		}
	}

	private static string GetHashFieldName(int i)
	{
		return i < 10 ? $"m_Bytes__{i}" : $"m_Bytes_{i}";
	}
}
