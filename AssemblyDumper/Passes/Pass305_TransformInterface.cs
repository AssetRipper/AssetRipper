using AssemblyDumper.Utils;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Math.Vectors;

namespace AssemblyDumper.Passes
{
	public static class Pass305_TransformInterface
	{
		const MethodAttributes InterfacePropertyImplementationAttributes =
			MethodAttributes.Public |
			MethodAttributes.Final |
			MethodAttributes.HideBySig |
			MethodAttributes.SpecialName |
			MethodAttributes.NewSlot |
			MethodAttributes.Virtual;

		public static void DoPass()
		{
			Console.WriteLine("Pass 305: Transform Interface");
			if (!SharedState.TypeDictionary.TryGetValue("Transform", out TypeDefinition? type))
			{
				throw new Exception("Transform not found");
			}
			else
			{
				ITypeDefOrRef transformInterface = SharedState.Importer.ImportCommonType<ITransform>();
				type.Interfaces.Add(new InterfaceImplementation(transformInterface));

				type.ImplementFatherProperty();
				type.ImplementChildrenProperty();
				type.ImplementLocalPositionProperty();
				type.ImplementLocalRotationProperty();
				type.ImplementLocalScaleProperty();
			}
		}

		private static void ImplementFatherProperty(this TypeDefinition type)
		{
			ITypeDefOrRef commonPPtrType = SharedState.Importer.ImportCommonType("AssetRipper.Core.Classes.Misc.PPtr`1");
			ITypeDefOrRef transformInterface = SharedState.Importer.ImportCommonType<ITransform>();
			GenericInstanceTypeSignature transformPPtrType = commonPPtrType.MakeGenericInstanceType(transformInterface.ToTypeSignature());
			SzArrayTypeSignature transformPPtrArray = transformPPtrType.MakeSzArrayType();

			MethodDefinition explicitConversionMethod = PPtrUtils.GetExplicitConversion<ITransform>(SharedState.TypeDictionary["PPtr_Transform_"]);

			FieldDefinition field = type.GetFieldByName("m_Father");

			PropertyDefinition property = type.AddGetterProperty(nameof(ITransform.FatherPtr), InterfacePropertyImplementationAttributes, transformPPtrType);
			CilInstructionCollection processor = property.GetMethod!.CilMethodBody!.Instructions;

			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldfld, field);
			processor.Add(CilOpCodes.Call, explicitConversionMethod);
			processor.Add(CilOpCodes.Ret);
		}

		private static void ImplementChildrenProperty(this TypeDefinition type)
		{
			ITypeDefOrRef commonPPtrType = SharedState.Importer.ImportCommonType("AssetRipper.Core.Classes.Misc.PPtr`1");
			ITypeDefOrRef transformInterface = SharedState.Importer.ImportCommonType<ITransform>();
			GenericInstanceTypeSignature transformPPtrType = commonPPtrType.MakeGenericInstanceType(transformInterface.ToTypeSignature());
			SzArrayTypeSignature transformPPtrArray = transformPPtrType.MakeSzArrayType();

			TypeDefinition elementType = SharedState.TypeDictionary["PPtr_Transform_"];

			MethodDefinition explicitConversionMethod = PPtrUtils.GetExplicitConversion<ITransform>(elementType);

			FieldDefinition field = type.GetFieldByName("m_Children");

			PropertyDefinition property = type.AddGetterProperty(nameof(ITransform.ChildrenPtrs), InterfacePropertyImplementationAttributes, transformPPtrArray);
			CilInstructionCollection processor = property.GetMethod!.CilMethodBody!.Instructions;

			//Make local and store length in it
			CilLocalVariable? countLocal = new CilLocalVariable(SystemTypeGetter.Int32); //Create local
			processor.Owner.LocalVariables.Add(countLocal); //Add to method
			processor.Add(CilOpCodes.Ldarg_0); //Load this
			processor.Add(CilOpCodes.Ldfld, field); //Load array
			processor.Add(CilOpCodes.Ldlen); //Get length
			processor.Add(CilOpCodes.Stloc, countLocal); //Store it

			//Create empty array and local for it
			processor.Add(CilOpCodes.Ldloc, countLocal); //Load count
			processor.Add(CilOpCodes.Newarr, transformPPtrType.ToTypeDefOrRef()); //Create new array of kvp with given count
			CilLocalVariable? arrayLocal = new CilLocalVariable(transformPPtrArray); //Create local
			processor.Owner.LocalVariables.Add(arrayLocal); //Add to method
			processor.Add(CilOpCodes.Stloc, arrayLocal); //Store array in local

			//Make an i
			CilLocalVariable? iLocal = new CilLocalVariable(SystemTypeGetter.Int32); //Create local
			processor.Owner.LocalVariables.Add(iLocal); //Add to method
			processor.Add(CilOpCodes.Ldc_I4_0); //Load 0 as an int32
			processor.Add(CilOpCodes.Stloc, iLocal); //Store in count

			//Create a label for a dummy instruction to jump back to
			CilInstructionLabel jumpTargetLabel = new CilInstructionLabel();
			CilInstructionLabel loopConditionStartLabel = new CilInstructionLabel();

			//Create an empty, unconditional branch which will jump down to the loop condition.
			//This converts the do..while loop into a for loop.
			CilInstruction? unconditionalBranch = processor.Add(CilOpCodes.Br, loopConditionStartLabel);

			//Now we just read pair, increment i, compare against count, and jump back to here if it's less
			jumpTargetLabel.Instruction = processor.Add(CilOpCodes.Nop); //Create a dummy instruction to jump back to

			//Read element at index i of array
			processor.Add(CilOpCodes.Ldloc, arrayLocal); //Load array local
			processor.Add(CilOpCodes.Ldloc, iLocal); //Load i local
			processor.Add(CilOpCodes.Ldarg_0); //Load this
			processor.Add(CilOpCodes.Ldfld, field); //Load array
			processor.Add(CilOpCodes.Ldloc, iLocal); //Load i local
			processor.Add(CilOpCodes.Ldelem, elementType); //Store in array
			processor.Add(CilOpCodes.Call, explicitConversionMethod);
			processor.Add(CilOpCodes.Stelem, transformPPtrType.ToTypeDefOrRef()); //Store in array

			//Increment i
			processor.Add(CilOpCodes.Ldloc, iLocal); //Load i local
			processor.Add(CilOpCodes.Ldc_I4_1); //Load constant 1 as int32
			processor.Add(CilOpCodes.Add); //Add 
			processor.Add(CilOpCodes.Stloc, iLocal); //Store in i local

			//Jump to start of loop if i < count
			loopConditionStartLabel.Instruction = processor.Add(CilOpCodes.Ldloc, iLocal); //Load i
			processor.Add(CilOpCodes.Ldloc, countLocal); //Load count
			processor.Add(CilOpCodes.Blt, jumpTargetLabel); //Jump back up if less than

			processor.Add(CilOpCodes.Ldloc, arrayLocal); //Store array in local
			processor.Add(CilOpCodes.Ret);
		}

		private static void ImplementLocalPositionProperty(this TypeDefinition type)
		{
			type.ImplementVector3Property("LocalPosition", "m_LocalPosition");
		}

		private static void ImplementLocalRotationProperty(this TypeDefinition type)
		{
			type.ImplementQuaternionProperty("LocalRotation", "m_LocalRotation");
		}

		private static void ImplementLocalScaleProperty(this TypeDefinition type)
		{
			type.ImplementVector3Property("LocalScale", "m_LocalScale");
		}

		private static void ImplementVector3Property(this TypeDefinition type, string propertyName, string fieldName)
		{
			ITypeDefOrRef commonVector3 = SharedState.Importer.ImportCommonType<Vector3f>();
			FieldDefinition field = type.GetFieldByName(fieldName);
			TypeSignature fieldType = field.Signature!.FieldType;
			TypeDefinition fieldTypeDefinition = fieldType.Resolve()!;
			PropertyDefinition property = type.AddFullProperty(propertyName, InterfacePropertyImplementationAttributes, commonVector3.ToTypeSignature());

			CilInstructionCollection getProcessor = property.GetMethod!.CilMethodBody!.Instructions;
			MethodDefinition conversionMethod = fieldTypeDefinition.Methods.Single(m => m.Name == "op_Implicit");
			getProcessor.Add(CilOpCodes.Ldarg_0);
			getProcessor.Add(CilOpCodes.Ldfld, field);
			getProcessor.Add(CilOpCodes.Call, conversionMethod);
			getProcessor.Add(CilOpCodes.Ret);

			CilInstructionCollection setProcessor = property.SetMethod!.CilMethodBody!.Instructions;

			IMethodDefOrRef? commonX = SharedState.Importer.ImportMethod(commonVector3.Resolve()!.Properties.Single(m => m.Name == "X").GetMethod!);
			FieldDefinition? specificX = fieldTypeDefinition.Fields.Single(m => m.Name == "x");
			setProcessor.Add(CilOpCodes.Ldarg_0);
			setProcessor.Add(CilOpCodes.Ldfld, field);
			setProcessor.Add(CilOpCodes.Ldarg_1);
			setProcessor.Add(CilOpCodes.Call, commonX);
			setProcessor.Add(CilOpCodes.Stfld, specificX);

			IMethodDefOrRef? commonY = SharedState.Importer.ImportMethod(commonVector3.Resolve()!.Properties.Single(m => m.Name == "Y").GetMethod!);
			FieldDefinition? specificY = fieldTypeDefinition.Fields.Single(m => m.Name == "y");
			setProcessor.Add(CilOpCodes.Ldarg_0);
			setProcessor.Add(CilOpCodes.Ldfld, field);
			setProcessor.Add(CilOpCodes.Ldarg_1);
			setProcessor.Add(CilOpCodes.Call, commonY);
			setProcessor.Add(CilOpCodes.Stfld, specificY);

			IMethodDefOrRef? commonZ = SharedState.Importer.ImportMethod(commonVector3.Resolve()!.Properties.Single(m => m.Name == "Z").GetMethod!);
			FieldDefinition? specificZ = fieldTypeDefinition.Fields.Single(m => m.Name == "z");
			setProcessor.Add(CilOpCodes.Ldarg_0);
			setProcessor.Add(CilOpCodes.Ldfld, field);
			setProcessor.Add(CilOpCodes.Ldarg_1);
			setProcessor.Add(CilOpCodes.Call, commonZ);
			setProcessor.Add(CilOpCodes.Stfld, specificZ);

			setProcessor.Add(CilOpCodes.Ret);
			setProcessor.OptimizeMacros();
		}

		private static void ImplementQuaternionProperty(this TypeDefinition type, string propertyName, string fieldName)
		{
			ITypeDefOrRef commonVector3 = SharedState.Importer.ImportCommonType<Quaternionf>();
			FieldDefinition field = type.GetFieldByName(fieldName);
			TypeSignature fieldType = field.Signature!.FieldType;
			TypeDefinition fieldTypeDefinition = fieldType.Resolve()!;
			PropertyDefinition property = type.AddFullProperty(propertyName, InterfacePropertyImplementationAttributes, commonVector3.ToTypeSignature());

			CilInstructionCollection getProcessor = property.GetMethod!.CilMethodBody!.Instructions;
			MethodDefinition conversionMethod = fieldTypeDefinition.Methods.Single(m => m.Name == "op_Implicit");
			getProcessor.Add(CilOpCodes.Ldarg_0);
			getProcessor.Add(CilOpCodes.Ldfld, field);
			getProcessor.Add(CilOpCodes.Call, conversionMethod);
			getProcessor.Add(CilOpCodes.Ret);

			CilInstructionCollection setProcessor = property.SetMethod!.CilMethodBody!.Instructions;

			IMethodDefOrRef? commonX = SharedState.Importer.ImportMethod(commonVector3.Resolve()!.Properties.Single(m => m.Name == "X").GetMethod!);
			FieldDefinition? specificX = fieldTypeDefinition.Fields.Single(m => m.Name == "x");
			setProcessor.Add(CilOpCodes.Ldarg_0);
			setProcessor.Add(CilOpCodes.Ldfld, field);
			setProcessor.Add(CilOpCodes.Ldarg_1);
			setProcessor.Add(CilOpCodes.Call, commonX);
			setProcessor.Add(CilOpCodes.Stfld, specificX);

			IMethodDefOrRef? commonY = SharedState.Importer.ImportMethod(commonVector3.Resolve()!.Properties.Single(m => m.Name == "Y").GetMethod!);
			FieldDefinition? specificY = fieldTypeDefinition.Fields.Single(m => m.Name == "y");
			setProcessor.Add(CilOpCodes.Ldarg_0);
			setProcessor.Add(CilOpCodes.Ldfld, field);
			setProcessor.Add(CilOpCodes.Ldarg_1);
			setProcessor.Add(CilOpCodes.Call, commonY);
			setProcessor.Add(CilOpCodes.Stfld, specificY);

			IMethodDefOrRef? commonZ = SharedState.Importer.ImportMethod(commonVector3.Resolve()!.Properties.Single(m => m.Name == "Z").GetMethod!);
			FieldDefinition? specificZ = fieldTypeDefinition.Fields.Single(m => m.Name == "z");
			setProcessor.Add(CilOpCodes.Ldarg_0);
			setProcessor.Add(CilOpCodes.Ldfld, field);
			setProcessor.Add(CilOpCodes.Ldarg_1);
			setProcessor.Add(CilOpCodes.Call, commonZ);
			setProcessor.Add(CilOpCodes.Stfld, specificZ);

			IMethodDefOrRef? commonW = SharedState.Importer.ImportMethod(commonVector3.Resolve()!.Properties.Single(m => m.Name == "W").GetMethod!);
			FieldDefinition? specificW = fieldTypeDefinition.Fields.Single(m => m.Name == "w");
			setProcessor.Add(CilOpCodes.Ldarg_0);
			setProcessor.Add(CilOpCodes.Ldfld, field);
			setProcessor.Add(CilOpCodes.Ldarg_1);
			setProcessor.Add(CilOpCodes.Call, commonW);
			setProcessor.Add(CilOpCodes.Stfld, specificW);

			setProcessor.Add(CilOpCodes.Ret);
			setProcessor.OptimizeMacros();
		}
	}
}
