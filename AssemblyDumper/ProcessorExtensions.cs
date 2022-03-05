using AssemblyDumper.Utils;

namespace AssemblyDumper
{
	internal static class ProcessorExtensions
	{
		public static void AddNotSupportedException(this CilInstructionCollection processor)
		{
			processor.Add(CilOpCodes.Newobj, SystemTypeGetter.NotSupportedExceptionConstructor!);
			processor.Add(CilOpCodes.Throw);
		}

		public static void AddDefaultValue(this CilInstructionCollection processor, ITypeDefOrRef targetType)
		{
			processor.AddDefaultValue(targetType.ToTypeSignature());
		}

		public static void AddDefaultValue(this CilInstructionCollection processor, TypeSignature targetType)
		{
			if (targetType.FullName == "System.Void")
			{
				//Do nothing
			}
			else if (targetType.IsValueType)
			{
				CilLocalVariable? variable = new CilLocalVariable(targetType);
				processor.Owner.LocalVariables.Add(variable);
				processor.Add(CilOpCodes.Ldloca, variable);
				processor.Add(CilOpCodes.Initobj, targetType.ToTypeDefOrRef());
				processor.Add(CilOpCodes.Ldloc, variable);
			}
			else
			{
				processor.Add(CilOpCodes.Ldnull);
			}
		}

		public static void AddLogStatement(this CilInstructionCollection processor, string text)
		{
			Func<MethodDefinition, bool> func = m => m.IsStatic && m.Name == "Info" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.Name == "String";
			IMethodDefOrRef writeMethod = SharedState.Importer.ImportCommonMethod("AssetRipper.Core.Logging.Logger", func);
			processor.Add(CilOpCodes.Ldstr, text);
			processor.Add(CilOpCodes.Call, writeMethod);
		}

		/// <summary>
		/// Remove the last instruction in the processor's collection
		/// </summary>
		/// <param name="processor">The processor to remove the instruction from</param>
		public static void Pop(this CilInstructionCollection processor) => processor.RemoveAt(processor.Count - 1);

		/// <summary>
		/// Assumes that arg_1 is int length
		/// </summary>
		/// <param name="processor"></param>
		/// <param name="field"></param>
		/// <exception cref="Exception"></exception>
		public static void AddInitializeArrayField(this CilInstructionCollection processor, FieldDefinition field)
		{
			SzArrayTypeSignature fieldType = field.Signature!.FieldType as SzArrayTypeSignature ?? throw new Exception("Field type is not an sz array");
			TypeSignature elementType = fieldType.BaseType;
			IMethodDefOrRef constructor = SharedState.Importer.ImportMethod(elementType.Resolve()!.GetDefaultConstructor());

			//Create empty array and local for it
			processor.Add(CilOpCodes.Ldarg_1); //Load length argument
			processor.Add(CilOpCodes.Newarr, elementType.ToTypeDefOrRef()); //Create new array of kvp with given count
			CilLocalVariable? arrayLocal = new CilLocalVariable(fieldType); //Create local
			processor.Owner.LocalVariables.Add(arrayLocal); //Add to method
			processor.Add(CilOpCodes.Stloc, arrayLocal); //Store array in local

			//Make local and store length in it
			CilLocalVariable? countLocal = new CilLocalVariable(SystemTypeGetter.Int32!); //Create local
			processor.Owner.LocalVariables.Add(countLocal); //Add to method
			processor.Add(CilOpCodes.Ldloc, arrayLocal); //Load array local
			processor.Add(CilOpCodes.Ldlen); //Get length
			processor.Add(CilOpCodes.Stloc, countLocal); //Store it

			//Make an i
			CilLocalVariable? iLocal = new CilLocalVariable(SystemTypeGetter.Int32!); //Create local
			processor.Owner.LocalVariables.Add(iLocal); //Add to method
			processor.Add(CilOpCodes.Ldc_I4_0); //Load 0 as an int32
			processor.Add(CilOpCodes.Stloc, iLocal); //Store in count

			//Create a label for a dummy instruction to jump back to
			CilInstructionLabel jumpTargetLabel = new CilInstructionLabel();
			CilInstructionLabel loopConditionStartLabel = new CilInstructionLabel();

			//Create an empty, unconditional branch which will jump down to the loop condition.
			//This converts the do..while loop into a for loop.
			processor.Add(CilOpCodes.Br, loopConditionStartLabel);

			//Now we just read pair, increment i, compare against count, and jump back to here if it's less
			jumpTargetLabel.Instruction = processor.Add(CilOpCodes.Nop); //Create a dummy instruction to jump back to

			//Create element at index i of array
			processor.Add(CilOpCodes.Ldloc, arrayLocal); //Load array local
			processor.Add(CilOpCodes.Ldloc, iLocal); //Load i local
			processor.Add(CilOpCodes.Newobj, constructor); //Create instance
			processor.Add(CilOpCodes.Stelem, elementType.ToTypeDefOrRef()); //Store in array

			//Increment i
			processor.Add(CilOpCodes.Ldloc, iLocal); //Load i local
			processor.Add(CilOpCodes.Ldc_I4_1); //Load constant 1 as int32
			processor.Add(CilOpCodes.Add); //Add 
			processor.Add(CilOpCodes.Stloc, iLocal); //Store in i local

			//Jump to start of loop if i < count
			loopConditionStartLabel.Instruction = processor.Add(CilOpCodes.Ldloc, iLocal); //Load i
			processor.Add(CilOpCodes.Ldloc, countLocal); //Load count
			processor.Add(CilOpCodes.Blt, jumpTargetLabel); //Jump back up if less than

			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldloc, arrayLocal); //Store array in local
			processor.Add(CilOpCodes.Stfld, field);
		}
	}
}
