using AssemblyDumper.Utils;
using AssetRipper.Core.Classes.Misc;

namespace AssemblyDumper.Passes
{
	public static class Pass201_GuidImplicitConversion
	{
		const MethodAttributes ConversionAttributes = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
		public static void DoPass()
		{
			System.Console.WriteLine("Pass 201: GUID Implicit Conversion");
			if(SharedState.TypeDictionary.TryGetValue("GUID", out TypeDefinition? guidType))
			{
				AddImplicitConversion(guidType);
				AddExplicitConversion(guidType);
			}
		}

		private static void AddImplicitConversion(TypeDefinition guidType)
		{
			ITypeDefOrRef commonGuidType = SharedState.Importer.ImportCommonType<UnityGUID>();
			IMethodDefOrRef constructor = SharedState.Importer.ImportCommonConstructor<UnityGUID>(4);

			FieldDefinition data0 = guidType.Fields.Single(field => field.Name == "data_0_");
			FieldDefinition data1 = guidType.Fields.Single(field => field.Name == "data_1_");
			FieldDefinition data2 = guidType.Fields.Single(field => field.Name == "data_2_");
			FieldDefinition data3 = guidType.Fields.Single(field => field.Name == "data_3_");

			MethodDefinition implicitMethod = guidType.AddMethod("op_Implicit", ConversionAttributes, commonGuidType);
			implicitMethod.AddParameter("value", guidType);

			implicitMethod.CilMethodBody!.InitializeLocals = true;
			CilInstructionCollection processor = implicitMethod.CilMethodBody.Instructions;

			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldfld, data0);
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldfld, data1);
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldfld, data2);
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldfld, data3);
			processor.Add(CilOpCodes.Newobj, constructor);
			processor.Add(CilOpCodes.Ret);
		}

		private static void AddExplicitConversion(TypeDefinition guidType)
		{
			ITypeDefOrRef commonGuidType = SharedState.Importer.ImportCommonType<UnityGUID>();
			IMethodDefOrRef constructor = guidType.Methods.Single(m => m.IsConstructor && m.Parameters.Count == 0 && !m.IsStatic);

			FieldDefinition data_0_ = guidType.Fields.Single(field => field.Name == "data_0_");
			FieldDefinition data_1_ = guidType.Fields.Single(field => field.Name == "data_1_");
			FieldDefinition data_2_ = guidType.Fields.Single(field => field.Name == "data_2_");
			FieldDefinition data_3_ = guidType.Fields.Single(field => field.Name == "data_3_");

			IMethodDefOrRef getData0 = SharedState.Importer.ImportCommonMethod<UnityGUID>(m => m.Name == "get_Data0");
			IMethodDefOrRef getData1 = SharedState.Importer.ImportCommonMethod<UnityGUID>(m => m.Name == "get_Data1");
			IMethodDefOrRef getData2 = SharedState.Importer.ImportCommonMethod<UnityGUID>(m => m.Name == "get_Data2");
			IMethodDefOrRef getData3 = SharedState.Importer.ImportCommonMethod<UnityGUID>(m => m.Name == "get_Data3");

			MethodDefinition explicitMethod = guidType.AddMethod("op_Explicit", ConversionAttributes, guidType);
			Parameter parameter = explicitMethod.AddParameter("value", commonGuidType);

			CilInstructionCollection processor = explicitMethod.CilMethodBody!.Instructions;

			processor.Add(CilOpCodes.Newobj, constructor);
			processor.Add(CilOpCodes.Dup);
			processor.Add(CilOpCodes.Ldarga, parameter);
			processor.Add(CilOpCodes.Call, getData0);
			processor.Add(CilOpCodes.Stfld, data_0_);
			processor.Add(CilOpCodes.Dup);
			processor.Add(CilOpCodes.Ldarga, parameter);
			processor.Add(CilOpCodes.Call, getData1);
			processor.Add(CilOpCodes.Stfld, data_1_);
			processor.Add(CilOpCodes.Dup);
			processor.Add(CilOpCodes.Ldarga, parameter);
			processor.Add(CilOpCodes.Call, getData2);
			processor.Add(CilOpCodes.Stfld, data_2_);
			processor.Add(CilOpCodes.Dup);
			processor.Add(CilOpCodes.Ldarga, parameter);
			processor.Add(CilOpCodes.Call, getData3);
			processor.Add(CilOpCodes.Stfld, data_3_);
			processor.Add(CilOpCodes.Ret);
		}
	}
}
