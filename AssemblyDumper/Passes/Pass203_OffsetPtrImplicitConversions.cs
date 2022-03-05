using AssemblyDumper.Utils;

namespace AssemblyDumper.Passes
{
	public static class Pass203_OffsetPtrImplicitConversions
	{
		const MethodAttributes ConversionAttributes = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

		public static void DoPass()
		{
			System.Console.WriteLine("Pass 203: OffsetPtr Implicit Conversions");

			foreach ((string name, TypeDefinition type) in SharedState.TypeDictionary)
			{
				if (name.StartsWith("OffsetPtr"))
				{
					type.AddImplicitConversion();
				}
			}
		}

		private static void AddImplicitConversion(this TypeDefinition type)
		{
			FieldDefinition field = type.GetField();
			
			MethodDefinition implicitMethod = type.AddMethod("op_Implicit", ConversionAttributes, field.Signature!.FieldType);
			implicitMethod.AddParameter("value", type);

			CilInstructionCollection processor = implicitMethod.CilMethodBody!.Instructions;

			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldfld, field);
			processor.Add(CilOpCodes.Ret);
		}

		private static FieldDefinition GetField(this TypeDefinition type)
		{
			return type.Fields.Single(field => field.Name == "data");
		}
	}
}
