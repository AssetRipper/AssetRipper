using AssemblyDumper.Utils;
using AssetRipper.Core.Utils;

namespace AssemblyDumper.Passes
{
	public static class Pass030_AddArrayInitializationMethods
	{
		private const string InitializePrefix = "InitializeArray_";
		private const string LengthName = "length";

		public static void DoPass()
		{
			Console.WriteLine("Pass 030: Add Array Initialization Methods");
			IMethodDefOrRef arrayUtilsMethod = SharedState.Importer.ImportCommonMethod(
				typeof(ArrayUtils),
				m => m.Name == nameof(ArrayUtils.CreateAndInitializeArray));
			foreach(TypeDefinition type in SharedState.TypeDictionary.Values)
			{
				type.ProcessType(arrayUtilsMethod);
			}
		}

		private static void ProcessType(this TypeDefinition type, IMethodDefOrRef arrayUtilsMethod)
		{
			foreach(FieldDefinition field in type.Fields)
			{
				if(field.Signature!.FieldType is SzArrayTypeSignature fieldTypeSignature)
				{
					TypeSignature elementType = fieldTypeSignature.BaseType;
					if (!elementType.IsValueType && elementType is not SzArrayTypeSignature)
					{
						MethodSpecification initializerMethod = MethodUtils.MakeGenericInstanceMethod(arrayUtilsMethod, elementType);
						type.AddInitializeMethod(field, initializerMethod);
					}
				}
			}
		}

		private static void AddInitializeMethod(this TypeDefinition type, FieldDefinition field, MethodSpecification initializerMethod)
		{
			string methodName = InitializePrefix + field.Name;
			MethodDefinition method = type.AddMethod(methodName, MethodAttributes.Public | MethodAttributes.HideBySig, SystemTypeGetter.Void);
			method.AddParameter(LengthName, SystemTypeGetter.Int32);

			CilInstructionCollection processor = method.CilMethodBody!.Instructions;
			processor.Add(CilOpCodes.Ldarg_0);//instance
			processor.Add(CilOpCodes.Ldarg_1);//length
			processor.Add(CilOpCodes.Call, initializerMethod);
			processor.Add(CilOpCodes.Stfld, field);
			processor.Add(CilOpCodes.Ret);
		}
	}
}
