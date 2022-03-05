using AssemblyDumper.Unity;
using AssemblyDumper.Utils;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.YAML;

namespace AssemblyDumper.Passes
{
	public static class Pass102_FillYamlMethods
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		private static IMethodDefOrRef addSerializedVersionMethod;
		private static IMethodDefOrRef mappingAddMethod;
		private static IMethodDefOrRef sequenceAddMethod;
		private static IMethodDefOrRef byteArrayToYamlMethod;
		private static IMethodDefOrRef addTypelessDataMethod;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		private static CilInstructionLabel DummyInstructionLabel { get; } = new CilInstructionLabel();

		private static bool emittingRelease = true;

		private static void Initialize()
		{
			Func<MethodDefinition, bool> filter = m => m.Name == "AddSerializedVersion";
			addSerializedVersionMethod = SharedState.Importer.ImportCommonMethod(typeof(AssetRipper.Core.IO.Extensions.SerializedVersionYAMLExtensions), filter);

			mappingAddMethod = SharedState.Importer.ImportCommonMethod<YAMLMappingNode>(
				m => m.Name == "Add" &&
				m.Parameters.Count == 2 &&
				m.Parameters[0].ParameterType.Name == nameof(AssetRipper.Core.YAML.YAMLNode) &&
				m.Parameters[1].ParameterType.Name == nameof(AssetRipper.Core.YAML.YAMLNode));

			Func<MethodDefinition, bool> filter1 = m => m.Name == "ExportYAML" && m.Parameters.Count == 1;
			byteArrayToYamlMethod = SharedState.Importer.ImportCommonMethod(typeof(AssetRipper.Core.YAML.Extensions.ArrayYAMLExtensions), filter1);

			addTypelessDataMethod = SharedState.Importer.ImportCommonMethod(typeof(AssetRipper.Core.YAML.Extensions.ArrayYAMLExtensions), m => m.Name == "AddTypelessData");

			sequenceAddMethod = SharedState.Importer.ImportCommonMethod<YAMLSequenceNode>(
				m => m.Name == "Add" &&
				m.Parameters.Count == 1 &&
				m.Parameters[0].ParameterType.Name == nameof(AssetRipper.Core.YAML.YAMLNode));
		}

		public static void DoPass()
		{
			Console.WriteLine("Pass 102: Filling yaml methods");
			Initialize();
			foreach ((string name, UnityClass klass) in SharedState.ClassDictionary)
			{
				TypeDefinition? type = SharedState.TypeDictionary[name];
				List<FieldDefinition> fields = FieldUtils.GetAllFieldsInTypeAndBase(type).Distinct().ToList();
				emittingRelease = false;
				type.FillEditorMethod(klass, fields);
				emittingRelease = true;
				type.FillReleaseMethod(klass, fields);
			}
		}

		private static void FillEditorMethod(this TypeDefinition type, UnityClass klass, List<FieldDefinition> fields)
		{
			MethodDefinition editorModeYamlMethod = type.Methods.Single(m => m.Name == "ExportYAMLEditor");
			editorModeYamlMethod.FillMethod(klass.EditorRootNode, fields);
		}

		private static void FillReleaseMethod(this TypeDefinition type, UnityClass klass, List<FieldDefinition> fields)
		{
			MethodDefinition? releaseModeYamlMethod = type.Methods.Single(m => m.Name == "ExportYAMLRelease");
			releaseModeYamlMethod.FillMethod(klass.ReleaseRootNode, fields);
		}

		private static void FillMethod(this MethodDefinition method, UnityNode? rootNode, List<FieldDefinition> fields)
		{
			CilMethodBody body = method.CilMethodBody!;
			CilInstructionCollection processor = body.Instructions;

			body.InitializeLocals = true;
			CilLocalVariable resultNode = new CilLocalVariable(CommonTypeGetter.YAMLMappingNodeDefinition.ToTypeSignature());
			body.LocalVariables.Add(resultNode);
			processor.Add(CilOpCodes.Newobj, CommonTypeGetter.YAMLMappingNodeConstructor);
			processor.Add(CilOpCodes.Stloc, resultNode);

			//Console.WriteLine($"Generating the release read method for {name}");
			if (rootNode != null)
			{
				processor.MaybeEmitFlowMappingStyle(rootNode, resultNode);
				processor.AddAddSerializedVersion(resultNode, rootNode.Version);
				foreach (UnityNode? unityNode in rootNode.SubNodes)
				{
					AddExportToProcessor(unityNode, processor, fields, resultNode);
				}
			}

			processor.Add(CilOpCodes.Ldloc, resultNode);
			processor.Add(CilOpCodes.Ret);
			processor.OptimizeMacros();
		}

		private static void AddAddSerializedVersion(this CilInstructionCollection processor, CilLocalVariable yamlMappingNode, int version)
		{
			processor.Add(CilOpCodes.Ldloc, yamlMappingNode);
			processor.Add(CilOpCodes.Ldc_I4, version);
			processor.Add(CilOpCodes.Call, addSerializedVersionMethod);
		}

		private static void AddExportToProcessor(UnityNode node, CilInstructionCollection processor, List<FieldDefinition> fields, CilLocalVariable yamlMappingNode)
		{
			//Get field
			FieldDefinition? field = fields.SingleOrDefault(f => f.Name == node.Name);

			if (field == null)
				throw new Exception($"Field {node.Name} cannot be found in {processor.Owner.Owner.DeclaringType} (fields are {string.Join(", ", fields.Select(f => f.Name))})");

			processor.AddExportFieldContent(node, field, yamlMappingNode);
		}

		private static void AddExportFieldContent(this CilInstructionCollection processor, UnityNode node, FieldDefinition field, CilLocalVariable yamlMappingNode)
		{
			if (SharedState.TypeDictionary.TryGetValue(node.TypeName, out TypeDefinition? typeDefinition))
			{
				processor.AddExportAssetField(node, field, yamlMappingNode, typeDefinition);
				return;
			}

			switch (node.TypeName)
			{
				case "vector":
				case "set":
				case "staticvector":
					processor.AddExportVectorField(node, field, yamlMappingNode);
					return;
				case "map":
					processor.AddExportDictionaryField(node, field, yamlMappingNode);
					return;
				case "pair":
					processor.AddExportPairField(node, field, yamlMappingNode);
					return;
				case "TypelessData": //byte array
					processor.AddExportTypelessDataField(node, field, yamlMappingNode);
					return;
				case "Array":
					processor.AddExportArrayField(node, field, yamlMappingNode);
					return;
				default:
					processor.AddExportPrimitiveField(node, field, yamlMappingNode);
					return;
			}
		}

		private static void AddLocalForLoadedValue(this CilInstructionCollection processor, UnityNode node, out CilLocalVariable localVariable)
		{
			if (SharedState.TypeDictionary.TryGetValue(node.TypeName, out TypeDefinition? typeDefinition))
			{
				processor.AddLocalForLoadedAssetValue(typeDefinition, out localVariable);
				return;
			}

			switch (node.TypeName)
			{
				case "vector":
				case "set":
				case "staticvector":
					processor.AddLocalForLoadedVector(node, out localVariable);
					return;
				case "map":
					processor.AddLocalForLoadedDictionary(node, out localVariable);
					return;
				case "pair":
					processor.AddLocalForLoadedPair(node, out localVariable);
					return;
				case "TypelessData": //byte array
					//processor.AddLocalForLoadedByteArray(out localVariable);
					//return;
					throw new NotSupportedException("TypelessData");
				case "Array":
					processor.AddLocalForLoadedArray(node, out localVariable);
					return;
				default:
					processor.AddLocalForLoadedPrimitiveValue(node.TypeName, out localVariable);
					return;
			}
		}

		private static void AddExportAssetField(this CilInstructionCollection processor, UnityNode node, FieldDefinition field, CilLocalVariable yamlMappingNode, TypeDefinition typeDefinition)
		{
			processor.Add(CilOpCodes.Ldloc, yamlMappingNode);
			processor.AddScalarNodeForString(node.OriginalName);
			processor.AddLoadField(field);
			processor.AddNodeForLoadedAssetValue(typeDefinition);
			processor.Add(CilOpCodes.Call, mappingAddMethod);
		}

		private static void AddExportPrimitiveField(this CilInstructionCollection processor, UnityNode node, FieldDefinition field, CilLocalVariable yamlMappingNode)
		{
			processor.Add(CilOpCodes.Ldloc, yamlMappingNode);
			processor.AddScalarNodeForString(node.OriginalName);
			processor.AddLoadField(field);
			processor.AddScalarNodeForLoadedPrimitiveValue(node.TypeName);
			processor.Add(CilOpCodes.Call, mappingAddMethod);
		}

		private static void AddNodeForLoadedAssetValue(this CilInstructionCollection processor, TypeDefinition type)
		{
			MethodDefinition exportMethod = type.GetYamlExportMethod();
			processor.Add(CilOpCodes.Ldarg_1); //IExportCollection container
			processor.Add(CilOpCodes.Call, exportMethod);
		}

		private static void AddLocalForLoadedAssetValue(this CilInstructionCollection processor, TypeDefinition type, out CilLocalVariable localVariable)
		{
			processor.AddNodeForLoadedAssetValue(type);
			localVariable = new CilLocalVariable(CommonTypeGetter.YAMLNodeDefinition.ToTypeSignature());
			processor.Owner.LocalVariables.Add(localVariable);
			processor.Add(CilOpCodes.Stloc, localVariable);
		}

		private static void AddExportPairField(this CilInstructionCollection processor, UnityNode node, FieldDefinition field, CilLocalVariable yamlMappingNode)
		{
			processor.AddLoadField(field);
			processor.AddLocalForLoadedPair(node, out CilLocalVariable pairNodeLocal);
			processor.Add(CilOpCodes.Ldloc, yamlMappingNode);
			processor.AddScalarNodeForString(node.OriginalName);
			processor.Add(CilOpCodes.Ldloc, pairNodeLocal);
			processor.Add(CilOpCodes.Call, mappingAddMethod);
		}

		private static void AddLocalForLoadedPair(this CilInstructionCollection processor, UnityNode pairNode, out CilLocalVariable localVariableForOutputNode)
		{
			UnityNode firstSubNode = pairNode.SubNodes[0];
			UnityNode secondSubNode = pairNode.SubNodes[1];
			GenericInstanceTypeSignature pairType = GenericTypeResolver.ResolvePairType(firstSubNode, secondSubNode);
			bool firstIsScalar = SystemTypeGetter.GetCppPrimitiveTypeSignature(firstSubNode.TypeName) != null
				|| firstSubNode.TypeName == Pass002_RenameSubnodes.Utf8StringName;

			MethodDefinition getKeyDefinition = CommonTypeGetter.LookupCommonMethod("AssetRipper.Core.IO.NullableKeyValuePair`2", m => m.Name == "get_Key");
			IMethodDefOrRef getKeyReference = MethodUtils.MakeMethodOnGenericType(pairType, getKeyDefinition);
			MethodDefinition getValueDefinition = CommonTypeGetter.LookupCommonMethod("AssetRipper.Core.IO.NullableKeyValuePair`2", m => m.Name == "get_Value");
			IMethodDefOrRef getValueReference = MethodUtils.MakeMethodOnGenericType(pairType, getValueDefinition);

			CilLocalVariable pair = new CilLocalVariable(pairType);
			processor.Owner.LocalVariables.Add(pair);
			processor.Add(CilOpCodes.Stloc, pair);

			localVariableForOutputNode = new CilLocalVariable(CommonTypeGetter.YAMLMappingNodeDefinition.ToTypeSignature());
			processor.Owner.LocalVariables.Add(localVariableForOutputNode);
			processor.Add(CilOpCodes.Newobj, CommonTypeGetter.YAMLMappingNodeConstructor);
			processor.Add(CilOpCodes.Stloc, localVariableForOutputNode);

			if (firstIsScalar)
			{
				processor.Add(CilOpCodes.Ldloc, pair);
				processor.Add(CilOpCodes.Call, getKeyReference);
				processor.AddLocalForLoadedValue(firstSubNode, out CilLocalVariable firstExportLocal);

				processor.Add(CilOpCodes.Ldloc, pair);
				processor.Add(CilOpCodes.Call, getValueReference);
				processor.AddLocalForLoadedValue(secondSubNode, out CilLocalVariable secondExportLocal);

				processor.Add(CilOpCodes.Ldloc, localVariableForOutputNode);
				processor.Add(CilOpCodes.Ldloc, firstExportLocal);
				processor.Add(CilOpCodes.Ldloc, secondExportLocal);
				processor.Add(CilOpCodes.Call, mappingAddMethod);
			}
			else
			{
				processor.Add(CilOpCodes.Ldloc, pair);
				processor.Add(CilOpCodes.Call, getKeyReference);
				processor.AddLocalForLoadedValue(firstSubNode, out CilLocalVariable? local1);
				processor.Add(CilOpCodes.Ldloc, localVariableForOutputNode);
				processor.AddScalarNodeForString("first");
				processor.Add(CilOpCodes.Ldloc, local1);
				processor.Add(CilOpCodes.Call, mappingAddMethod);

				processor.Add(CilOpCodes.Ldloc, pair);
				processor.Add(CilOpCodes.Call, getValueReference);
				processor.AddLocalForLoadedValue(secondSubNode, out CilLocalVariable? local2);
				processor.Add(CilOpCodes.Ldloc, localVariableForOutputNode);
				processor.AddScalarNodeForString("second");
				processor.Add(CilOpCodes.Ldloc, local2);
				processor.Add(CilOpCodes.Call, mappingAddMethod);
			}
		}

		private static void AddExportVectorField(this CilInstructionCollection processor, UnityNode vectorNode, FieldDefinition field, CilLocalVariable yamlMappingNode)
		{
			//This cloning is necessary to prevent the code from using "Array" instead of the actual name
			UnityNode? arrayNode = vectorNode.SubNodes[0].DeepClone();
			arrayNode.Name = vectorNode.Name;
			arrayNode.OriginalName = vectorNode.OriginalName;
			processor.AddExportArrayField(arrayNode, field, yamlMappingNode);
		}

		private static void AddLocalForLoadedVector(this CilInstructionCollection processor, UnityNode vectorNode, out CilLocalVariable localVariable)
		{
			//This cloning is necessary to prevent the code from using "Array" instead of the actual name
			UnityNode? arrayNode = vectorNode.SubNodes[0].DeepClone();
			arrayNode.Name = vectorNode.Name;
			arrayNode.OriginalName = vectorNode.OriginalName;
			processor.AddLocalForLoadedArray(arrayNode, out localVariable);
		}

		private static void AddExportTypelessDataField(this CilInstructionCollection processor, UnityNode node, FieldDefinition field, CilLocalVariable yamlMappingNode)
		{
			processor.Add(CilOpCodes.Ldloc, yamlMappingNode);
			processor.Add(CilOpCodes.Ldstr, node.OriginalName);
			processor.AddLoadField(field);
			processor.Add(CilOpCodes.Call, addTypelessDataMethod);
		}

		private static void AddExportByteArrayField(this CilInstructionCollection processor, UnityNode node, FieldDefinition field, CilLocalVariable yamlMappingNode)
		{
			processor.Add(CilOpCodes.Ldloc, yamlMappingNode);
			processor.AddScalarNodeForString(node.OriginalName);
			processor.AddLoadField(field);
			processor.Add(CilOpCodes.Call, byteArrayToYamlMethod);
			processor.Add(CilOpCodes.Call, mappingAddMethod);
		}

		private static void AddLocalForLoadedByteArray(this CilInstructionCollection processor, out CilLocalVariable localVariable)
		{
			processor.Add(CilOpCodes.Call, byteArrayToYamlMethod);
			localVariable = new CilLocalVariable(CommonTypeGetter.YAMLNodeDefinition.ToTypeSignature());
			processor.Owner.LocalVariables.Add(localVariable);
			processor.Add(CilOpCodes.Stloc, localVariable);
		}

		private static void AddExportArrayField(this CilInstructionCollection processor, UnityNode node, FieldDefinition field, CilLocalVariable yamlMappingNode)
		{
			TypeSignature elementType = GenericTypeResolver.ResolveArrayType(node).BaseType;
			if (elementType is not ArrayTypeSignature && elementType.FullName == "System.Byte")
			{
				processor.AddExportByteArrayField(node, field, yamlMappingNode);
			}
			else
			{
				processor.AddLoadField(field);
				processor.AddLocalForLoadedArray(node, out CilLocalVariable sequenceNode);
				processor.Add(CilOpCodes.Ldloc, yamlMappingNode);
				processor.AddScalarNodeForString(node.OriginalName);
				processor.Add(CilOpCodes.Ldloc, sequenceNode);
				processor.Add(CilOpCodes.Call, mappingAddMethod);
			}
		}

		private static void AddLocalForLoadedArray(this CilInstructionCollection processor, UnityNode arrayNode, out CilLocalVariable sequenceNode)
		{
			UnityNode elementNode = arrayNode.SubNodes[1];
			SzArrayTypeSignature arrayType = GenericTypeResolver.ResolveArrayType(arrayNode);

			TypeSignature elementType = arrayType.BaseType;
			if (elementType is not ArrayTypeSignature && elementType.FullName == "System.Byte")
			{
				throw new NotSupportedException("Byte arrays not supported");
			}

			CilLocalVariable array = new CilLocalVariable(arrayType);
			processor.Owner.LocalVariables.Add(array);
			processor.Add(CilOpCodes.Stloc, array);

			sequenceNode = new CilLocalVariable(CommonTypeGetter.YAMLSequenceNodeDefinition.ToTypeSignature());
			processor.Owner.LocalVariables.Add(sequenceNode);
			processor.Add(CilOpCodes.Ldc_I4, 0); //SequenceStyle.Block
			processor.Add(CilOpCodes.Newobj, CommonTypeGetter.YAMLSequenceNodeConstructor);
			processor.Add(CilOpCodes.Stloc, sequenceNode);

			//Make local and store length in it
			CilLocalVariable? countLocal = new CilLocalVariable(SystemTypeGetter.Int32); //Create local
			processor.Owner.LocalVariables.Add(countLocal); //Add to method
			processor.Add(CilOpCodes.Ldloc, array); //Load array
			processor.Add(CilOpCodes.Ldlen); //Get length
			processor.Add(CilOpCodes.Stloc, countLocal); //Store it

			//Make an i
			CilLocalVariable? iLocal = new CilLocalVariable(SystemTypeGetter.Int32); //Create local
			processor.Owner.LocalVariables.Add(iLocal); //Add to method
			processor.Add(CilOpCodes.Ldc_I4_0); //Load 0 as an int32
			processor.Add(CilOpCodes.Stloc, iLocal); //Store in count

			//Create an empty, unconditional branch which will jump down to the loop condition.
			//This converts the do..while loop into a for loop.
			CilInstruction? unconditionalBranchInstruction = processor.Add(CilOpCodes.Br, DummyInstructionLabel);

			//Now we just read pair, increment i, compare against count, and jump back to here if it's less
			ICilLabel? jumpTargetLabel = processor.Add(CilOpCodes.Nop).CreateLabel(); //Create a dummy instruction to jump back to

			//Do stuff at index i
			processor.Add(CilOpCodes.Ldloc, array);
			processor.Add(CilOpCodes.Ldloc, iLocal);
			processor.AddLoadElement(elementType.ToTypeDefOrRef());
			processor.AddLocalForLoadedValue(elementNode, out CilLocalVariable elementLocal);

			processor.Add(CilOpCodes.Ldloc, sequenceNode);
			processor.Add(CilOpCodes.Ldloc, elementLocal);
			processor.Add(CilOpCodes.Call, sequenceAddMethod);

			//Increment i
			processor.Add(CilOpCodes.Ldloc, iLocal); //Load i local
			processor.Add(CilOpCodes.Ldc_I4_1); //Load constant 1 as int32
			processor.Add(CilOpCodes.Add); //Add 
			processor.Add(CilOpCodes.Stloc, iLocal); //Store in i local

			//Jump to start of loop if i < count
			ICilLabel? loopConditionStartLabel = processor.Add(CilOpCodes.Ldloc, iLocal).CreateLabel(); //Load i
			processor.Add(CilOpCodes.Ldloc, countLocal); //Load count
			processor.Add(CilOpCodes.Blt, jumpTargetLabel); //Jump back up if less than
			unconditionalBranchInstruction.Operand = loopConditionStartLabel;
		}

		private static void AddExportDictionaryField(this CilInstructionCollection processor, UnityNode node, FieldDefinition field, CilLocalVariable yamlMappingNode)
		{
			processor.AddLoadField(field);
			processor.AddLocalForLoadedDictionary(node, out CilLocalVariable dictionaryNodeLocal);
			processor.Add(CilOpCodes.Ldloc, yamlMappingNode);
			processor.AddScalarNodeForString(node.OriginalName);
			processor.Add(CilOpCodes.Ldloc, dictionaryNodeLocal);
			processor.Add(CilOpCodes.Call, mappingAddMethod);
		}

		private static void AddLocalForLoadedDictionary(this CilInstructionCollection processor, UnityNode dictionaryNode, out CilLocalVariable sequenceLocal)
		{
			UnityNode pairNode = dictionaryNode.SubNodes[0].SubNodes[1];
			UnityNode firstNode = dictionaryNode.SubNodes[0].SubNodes[1].SubNodes[0];
			UnityNode secondNode = dictionaryNode.SubNodes[0].SubNodes[1].SubNodes[1];

			GenericInstanceTypeSignature? genericDictType = GenericTypeResolver.ResolveDictionaryType(dictionaryNode);
			GenericInstanceTypeSignature? genericPairType = GenericTypeResolver.ResolvePairType(pairNode);
			GenericInstanceTypeSignature? genericListType = ((ITypeDefOrRef)SharedState.Importer.ImportSystemType("System.Collections.Generic.List`1")).ToTypeSignature().MakeGenericInstanceType(genericPairType);
			CilLocalVariable? dictLocal = new CilLocalVariable(genericDictType); //Create local
			processor.Owner.LocalVariables.Add(dictLocal); //Add to method
			processor.Add(CilOpCodes.Stloc, dictLocal); //Store dict in local

			sequenceLocal = new CilLocalVariable(CommonTypeGetter.YAMLSequenceNodeDefinition.ToTypeSignature());
			processor.Owner.LocalVariables.Add(sequenceLocal);
			processor.Add(CilOpCodes.Ldc_I4, 1); //SequenceStyle.BlockCurve
			processor.Add(CilOpCodes.Newobj, CommonTypeGetter.YAMLSequenceNodeConstructor);
			processor.Add(CilOpCodes.Stloc, sequenceLocal);

			//Get length of dictionary
			processor.Add(CilOpCodes.Ldloc, dictLocal);
			MethodDefinition getCountDefinition = SystemTypeGetter.LookupSystemType("System.Collections.Generic.List`1")!.Properties.Single(m => m.Name == "Count").GetMethod!;
			IMethodDefOrRef getCountReference = MethodUtils.MakeMethodOnGenericType(genericListType, getCountDefinition);
			processor.Add(CilOpCodes.Call, getCountReference);

			//Make local and store length in it
			CilLocalVariable countLocal = new CilLocalVariable(SystemTypeGetter.Int32); //Create local
			processor.Owner.LocalVariables.Add(countLocal); //Add to method
			processor.Add(CilOpCodes.Stloc, countLocal); //Store count in it

			//Make an i
			CilLocalVariable iLocal = new CilLocalVariable(SystemTypeGetter.Int32); //Create local
			processor.Owner.LocalVariables.Add(iLocal); //Add to method
			processor.Add(CilOpCodes.Ldc_I4_0); //Load 0 as an int32
			processor.Add(CilOpCodes.Stloc, iLocal); //Store in count

			//Create an empty, unconditional branch which will jump down to the loop condition.
			//This converts the do..while loop into a for loop.
			CilInstruction unconditionalBranchInstruction = processor.Add(CilOpCodes.Br, DummyInstructionLabel);

			//Now we just read key + value, increment i, compare against count, and jump back to here if it's less
			ICilLabel jumpTargetLabel = processor.Add(CilOpCodes.Nop).CreateLabel(); //Create the dummy instruction to jump back to

			//Export Yaml
			MethodDefinition getItemDefinition = SystemTypeGetter.LookupSystemType("System.Collections.Generic.List`1")!.Properties.Single(m => m.Name == "Item").GetMethod!;
			IMethodDefOrRef getItemReference = MethodUtils.MakeMethodOnGenericType(genericListType, getItemDefinition);
			processor.Add(CilOpCodes.Ldloc, dictLocal); //Load Dictionary
			processor.Add(CilOpCodes.Ldloc, iLocal); //Load i
			processor.Add(CilOpCodes.Call, getItemReference); //Get the i_th key value pair
			processor.AddLocalForLoadedPair(pairNode, out CilLocalVariable pairExportLocal); //Emit yaml node for that key value pair

			processor.Add(CilOpCodes.Ldloc, sequenceLocal);
			processor.Add(CilOpCodes.Ldloc, pairExportLocal);
			processor.Add(CilOpCodes.Call, sequenceAddMethod); //Call the add method

			//Increment i
			processor.Add(CilOpCodes.Ldloc, iLocal); //Load i local
			processor.Add(CilOpCodes.Ldc_I4_1); //Load constant 1 as int32
			processor.Add(CilOpCodes.Add); //Add 
			processor.Add(CilOpCodes.Stloc, iLocal); //Store in i local

			//Jump to start of loop if i < count
			ICilLabel? loopConditionStartLabel = processor.Add(CilOpCodes.Ldloc, iLocal).CreateLabel(); //Load i
			processor.Add(CilOpCodes.Ldloc, countLocal); //Load count
			processor.Add(CilOpCodes.Blt, jumpTargetLabel); //Jump back up if less than
			unconditionalBranchInstruction.Operand = loopConditionStartLabel;
		}

		private static void AddLoadField(this CilInstructionCollection processor, FieldDefinition field)
		{
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldfld, field);
		}

		private static void AddScalarNodeForString(this CilInstructionCollection processor, string name)
		{
			processor.Add(CilOpCodes.Ldstr, name);
			processor.Add(CilOpCodes.Newobj, GetScalarNodeConstructor(SystemTypeGetter.String));
		}

		private static void AddScalarNodeForLoadedPrimitiveValue(this CilInstructionCollection processor, string typeName)
		{
			CorLibTypeSignature? type = SystemTypeGetter.GetCppPrimitiveTypeSignature(typeName) ?? throw new ArgumentException(nameof(typeName));
			processor.Add(CilOpCodes.Newobj, GetScalarNodeConstructor(type));
		}

		private static void AddLocalForLoadedPrimitiveValue(this CilInstructionCollection processor, string typeName, out CilLocalVariable localVariable)
		{
			processor.AddScalarNodeForLoadedPrimitiveValue(typeName);
			localVariable = new CilLocalVariable(CommonTypeGetter.YAMLScalarNodeDefinition.ToTypeSignature());
			processor.Owner.LocalVariables.Add(localVariable);
			processor.Add(CilOpCodes.Stloc, localVariable);
		}

		private static MethodDefinition GetYamlExportMethod(this TypeDefinition type)
		{
			if (emittingRelease)
				return type.Methods.Single(m => m.Name == "ExportYAMLRelease");
			else
				return type.Methods.Single(m => m.Name == "ExportYAMLEditor");
		}

		private static IMethodDefOrRef GetScalarNodeConstructor(TypeSignature parameterType)
		{
			return TryGetScalarNodeConstructor(parameterType) ?? throw new InvalidOperationException($"Could not find a scalar node constructor for {parameterType.FullName}");
		}

		private static readonly Dictionary<TypeSignature, IMethodDefOrRef?> scalarNodeConstructorCache = new Dictionary<TypeSignature, IMethodDefOrRef?>();

		private static IMethodDefOrRef? TryGetScalarNodeConstructor(TypeSignature parameterType)
		{
			if(scalarNodeConstructorCache.TryGetValue(parameterType, out IMethodDefOrRef? constructor))
			{
				return constructor;
			}
			else
			{
				TypeDefinition scalarType = CommonTypeGetter.LookupCommonType<AssetRipper.Core.YAML.YAMLScalarNode>()
				?? throw new Exception("Could not find the yaml scalar node type");
				MethodDefinition? constructorDefinition = scalarType.Methods.SingleOrDefault(m =>
					m.IsConstructor &&
					m.Parameters.Count == 1 &&
					m.Parameters[0].ParameterType.FullName == parameterType.FullName);
				constructor = constructorDefinition == null ? null : SharedState.Importer.ImportMethod(constructorDefinition);
				scalarNodeConstructorCache.Add(parameterType, constructor);
				return constructor;
			}
			
		}

		private static void MaybeEmitFlowMappingStyle(this CilInstructionCollection processor, UnityNode rootNode, CilLocalVariable yamlMappingNode)
		{
			if (((TransferMetaFlags)rootNode.MetaFlag).IsTransferUsingFlowMappingStyle())
			{
				IMethodDefOrRef setter = SharedState.Importer.ImportCommonMethod<YAMLMappingNode>(m => m.Name == "set_Style");
				processor.Add(CilOpCodes.Ldloc, yamlMappingNode);
				processor.Add(CilOpCodes.Ldc_I4, (int)AssetRipper.Core.YAML.MappingStyle.Flow);
				processor.Add(CilOpCodes.Call, setter);
			}
		}

		private static void AddLoadElement(this CilInstructionCollection processor, ITypeDefOrRef elementType)
		{
			if(elementType is SzArrayTypeSignature)
			{
				processor.Add(CilOpCodes.Ldelem_Ref);
				return;
			}

			string elementTypeName = elementType.FullName;
			switch (elementTypeName)
			{
				case "System.Boolean":
					processor.Add(CilOpCodes.Ldelem_U1);
					return;
				case "System.SByte":
					processor.Add(CilOpCodes.Ldelem_I1);
					return;
				case "System.Byte":
					processor.Add(CilOpCodes.Ldelem_U1);
					return;
				case "System.Int16":
					processor.Add(CilOpCodes.Ldelem_I2);
					return;
				case "System.UInt16":
					processor.Add(CilOpCodes.Ldelem_U2);
					return;
				case "System.Int32":
					processor.Add(CilOpCodes.Ldelem_I4);
					return;
				case "System.UInt32":
					processor.Add(CilOpCodes.Ldelem_U4);
					return;
				case "System.Int64":
					processor.Add(CilOpCodes.Ldelem_I8);
					return;
				case "System.UInt64":
					throw new NotSupportedException();
				case "System.Single":
					processor.Add(CilOpCodes.Ldelem_R4);
					return;
				case "System.Double":
					processor.Add(CilOpCodes.Ldelem_R8);
					return;
				default:
					processor.Add(CilOpCodes.Ldelem_Ref);
					return;
			}
		}
	}
}
