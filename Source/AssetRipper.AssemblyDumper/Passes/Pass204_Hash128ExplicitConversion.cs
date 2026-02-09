using AssetRipper.AssemblyDumper.Types;
using AssetRipper.IO.Files.BundleFiles;

namespace AssetRipper.AssemblyDumper.Passes;

public static class Pass204_Hash128ExplicitConversion
{
	const MethodAttributes ConversionAttributes = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
	public static void DoPass()
	{
		foreach (TypeDefinition type in SharedState.Instance.SubclassGroups["Hash128"].Types)
		{
			//type.AddConversion();
		}
	}

	private static void AddConversion(this TypeDefinition type)
	{
		ITypeDefOrRef returnType = SharedState.Instance.Importer.ImportType<Hash128>();
		MethodDefinition method = type.AddMethod("op_Explicit", ConversionAttributes, returnType.ToTypeSignature());
		method.AddParameter(type.ToTypeSignature(), "value");
		method.CilMethodBody!.InitializeLocals = true;

		CilInstructionCollection instructions = method.CilMethodBody.Instructions;
		SzArrayTypeSignature arrayType = SharedState.Instance.Importer.UInt8.MakeSzArrayType();

		instructions.Add(CilOpCodes.Ldc_I4, 16);
		instructions.Add(CilOpCodes.Newarr, SharedState.Instance.Importer.UInt8.ToTypeDefOrRef());

		CilLocalVariable array = instructions.AddLocalVariable(arrayType);
		instructions.Add(CilOpCodes.Stloc, array);

		for (int i = 0; i < 16; i++)
		{
			FieldDefinition field = type.GetFieldByName(GetFieldName(i));
			instructions.Add(CilOpCodes.Ldloc, array);
			instructions.Add(CilOpCodes.Ldc_I4, i);
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldfld, field);
			instructions.Add(CilOpCodes.Stelem, SharedState.Instance.Importer.UInt8.ToTypeDefOrRef());
		}

		instructions.Add(CilOpCodes.Ldloc, array);

		IMethodDefOrRef constructor = SharedState.Instance.Importer.ImportMethod<Hash128>(m => m.IsConstructor && m.Parameters.Count == 1 && m.Parameters[0].ParameterType is SzArrayTypeSignature);
		instructions.Add(CilOpCodes.Newobj, constructor);
		instructions.Add(CilOpCodes.Ret);
		instructions.OptimizeMacros();
	}

	private static string GetFieldName(int i)
	{
		return i < 10 ? $"m_Bytes__{i}" : $"m_Bytes_{i}";
	}
}
