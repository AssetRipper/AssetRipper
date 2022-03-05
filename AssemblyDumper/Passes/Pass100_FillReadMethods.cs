using AssemblyDumper.Unity;
using AssemblyDumper.Utils;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;

namespace AssemblyDumper.Passes
{
	public static class Pass100_FillReadMethods
	{
		private static CilInstructionLabel DummyInstructionLabel { get; } = new CilInstructionLabel();

		public static void DoPass()
		{
			Console.WriteLine("Pass 100: Filling read methods");

			foreach ((string name, UnityClass klass) in SharedState.ClassDictionary)
			{
				if (!SharedState.TypeDictionary.ContainsKey(name))
					//Skip primitive types
					continue;

				TypeDefinition? type = SharedState.TypeDictionary[name];

				MethodDefinition? editorModeReadMethod = type.Methods.First(m => m.Name == "ReadEditor");
				MethodDefinition? releaseModeReadMethod = type.Methods.First(m => m.Name == "ReadRelease");

				CilMethodBody editorModeBody = editorModeReadMethod.CilMethodBody!;
				CilMethodBody releaseModeBody = releaseModeReadMethod.CilMethodBody!;

				CilInstructionCollection editorModeProcessor = editorModeBody.Instructions;
				CilInstructionCollection releaseModeProcessor = releaseModeBody.Instructions;

				List<FieldDefinition>? fields = FieldUtils.GetAllFieldsInTypeAndBase(type);

				//Console.WriteLine($"Generating the editor read method for {name}");
				if (klass.EditorRootNode != null)
				{
					foreach (UnityNode? unityNode in klass.EditorRootNode.SubNodes)
					{
						AddLoadToProcessor(unityNode, editorModeProcessor, fields);
					}
				}

				//Console.WriteLine($"Generating the release read method for {name}");
				if (klass.ReleaseRootNode != null)
				{
					foreach (UnityNode? unityNode in klass.ReleaseRootNode.SubNodes)
					{
						AddLoadToProcessor(unityNode, releaseModeProcessor, fields);
					}
				}

				editorModeProcessor.Add(CilOpCodes.Ret);
				releaseModeProcessor.Add(CilOpCodes.Ret);

				editorModeProcessor.OptimizeMacros();
				releaseModeProcessor.OptimizeMacros();
			}
		}

		private static void AddLoadToProcessor(UnityNode node, CilInstructionCollection processor, List<FieldDefinition> fields)
		{
			//Get field
			FieldDefinition? field = fields.SingleOrDefault(f => f.Name == node.Name);

			if (field == null)
				throw new Exception($"Field {node.Name} cannot be found in {processor.Owner.Owner.DeclaringType} (fields are {string.Join(", ", fields.Select(f => f.Name))})");

			ReadFieldContent(node, processor, field);
		}

		private static void ReadFieldContent(UnityNode node, CilInstructionCollection processor, FieldDefinition field)
		{
			if (SharedState.TypeDictionary.TryGetValue(node.TypeName, out TypeDefinition? fieldType))
			{
				ReadAssetTypeToField(node, processor, field, fieldType, 0);
				return;
			}

			switch (node.TypeName)
			{
				case "vector":
				case "set":
				case "staticvector":
					ReadVectorToField(node, processor, field, 1);
					return;
				case "map":
					ReadDictionaryToField(node, processor, field);
					return;
				case "pair":
					ReadPairToField(node, processor, field);
					return;
				case "TypelessData": //byte array
					ReadByteArrayToField(node, processor, field);
					return;
				case "Array":
					ReadArrayToField(node, processor, field, 1);
					return;
			}

			ReadPrimitiveTypeToField(node, processor, field, 0);
		}

		private static CilLocalVariable ReadContentToLocal(UnityNode node, CilInstructionCollection processor)
		{
			if (SharedState.TypeDictionary.TryGetValue(node.TypeName, out TypeDefinition? fieldType))
			{
				return ReadAssetTypeToLocal(node, processor, fieldType, 0);
			}

			switch (node.TypeName)
			{
				case "vector":
				case "set":
				case "staticvector":
					return ReadVectorToLocal(node, processor, 1);
				case "map":
					return ReadDictionaryToLocal(node, processor);
				case "pair":
					return ReadPairToLocal(node, processor);
				case "TypelessData": //byte array
					return ReadByteArrayToLocal(node, processor);
				case "Array":
					return ReadArrayToLocal(node, processor, 1);
				default:
					return ReadPrimitiveTypeToLocal(node, processor, 0);
			}
		}

		private static void MaybeAlignBytes(UnityNode node, CilInstructionCollection processor)
		{
			if (((TransferMetaFlags)node.MetaFlag).IsAlignBytes())
			{
				//Load reader
				processor.Add(CilOpCodes.Ldarg_1);

				//Get ReadAsset
				MethodDefinition alignMethod = CommonTypeGetter.EndianReaderDefinition.Resolve()!.Methods.Single(m => m.Name == "AlignStream");

				//Call it
				processor.Add(CilOpCodes.Call, SharedState.Importer.ImportMethod(alignMethod));
			}
		}

		private static void ReadPrimitiveTypeToField(UnityNode node, CilInstructionCollection processor, FieldDefinition field, int arrayDepth)
		{
			//Load this
			processor.Add(CilOpCodes.Ldarg_0);

			ReadPrimitiveTypeWithoutAligning(node, processor, arrayDepth);

			//Store result in field
			processor.Add(CilOpCodes.Stfld, field);

			MaybeAlignBytes(node, processor);
		}

		private static CilLocalVariable ReadPrimitiveTypeToLocal(UnityNode node, CilInstructionCollection processor, int arrayDepth)
		{
			TypeSignature fieldType = ReadPrimitiveTypeWithoutAligning(node, processor, arrayDepth);
			CilLocalVariable local = new CilLocalVariable(fieldType);
			processor.Owner.LocalVariables.Add(local);
			processor.Add(CilOpCodes.Stloc, local);
			MaybeAlignBytes(node, processor);
			return local;
		}

		private static TypeSignature ReadPrimitiveTypeWithoutAligning(UnityNode node, CilInstructionCollection processor, int arrayDepth)
		{
			//Primitives
			string csPrimitiveTypeName = SystemTypeGetter.CppPrimitivesToCSharpPrimitives[node.TypeName];
			CorLibTypeSignature csPrimitiveType = SystemTypeGetter.GetCppPrimitiveTypeSignature(node.TypeName) ?? throw new Exception();

			//Read
			MethodDefinition? primitiveReadMethod = arrayDepth switch
			{
				0 => CommonTypeGetter.AssetReaderDefinition.Resolve()!.Methods.SingleOrDefault(m => m.Name == $"Read{csPrimitiveTypeName}") //String
				     ?? CommonTypeGetter.EndianReaderDefinition.Resolve()!.Methods.SingleOrDefault(m => m.Name == $"Read{csPrimitiveTypeName}")
				     ?? SystemTypeGetter.LookupSystemType("System.IO.BinaryReader")!.Methods.SingleOrDefault(m => m.Name == $"Read{csPrimitiveTypeName}"), //Byte, SByte, and Boolean
				1 => CommonTypeGetter.EndianReaderDefinition.Resolve()!.Methods.SingleOrDefault(m => m.Name == $"Read{csPrimitiveTypeName}Array" && m.Parameters.Count == 1),
				2 => CommonTypeGetter.EndianReaderExtensionsDefinition.Resolve()!.Methods.SingleOrDefault(m => m.Name == $"Read{csPrimitiveTypeName}ArrayArray"),
				_ => throw new ArgumentOutOfRangeException(nameof(arrayDepth), $"ReadPrimitiveType does not support array depth '{arrayDepth}'"),
			};

			if (primitiveReadMethod == null)
				throw new Exception($"Missing a read method for {csPrimitiveTypeName} in {processor.Owner.Owner.DeclaringType}");

			//Load reader
			processor.Add(CilOpCodes.Ldarg_1);

			if (arrayDepth == 1)//Read{Primitive}Array has an allowAlignment parameter
			{
				processor.Add(CilOpCodes.Ldc_I4, 0);//load false onto the stack
			}

			//Call read method
			processor.Add(CilOpCodes.Call, SharedState.Importer.ImportMethod(primitiveReadMethod));

			return arrayDepth switch
			{
				0 => csPrimitiveType,
				1 => csPrimitiveType.MakeSzArrayType(),
				2 => csPrimitiveType.MakeSzArrayType().MakeSzArrayType(),
				_ => throw new ArgumentOutOfRangeException(nameof(arrayDepth)),
			};
		}

		private static void ReadAssetTypeToField(UnityNode node, CilInstructionCollection processor, FieldDefinition field, TypeDefinition fieldType, int arrayDepth)
		{
			//Load "this" for field store later
			processor.Add(CilOpCodes.Ldarg_0);

			ReadAssetTypeWithoutAligning(node, processor, fieldType, arrayDepth);

			//Store result in field
			processor.Add(CilOpCodes.Stfld, field);

			//Maybe Align Bytes
			MaybeAlignBytes(node, processor);
		}

		private static CilLocalVariable ReadAssetTypeToLocal(UnityNode node, CilInstructionCollection processor, TypeDefinition fieldBaseType, int arrayDepth)
		{
			TypeSignature fieldSignature = ReadAssetTypeWithoutAligning(node, processor, fieldBaseType, arrayDepth);
			CilLocalVariable local = new CilLocalVariable(fieldSignature);
			processor.Owner.LocalVariables.Add(local);
			processor.Add(CilOpCodes.Stloc, local);
			MaybeAlignBytes(node, processor);
			return local;
		}

		private static TypeSignature ReadAssetTypeWithoutAligning(UnityNode node, CilInstructionCollection processor, TypeDefinition fieldBaseType, int arrayDepth)
		{
			TypeSignature fieldBaseSignature = fieldBaseType.ToTypeSignature();
			//Load reader
			processor.Add(CilOpCodes.Ldarg_1);

			//Get ReadAsset
			MethodDefinition readMethod = arrayDepth switch
			{
				0 => CommonTypeGetter.AssetReaderDefinition.Resolve()!.Methods.First(m => m.Name == "ReadAsset"),
				1 => CommonTypeGetter.AssetReaderDefinition.Resolve()!.Methods.First(m => m.Name == "ReadAssetArray" && m.Parameters.Count == 1),
				2 => CommonTypeGetter.AssetReaderDefinition.Resolve()!.Methods.First(m => m.Name == "ReadAssetArrayArray" && m.Parameters.Count == 1),
				_ => throw new ArgumentOutOfRangeException(nameof(arrayDepth), $"ReadAssetType does not support array depth '{arrayDepth}'"),
			};

			//Make generic ReadAsset<T>
			MethodSpecification? genericInst = MethodUtils.MakeGenericInstanceMethod(readMethod, fieldBaseSignature);

			if (arrayDepth > 0)//ReadAssetArray and ReadAssetArrayArray have an allowAlignment parameter
			{
				processor.Add(CilOpCodes.Ldc_I4, 0);//load false onto the stack
			}

			//Call it
			processor.Add(CilOpCodes.Call, genericInst);

			return arrayDepth switch
			{
				0 => fieldBaseSignature,
				1 => fieldBaseSignature.MakeSzArrayType(),
				2 => fieldBaseSignature.MakeSzArrayType().MakeSzArrayType(),
				_ => throw new ArgumentOutOfRangeException(nameof(arrayDepth)),
			};
		}

		private static void ReadByteArrayToField(UnityNode node, CilInstructionCollection processor, FieldDefinition field)
		{
			//Load "this" for field store later
			processor.Add(CilOpCodes.Ldarg_0);
			ReadByteArrayWithoutAligning(node, processor);
			//Store result in field
			processor.Add(CilOpCodes.Stfld, field);
			MaybeAlignBytes(node, processor);
		}

		private static CilLocalVariable ReadByteArrayToLocal(UnityNode node, CilInstructionCollection processor)
		{
			ReadByteArrayWithoutAligning(node, processor);
			CilLocalVariable local = new CilLocalVariable(SystemTypeGetter.UInt8.MakeSzArrayType());
			processor.Owner.LocalVariables.Add(local);
			processor.Add(CilOpCodes.Stloc, local);
			MaybeAlignBytes(node, processor);
			return local;
		}

		private static void ReadByteArrayWithoutAligning(UnityNode node, CilInstructionCollection processor)
		{
			//Load reader
			processor.Add(CilOpCodes.Ldarg_1);

			//Get ReadAsset
			MethodDefinition? readMethod = CommonTypeGetter.EndianReaderDefinition.Resolve()!.Methods.First(m => m.Name == "ReadByteArray");

			//Call it
			processor.Add(CilOpCodes.Call, SharedState.Importer.ImportMethod(readMethod));
		}

		private static void ReadVectorToField(UnityNode node, CilInstructionCollection processor, FieldDefinition field, int arrayDepth)
		{
			UnityNode? listTypeNode = node.SubNodes[0];
			if (listTypeNode.TypeName is "Array")
			{
				ReadArrayToField(listTypeNode, processor, field, arrayDepth);
			}
			else
			{
				throw new ArgumentException($"Invalid subnode for {node.TypeName}", nameof(node));
			}

			MaybeAlignBytes(node, processor);
		}

		private static CilLocalVariable ReadVectorToLocal(UnityNode node, CilInstructionCollection processor, int arrayDepth)
		{
			UnityNode? listTypeNode = node.SubNodes[0];
			if (listTypeNode.TypeName is "Array")
			{
				CilLocalVariable? result = ReadArrayToLocal(listTypeNode, processor, arrayDepth);
				MaybeAlignBytes(node, processor);
				return result;
			}
			else
			{
				throw new ArgumentException($"Invalid subnode for {node.TypeName}", nameof(node));
			}
		}

		private static void ReadArrayToField(UnityNode node, CilInstructionCollection processor, FieldDefinition field, int arrayDepth)
		{
			UnityNode? listTypeNode = node.SubNodes[1];
			if (SharedState.TypeDictionary.TryGetValue(listTypeNode.TypeName, out TypeDefinition? fieldType))
			{
				ReadAssetTypeToField(listTypeNode, processor, field, fieldType, arrayDepth);
			}
			else
				switch (listTypeNode.TypeName)
				{
					case "vector" or "set" or "staticvector":
						ReadVectorToField(listTypeNode, processor, field, arrayDepth + 1);
						break;
					case "Array":
						ReadArrayToField(listTypeNode, processor, field, arrayDepth + 1);
						break;
					case "map":
						if (arrayDepth > 1)
							throw new("ReadArray does not support dictionary arrays with a depth > 1. Found in {node.Name} (field {field}) of {processor.Body.Method.DeclaringType}");

						ReadDictionaryArrayToField(processor, field, node);
						break;
					case "pair":
						if (arrayDepth > 2)
							throw new($"ReadArray does not support Pair arrays with a depth > 2. Found in {node.Name} (field {field}) of {processor.Owner.Owner.DeclaringType}");

						if (arrayDepth == 2)
						{
							ReadPairArrayArrayToField(processor, field, listTypeNode);
							break;
						}

						ReadPairArrayToField(processor, field, listTypeNode);
						break;
					default:
						ReadPrimitiveTypeToField(listTypeNode, processor, field, arrayDepth);
						break;
				}

			MaybeAlignBytes(node, processor);
		}

		private static CilLocalVariable ReadArrayToLocal(UnityNode node, CilInstructionCollection processor, int arrayDepth)
		{
			UnityNode? listTypeNode = node.SubNodes[1];
			if (SharedState.TypeDictionary.TryGetValue(listTypeNode.TypeName, out TypeDefinition? fieldType))
			{
				return ReadAssetTypeToLocal(listTypeNode, processor, fieldType, arrayDepth);
			}
			else
				switch (listTypeNode.TypeName)
				{
					case "vector" or "set" or "staticvector":
						return ReadVectorToLocal(listTypeNode, processor, arrayDepth + 1);
					case "Array":
						return ReadArrayToLocal(listTypeNode, processor, arrayDepth + 1);
					case "map":
						if (arrayDepth > 1)
							throw new NotSupportedException($"ReadArray does not support dictionary arrays with a depth > 1. Found in {node.Name} of {processor.Owner.Owner.DeclaringType}");

						return ReadDictionaryArrayToLocal(processor, node);
					case "pair":
						if (arrayDepth > 2)
							throw new($"ReadArray does not support Pair arrays with a depth > 2. Found in {node.Name} of {processor.Owner.Owner.DeclaringType}");

						if (arrayDepth == 2)
						{
							return ReadPairArrayArray(processor, listTypeNode);
						}

						return ReadPairArrayToLocal(processor, listTypeNode);
					default:
						return ReadPrimitiveTypeToLocal(listTypeNode, processor, arrayDepth);
				}
		}

		private static void ReadPairArrayToField(CilInstructionCollection processor, FieldDefinition field, UnityNode listTypeNode)
		{
			CilLocalVariable? arrayLocal = ReadPairArrayToLocal(processor, listTypeNode);
			//Now just store field
			processor.Add(CilOpCodes.Ldarg_0); //Load this
			processor.Add(CilOpCodes.Ldloc, arrayLocal); //Load array
			processor.Add(CilOpCodes.Stfld, field); //Store field
		}

		private static CilLocalVariable ReadPairArrayToLocal(CilInstructionCollection processor, UnityNode listTypeNode)
		{
			//Strategy:
			//Read Count
			//Make array of size count
			//For i = 0 .. count
			//	Read pair, store in array
			//Store array in field

			//Resolve things we'll need
			UnityNode first = listTypeNode.SubNodes[0];
			UnityNode second = listTypeNode.SubNodes[1];
			GenericInstanceTypeSignature genericKvp = GenericTypeResolver.ResolvePairType(first, second);

			SzArrayTypeSignature arrayType = genericKvp.MakeSzArrayType();

			//Read length of array
			IMethodDefOrRef? intReader = SharedState.Importer.ImportMethod(CommonTypeGetter.EndianReaderDefinition.Resolve()!.Methods.Single(m => m.Name == "ReadInt32"));
			processor.Add(CilOpCodes.Ldarg_1); //Load reader
			processor.Add(CilOpCodes.Call, intReader); //Call int reader

			//Make local and store length in it
			CilLocalVariable? countLocal = new CilLocalVariable(SystemTypeGetter.Int32); //Create local
			processor.Owner.LocalVariables.Add(countLocal); //Add to method
			processor.Add(CilOpCodes.Stloc, countLocal); //Store count in it

			//Create empty array and local for it
			processor.Add(CilOpCodes.Ldloc, countLocal); //Load count
			processor.Add(CilOpCodes.Newarr, genericKvp.ToTypeDefOrRef()); //Create new array of kvp with given count
			CilLocalVariable? arrayLocal = new CilLocalVariable(arrayType); //Create local
			processor.Owner.LocalVariables.Add(arrayLocal); //Add to method
			processor.Add(CilOpCodes.Stloc, arrayLocal); //Store array in local

			//Make an i
			CilLocalVariable? iLocal = new CilLocalVariable(SystemTypeGetter.Int32); //Create local
			processor.Owner.LocalVariables.Add(iLocal); //Add to method
			processor.Add(CilOpCodes.Ldc_I4_0); //Load 0 as an int32
			processor.Add(CilOpCodes.Stloc, iLocal); //Store in count

			//Create a label for a dummy instruction to jump back to
			CilInstructionLabel? jumpTargetLabel = new CilInstructionLabel();

			//Create an empty, unconditional branch which will jump down to the loop condition.
			//This converts the do..while loop into a for loop.
			CilInstruction? unconditionalBranch = processor.Add(CilOpCodes.Br, DummyInstructionLabel);

			//Now we just read pair, increment i, compare against count, and jump back to here if it's less
			jumpTargetLabel.Instruction = processor.Add(CilOpCodes.Nop); //Create a dummy instruction to jump back to

			//Read element at index i of array
			CilLocalVariable? pairLocal = ReadPairToLocal(listTypeNode, processor); //Read the pair
			processor.Add(CilOpCodes.Ldloc, arrayLocal); //Load array local
			processor.Add(CilOpCodes.Ldloc, iLocal); //Load i local
			processor.Add(CilOpCodes.Ldloc, pairLocal);
			processor.Add(CilOpCodes.Stelem, genericKvp.ToTypeDefOrRef()); //Store in array

			//Increment i
			processor.Add(CilOpCodes.Ldloc, iLocal); //Load i local
			processor.Add(CilOpCodes.Ldc_I4_1); //Load constant 1 as int32
			processor.Add(CilOpCodes.Add); //Add 
			processor.Add(CilOpCodes.Stloc, iLocal); //Store in i local

			//Jump to start of loop if i < count
			ICilLabel? loopConditionStartLabel = processor.Add(CilOpCodes.Ldloc, iLocal).CreateLabel(); //Load i
			processor.Add(CilOpCodes.Ldloc, countLocal); //Load count
			processor.Add(CilOpCodes.Blt, jumpTargetLabel); //Jump back up if less than
			unconditionalBranch.Operand = loopConditionStartLabel;

			return arrayLocal;
		}

		private static void ReadPairArrayArrayToField(CilInstructionCollection processor, FieldDefinition field, UnityNode listTypeNode)
		{
			CilLocalVariable? arrayLocal = ReadPairArrayArray(processor, listTypeNode);
			//Now just store field
			processor.Add(CilOpCodes.Ldarg_0); //Load this
			processor.Add(CilOpCodes.Ldloc, arrayLocal); //Load array
			processor.Add(CilOpCodes.Stfld, field); //Store field
		}

		private static CilLocalVariable ReadPairArrayArray(CilInstructionCollection processor, UnityNode pairNode)
		{
			//Strategy:
			//Read Count
			//Make array of size count
			//For i = 0 .. count
			//	Read array of pairs, store in array of arrays
			//Store array in field

			//Resolve things we'll need
			UnityNode first = pairNode.SubNodes[0];
			UnityNode second = pairNode.SubNodes[1];
			GenericInstanceTypeSignature genericKvp = GenericTypeResolver.ResolvePairType(first, second);

			SzArrayTypeSignature arrayType = genericKvp.MakeSzArrayType();

			//Read length of array
			IMethodDefOrRef? intReader = SharedState.Importer.ImportMethod(CommonTypeGetter.EndianReaderDefinition.Resolve()!.Methods.Single(m => m.Name == "ReadInt32"));
			processor.Add(CilOpCodes.Ldarg_1); //Load reader
			processor.Add(CilOpCodes.Call, intReader); //Call int reader

			//Make local and store length in it
			CilLocalVariable? countLocal = new CilLocalVariable(SystemTypeGetter.Int32); //Create local
			processor.Owner.LocalVariables.Add(countLocal); //Add to method
			processor.Add(CilOpCodes.Stloc, countLocal); //Store count in it

			//Create empty array and local for it
			processor.Add(CilOpCodes.Ldloc, countLocal); //Load count
			processor.Add(CilOpCodes.Newarr, arrayType.ToTypeDefOrRef()); //Create new array of arrays of kvps with given count
			CilLocalVariable? arrayLocal = new CilLocalVariable(arrayType.MakeSzArrayType()); //Create local
			processor.Owner.LocalVariables.Add(arrayLocal); //Add to method
			processor.Add(CilOpCodes.Stloc, arrayLocal); //Store array in local

			//Make an i
			CilLocalVariable? iLocal = new CilLocalVariable(SystemTypeGetter.Int32); //Create local
			processor.Owner.LocalVariables.Add(iLocal); //Add to method
			processor.Add(CilOpCodes.Ldc_I4_0); //Load 0 as an int32
			processor.Add(CilOpCodes.Stloc, iLocal); //Store in count

			//Create an empty, unconditional branch which will jump down to the loop condition.
			//This converts the do..while loop into a for loop.
			CilInstruction? unconditionalBranch = processor.Add(CilOpCodes.Br, DummyInstructionLabel);

			//Now we just read pair, increment i, compare against count, and jump back to here if it's less
			ICilLabel? jumpTarget = processor.Add(CilOpCodes.Nop).CreateLabel(); //Create a dummy instruction to jump back to

			//Read element at index i of array
			CilLocalVariable? pairArrayLocal = ReadPairArrayToLocal(processor, pairNode); //Read the array of pairs
			
			processor.Add(CilOpCodes.Ldloc, arrayLocal); //Load array of arrays local
			processor.Add(CilOpCodes.Ldloc, iLocal); //Load i local
			processor.Add(CilOpCodes.Ldloc, pairArrayLocal); //Load array of pairs
			processor.Add(CilOpCodes.Stelem, arrayType.ToTypeDefOrRef()); //Store in array

			//Increment i
			processor.Add(CilOpCodes.Ldloc, iLocal); //Load i local
			processor.Add(CilOpCodes.Ldc_I4_1); //Load constant 1 as int32
			processor.Add(CilOpCodes.Add); //Add 
			processor.Add(CilOpCodes.Stloc, iLocal); //Store in i local

			//Jump to start of loop if i < count
			ICilLabel? loopConditionStartLabel = processor.Add(CilOpCodes.Ldloc, iLocal).CreateLabel(); //Load i
			processor.Add(CilOpCodes.Ldloc, countLocal); //Load count
			processor.Add(CilOpCodes.Blt, jumpTarget); //Jump back up if less than
			unconditionalBranch.Operand = loopConditionStartLabel;

			MaybeAlignBytes(pairNode, processor);

			return arrayLocal;
		}

		private static void ReadDictionaryArrayToField(CilInstructionCollection processor, FieldDefinition field, UnityNode node)
		{
			CilLocalVariable? arrayLocal = ReadDictionaryArrayToLocal(processor, node);
			//Now just store field
			processor.Add(CilOpCodes.Ldarg_0); //Load this
			processor.Add(CilOpCodes.Ldloc, arrayLocal); //Load array
			processor.Add(CilOpCodes.Stfld, field); //Store field
		}

		private static CilLocalVariable ReadDictionaryArrayToLocal(CilInstructionCollection processor, UnityNode node)
		{
			//you know the drill
			//read count
			//make empty array
			//for i = 0 .. count 
			//  read an entire bloody dictionary
			//set field

			//we need an array type, so let's get that
			UnityNode? dictNode = node.SubNodes[1];
			GenericInstanceTypeSignature? dictType = GenericTypeResolver.ResolveDictionaryType(dictNode);
			SzArrayTypeSignature? arrayType = dictType.MakeSzArrayType(); //cursed. that is all.

			//Read length of array
			IMethodDefOrRef? intReader = SharedState.Importer.ImportMethod(CommonTypeGetter.EndianReaderDefinition.Resolve()!.Methods.Single(m => m.Name == "ReadInt32"));
			processor.Add(CilOpCodes.Ldarg_1); //Load reader
			processor.Add(CilOpCodes.Call, intReader); //Call int reader

			//Make local and store length in it
			CilLocalVariable? countLocal = new CilLocalVariable(SystemTypeGetter.Int32); //Create local
			processor.Owner.LocalVariables.Add(countLocal); //Add to method
			processor.Add(CilOpCodes.Stloc, countLocal); //Store count in it

			//Create empty array and local for it
			processor.Add(CilOpCodes.Ldloc, countLocal); //Load count
			processor.Add(CilOpCodes.Newarr, dictType.ToTypeDefOrRef()); //Create new array of dictionaries with given count
			CilLocalVariable? arrayLocal = new CilLocalVariable(arrayType); //Create local
			processor.Owner.LocalVariables.Add(arrayLocal); //Add to method
			processor.Add(CilOpCodes.Stloc, arrayLocal); //Store array in local

			//Make an i
			CilLocalVariable? iLocal = new CilLocalVariable(SystemTypeGetter.Int32); //Create local
			processor.Owner.LocalVariables.Add(iLocal); //Add to method
			processor.Add(CilOpCodes.Ldc_I4_0); //Load 0 as an int32
			processor.Add(CilOpCodes.Stloc, iLocal); //Store in count

			//Create an empty, unconditional branch which will jump down to the loop condition.
			//This converts the do..while loop into a for loop.
			CilInstruction? unconditionalBranch = processor.Add(CilOpCodes.Br, DummyInstructionLabel);

			//Now we just read pair, increment i, compare against count, and jump back to here if it's less
			ICilLabel? jumpTarget = processor.Add(CilOpCodes.Nop).CreateLabel(); //Create a dummy instruction to jump back to

			//Read element at index i of array
			CilLocalVariable? dictLocal = ReadDictionaryToLocal(dictNode, processor);
			processor.Add(CilOpCodes.Ldloc, arrayLocal); //Load array local
			processor.Add(CilOpCodes.Ldloc, iLocal); //Load i local
			processor.Add(CilOpCodes.Ldloc, dictLocal); //Load dict
			processor.Add(CilOpCodes.Stelem, dictType.ToTypeDefOrRef()); //Store in array

			//Increment i
			processor.Add(CilOpCodes.Ldloc, iLocal); //Load i local
			processor.Add(CilOpCodes.Ldc_I4_1); //Load constant 1 as int32
			processor.Add(CilOpCodes.Add); //Add 
			processor.Add(CilOpCodes.Stloc, iLocal); //Store in i local

			//Jump to start of loop if i < count
			ICilLabel? loopConditionStartLabel = processor.Add(CilOpCodes.Ldloc, iLocal).CreateLabel(); //Load i
			processor.Add(CilOpCodes.Ldloc, countLocal); //Load count
			processor.Add(CilOpCodes.Blt, jumpTarget); //Jump back up if less than
			unconditionalBranch.Operand = loopConditionStartLabel;

			MaybeAlignBytes(node, processor);

			return arrayLocal;
		}

		private static void ReadDictionaryToField(UnityNode node, CilInstructionCollection processor, FieldDefinition field)
		{
			CilLocalVariable dictLocal = ReadDictionaryToLocal(node, processor);
			//Now just store field
			processor.Add(CilOpCodes.Ldarg_0); //Load this

			processor.Add(CilOpCodes.Ldloc, dictLocal); //Load dict

			processor.Add(CilOpCodes.Stfld, field); //Store field
		}

		private static CilLocalVariable ReadDictionaryToLocal(UnityNode node, CilInstructionCollection processor)
		{
			//Strategy:
			//Read Count
			//Make dictionary
			//For i = 0 .. count
			//	Read key, read value, store in dict
			//Store dict in field

			//Resolve things we'll need
			GenericInstanceTypeSignature? genericDictType = GenericTypeResolver.ResolveDictionaryType(node);
			IMethodDefOrRef? genericDictCtor = MethodUtils.MakeConstructorOnGenericType(genericDictType, 0);
			IMethodDefOrRef? addMethod = MethodUtils.MakeMethodOnGenericType(genericDictType, genericDictType.Resolve()!.Methods.Single(m => m.Name == "Add" && m.Parameters.Count == 2));

			//Read length of array
			IMethodDefOrRef? intReader = SharedState.Importer.ImportMethod(CommonTypeGetter.EndianReaderDefinition.Resolve()!.Methods.Single(m => m.Name == "ReadInt32"));
			processor.Add(CilOpCodes.Ldarg_1); //Load reader
			processor.Add(CilOpCodes.Call, intReader); //Call int reader

			//Make local and store length in it
			CilLocalVariable? countLocal = new CilLocalVariable(SystemTypeGetter.Int32); //Create local
			processor.Owner.LocalVariables.Add(countLocal); //Add to method
			processor.Add(CilOpCodes.Stloc, countLocal); //Store count in it

			//Create empty dict and local for it
			processor.Add(CilOpCodes.Newobj, genericDictCtor); //Create new dictionary
			CilLocalVariable? dictLocal = new CilLocalVariable(genericDictType); //Create local
			processor.Owner.LocalVariables.Add(dictLocal); //Add to method
			processor.Add(CilOpCodes.Stloc, dictLocal); //Store dict in local

			//Make an i
			CilLocalVariable? iLocal = new CilLocalVariable(SystemTypeGetter.Int32); //Create local
			processor.Owner.LocalVariables.Add(iLocal); //Add to method
			processor.Add(CilOpCodes.Ldc_I4_0); //Load 0 as an int32
			processor.Add(CilOpCodes.Stloc, iLocal); //Store in count

			//Create an empty, unconditional branch which will jump down to the loop condition.
			//This converts the do..while loop into a for loop.
			CilInstruction? unconditionalBranch = processor.Add(CilOpCodes.Br, DummyInstructionLabel);

			//Now we just read key + value, increment i, compare against count, and jump back to here if it's less
			ICilLabel? jumpTarget = processor.Add(CilOpCodes.Nop).CreateLabel(); //Create a dummy instruction to jump back to

			//Read ith key-value pair of dict 
			CilLocalVariable? local1 = ReadContentToLocal(node.SubNodes[0].SubNodes[1].SubNodes[0], processor); //Load first
			CilLocalVariable? local2 = ReadContentToLocal(node.SubNodes[0].SubNodes[1].SubNodes[1], processor); //Load second
			processor.Add(CilOpCodes.Ldloc, dictLocal); //Load dict local
			processor.Add(CilOpCodes.Ldloc, local1); //Load 1st local
			processor.Add(CilOpCodes.Ldloc, local2); //Load 2nd local
			processor.Add(CilOpCodes.Call, addMethod); //Call Add(TKey, TValue)

			//Increment i
			processor.Add(CilOpCodes.Ldloc, iLocal); //Load i local
			processor.Add(CilOpCodes.Ldc_I4_1); //Load constant 1 as int32
			processor.Add(CilOpCodes.Add); //Add 
			processor.Add(CilOpCodes.Stloc, iLocal); //Store in i local

			//Jump to start of loop if i < count
			ICilLabel? loopConditionStartLabel = processor.Add(CilOpCodes.Ldloc, iLocal).CreateLabel(); //Load i
			processor.Add(CilOpCodes.Ldloc, countLocal); //Load count
			processor.Add(CilOpCodes.Blt, jumpTarget); //Jump back up if less than
			unconditionalBranch.Operand = loopConditionStartLabel;

			MaybeAlignBytes(node, processor);

			return dictLocal;
		}

		private static void ReadPairToField(UnityNode node, CilInstructionCollection processor, FieldDefinition field)
		{
			//Load this for later usage
			processor.Add(CilOpCodes.Ldarg_0);

			ReadPair(node, processor);

			//Store in field if desired
			processor.Add(CilOpCodes.Stfld, field);
		}

		private static CilLocalVariable ReadPairToLocal(UnityNode node, CilInstructionCollection processor)
		{
			TypeSignature pairType = ReadPair(node, processor);
			CilLocalVariable local = new CilLocalVariable(pairType);
			processor.Owner.LocalVariables.Add(local);
			processor.Add(CilOpCodes.Stloc, local);
			return local;
		}

		private static TypeSignature ReadPair(UnityNode node, CilInstructionCollection processor)
		{
			//Read one, read two, construct tuple, store field
			//Passing a null field to any of the Read generators causes no field store or this load to be emitted
			//Which is just what we want
			UnityNode? first = node.SubNodes[0];
			UnityNode? second = node.SubNodes[1];

			//Load the left side of the pair
			CilLocalVariable? local1 = ReadContentToLocal(first, processor);

			//Load the right side of the pair
			CilLocalVariable? local2 = ReadContentToLocal(second, processor);

			processor.Add(CilOpCodes.Ldloc, local1);
			processor.Add(CilOpCodes.Ldloc, local2);

			GenericInstanceTypeSignature? genericKvp = GenericTypeResolver.ResolvePairType(first, second);

			IMethodDefOrRef? genericCtor = MethodUtils.MakeConstructorOnGenericType(genericKvp, 2);

			//Call constructor
			processor.Add(CilOpCodes.Newobj, genericCtor);

			return genericKvp;
		}
	}
}
