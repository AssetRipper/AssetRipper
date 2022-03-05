using AssemblyDumper.Utils;

namespace AssemblyDumper.Passes
{
	public static class Pass204_Hash128ImplicitConversion
	{
		const MethodAttributes ConversionAttributes = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
		public static void DoPass()
		{
			Console.WriteLine("Pass 204: Hash128 Implicit Conversion");
			if (SharedState.TypeDictionary.TryGetValue("Hash128", out TypeDefinition? type))
			{
				type.AddConversion();
			}
		}

		private static void AddConversion(this TypeDefinition type)
		{
			ITypeDefOrRef returnType = SharedState.Importer.ImportCommonType<AssetRipper.Core.Classes.Misc.Hash128>();
			MethodDefinition method = type.AddMethod("op_Implicit", ConversionAttributes, returnType);
			method.AddParameter("value", type);
			method.CilMethodBody!.InitializeLocals = true;

			CilInstructionCollection processor = method.CilMethodBody.Instructions;
			SzArrayTypeSignature arrayType = SystemTypeGetter.UInt8.MakeSzArrayType();

			processor.Add(CilOpCodes.Ldc_I4, 16);
			processor.Add(CilOpCodes.Newarr, SystemTypeGetter.UInt8.ToTypeDefOrRef());

			CilLocalVariable array = new CilLocalVariable(arrayType);
			processor.Owner.LocalVariables.Add(array);
			processor.Add(CilOpCodes.Stloc, array);

			for(int i = 0; i < 16; i++)
			{
				FieldDefinition field = type.GetFieldByName($"bytes_{i}_");
				processor.Add(CilOpCodes.Ldloc, array);
				processor.Add(CilOpCodes.Ldc_I4, i);
				processor.Add(CilOpCodes.Ldarg_0);
				processor.Add(CilOpCodes.Ldfld, field);
				processor.Add(CilOpCodes.Stelem, SystemTypeGetter.UInt8.ToTypeDefOrRef());
			}

			processor.Add(CilOpCodes.Ldloc, array);

			IMethodDefOrRef constructor = SharedState.Importer.ImportCommonMethod<AssetRipper.Core.Classes.Misc.Hash128>(m => m.IsConstructor && m.Parameters.Count == 1 && m.Parameters[0].ParameterType is SzArrayTypeSignature);
			processor.Add(CilOpCodes.Newobj, constructor);
			processor.Add(CilOpCodes.Ret);
			processor.OptimizeMacros();
		}
	}
}
