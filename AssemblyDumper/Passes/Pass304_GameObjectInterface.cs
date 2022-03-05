using AssemblyDumper.Utils;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.GameObject;

namespace AssemblyDumper.Passes
{
	public static class Pass304_GameObjectInterface
	{
		const MethodAttributes InterfacePropertyImplementationAttributes =
			InterfaceMethodImplementationAttributes | 
			MethodAttributes.SpecialName; 
		const MethodAttributes InterfaceMethodImplementationAttributes =
			MethodAttributes.Public |
			MethodAttributes.Final |
			MethodAttributes.HideBySig |
			MethodAttributes.NewSlot |
			MethodAttributes.Virtual;
		public static void DoPass()
		{
			Console.WriteLine("Pass 304: GameObject Interface");
			if (!SharedState.TypeDictionary.TryGetValue("GameObject", out TypeDefinition? type))
			{
				throw new Exception("GameObject not found");
			}
			else
			{
				ITypeDefOrRef gameObjectInterface = SharedState.Importer.ImportCommonType<IGameObject>();
				type.Interfaces.Add(new InterfaceImplementation(gameObjectInterface));

				type.ImplementFullProperty("Tag", InterfacePropertyImplementationAttributes, SystemTypeGetter.UInt16, type.GetFieldByName("m_Tag"));
				type.ImplementStringProperty("TagString", InterfacePropertyImplementationAttributes, type.GetFieldByName("m_TagString"));
				type.ImplementFullProperty("IsActive", InterfacePropertyImplementationAttributes, SystemTypeGetter.Boolean, type.GetFieldByName("m_IsActive"));
				type.ImplementFullProperty("Layer", InterfacePropertyImplementationAttributes, SystemTypeGetter.UInt32, type.GetFieldByName("m_Layer"));

				ITypeDefOrRef commonPPtrType = SharedState.Importer.ImportCommonType("AssetRipper.Core.Classes.Misc.PPtr`1");
				ITypeDefOrRef componentInterface = SharedState.Importer.ImportCommonType<IComponent>();
				GenericInstanceTypeSignature componentPPtrType = commonPPtrType.MakeGenericInstanceType(componentInterface.ToTypeSignature());
				SzArrayTypeSignature componentPPtrArray = componentPPtrType.MakeSzArrayType();

				MethodDefinition explicitConversionMethod = PPtrUtils.GetExplicitConversion<IComponent>(SharedState.TypeDictionary["PPtr_Component_"]);

				FieldDefinition field = type.GetFieldByName("m_Component");
				SzArrayTypeSignature fieldType = (SzArrayTypeSignature)field.Signature!.FieldType;
				TypeSignature elementType = fieldType.BaseType;
				MethodDefinition fetchComponentsMethod = type.AddMethod("FetchComponents", InterfaceMethodImplementationAttributes, componentPPtrArray);
				fetchComponentsMethod.CilMethodBody!.InitializeLocals = true;
				CilInstructionCollection processor = fetchComponentsMethod.CilMethodBody.Instructions;

				//Make local and store length in it
				CilLocalVariable? countLocal = new CilLocalVariable(SystemTypeGetter.Int32); //Create local
				processor.Owner.LocalVariables.Add(countLocal); //Add to method
				processor.Add(CilOpCodes.Ldarg_0); //Load this
				processor.Add(CilOpCodes.Ldfld, field); //Load array
				processor.Add(CilOpCodes.Ldlen); //Get length
				processor.Add(CilOpCodes.Stloc, countLocal); //Store it

				//Create empty array and local for it
				processor.Add(CilOpCodes.Ldloc, countLocal); //Load count
				processor.Add(CilOpCodes.Newarr, componentPPtrType.ToTypeDefOrRef()); //Create new array of kvp with given count
				CilLocalVariable? arrayLocal = new CilLocalVariable(componentPPtrArray); //Create local
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
				processor.Add(CilOpCodes.Ldelem, elementType.ToTypeDefOrRef()); //Store in array
				if (elementType.Name == "ComponentPair")
				{
					FieldDefinition pptrField = elementType.Resolve()!.GetFieldByName("component");
					processor.Add(CilOpCodes.Ldfld, pptrField);
				}
				else
				{
					IMethodDefOrRef nullableGetValue = MethodUtils.MakeMethodOnGenericType((GenericInstanceTypeSignature)elementType, "get_Value");
					processor.Add(CilOpCodes.Call, nullableGetValue);
				}
				
				processor.Add(CilOpCodes.Call, explicitConversionMethod);
				processor.Add(CilOpCodes.Stelem, componentPPtrType.ToTypeDefOrRef()); //Store in array

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
		}
	}
}
