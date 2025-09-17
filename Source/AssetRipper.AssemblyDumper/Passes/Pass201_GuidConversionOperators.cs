using AssetRipper.AssemblyDumper.Methods;

namespace AssetRipper.AssemblyDumper.Passes;

public static class Pass201_GuidConversionOperators
{
	const MethodAttributes ConversionAttributes = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
	public static void DoPass()
	{
		foreach (TypeDefinition type in SharedState.Instance.SubclassGroups["GUID"].Types)
		{
			AddImplicitConversion(type);
			AddExplicitConversion(type);
		}
	}

	private static void AddImplicitConversion(TypeDefinition guidType)
	{
		ITypeDefOrRef commonGuidType = SharedState.Instance.Importer.ImportType<UnityGuid>();
		IMethodDefOrRef constructor = SharedState.Instance.Importer.ImportConstructor<UnityGuid>(4);

		FieldDefinition data0 = guidType.Fields.Single(field => field.Name == "m_Data_0_");
		FieldDefinition data1 = guidType.Fields.Single(field => field.Name == "m_Data_1_");
		FieldDefinition data2 = guidType.Fields.Single(field => field.Name == "m_Data_2_");
		FieldDefinition data3 = guidType.Fields.Single(field => field.Name == "m_Data_3_");

		MethodDefinition implicitMethod = guidType.AddMethod("op_Implicit", ConversionAttributes, commonGuidType.ToTypeSignature());
		implicitMethod.AddParameter(guidType.ToTypeSignature(), "value");

		implicitMethod.CilMethodBody!.InitializeLocals = true;
		CilInstructionCollection instructions = implicitMethod.CilMethodBody.Instructions;

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldfld, data0);
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldfld, data1);
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldfld, data2);
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldfld, data3);
		instructions.Add(CilOpCodes.Newobj, constructor);
		instructions.Add(CilOpCodes.Ret);
	}

	private static void AddExplicitConversion(TypeDefinition guidType)
	{
		ITypeDefOrRef commonGuidType = SharedState.Instance.Importer.ImportType<UnityGuid>();
		IMethodDefOrRef constructor = guidType.Methods.Single(m => m.IsConstructor && m.Parameters.Count == 0 && !m.IsStatic);

		FieldDefinition data0 = guidType.Fields.Single(field => field.Name == "m_Data_0_");
		FieldDefinition data1 = guidType.Fields.Single(field => field.Name == "m_Data_1_");
		FieldDefinition data2 = guidType.Fields.Single(field => field.Name == "m_Data_2_");
		FieldDefinition data3 = guidType.Fields.Single(field => field.Name == "m_Data_3_");

		IMethodDefOrRef getData0 = SharedState.Instance.Importer.ImportMethod<UnityGuid>(m => m.Name == $"get_{nameof(UnityGuid.Data0)}");
		IMethodDefOrRef getData1 = SharedState.Instance.Importer.ImportMethod<UnityGuid>(m => m.Name == $"get_{nameof(UnityGuid.Data1)}");
		IMethodDefOrRef getData2 = SharedState.Instance.Importer.ImportMethod<UnityGuid>(m => m.Name == $"get_{nameof(UnityGuid.Data2)}");
		IMethodDefOrRef getData3 = SharedState.Instance.Importer.ImportMethod<UnityGuid>(m => m.Name == $"get_{nameof(UnityGuid.Data3)}");

		MethodDefinition explicitMethod = guidType.AddMethod("op_Explicit", ConversionAttributes, guidType.ToTypeSignature());
		Parameter parameter = explicitMethod.AddParameter(commonGuidType.ToTypeSignature(), "value");

		CilInstructionCollection instructions = explicitMethod.CilMethodBody!.Instructions;

		instructions.Add(CilOpCodes.Newobj, constructor);
		instructions.Add(CilOpCodes.Dup);
		instructions.Add(CilOpCodes.Ldarga, parameter);
		instructions.Add(CilOpCodes.Call, getData0);
		instructions.Add(CilOpCodes.Stfld, data0);
		instructions.Add(CilOpCodes.Dup);
		instructions.Add(CilOpCodes.Ldarga, parameter);
		instructions.Add(CilOpCodes.Call, getData1);
		instructions.Add(CilOpCodes.Stfld, data1);
		instructions.Add(CilOpCodes.Dup);
		instructions.Add(CilOpCodes.Ldarga, parameter);
		instructions.Add(CilOpCodes.Call, getData2);
		instructions.Add(CilOpCodes.Stfld, data2);
		instructions.Add(CilOpCodes.Dup);
		instructions.Add(CilOpCodes.Ldarga, parameter);
		instructions.Add(CilOpCodes.Call, getData3);
		instructions.Add(CilOpCodes.Stfld, data3);
		instructions.Add(CilOpCodes.Ret);
	}
}
