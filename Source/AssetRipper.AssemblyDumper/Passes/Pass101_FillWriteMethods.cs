using AssetRipper.AssemblyDumper.Attributes;
using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.AssemblyDumper.Types;
using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.IO.Endian;
using System.Diagnostics;

namespace AssetRipper.AssemblyDumper.Passes;

public static class Pass101_FillWriteMethods
{
	private static IMethodDefOrRef? alignStreamMethod;
	private static IMethodDescriptor? writeInt32Method;
	private static ITypeDefOrRef? assetWriterReference;

	private static ITypeDefOrRef? assetDictionaryReference;
	private static TypeDefinition? assetDictionaryDefinition;
	private static ITypeDefOrRef? assetListReference;
	private static TypeDefinition? assetListDefinition;
	private static ITypeDefOrRef? keyValuePairReference;
	private static TypeDefinition? keyValuePairDefinition;

	private static MethodDefinition? writeAssetAlignDefinition;
	private static MethodDefinition? writeAssetListDefinition;
	private static MethodDefinition? writeAssetListAlignDefinition;

	private const string WriteRelease = nameof(UnityAssetBase.WriteRelease);
	private const string WriteEditor = nameof(UnityAssetBase.WriteEditor);
	private static string WriteMethod => emittingRelease ? WriteRelease : WriteEditor;

	private static readonly bool throwNotSupported = false;
	private static readonly Dictionary<string, IMethodDescriptor> methodDictionary = new();
	private static readonly SignatureComparer signatureComparer = new(SignatureComparisonFlags.VersionAgnostic);
	private static bool emittingRelease;

	private static void Initialize()
	{
		alignStreamMethod = SharedState.Instance.Importer.ImportMethod<EndianWriter>(m => m.Name == nameof(EndianWriter.AlignStream));
		writeInt32Method = ImportPrimitiveWriteMethod(ElementType.I4);
		assetWriterReference = SharedState.Instance.Importer.ImportType<AssetWriter>();

		assetDictionaryReference = SharedState.Instance.Importer.ImportType(typeof(AssetDictionary<,>));
		assetListReference = SharedState.Instance.Importer.ImportType(typeof(AssetList<>));
		keyValuePairReference = SharedState.Instance.Importer.ImportType(typeof(AssetPair<,>));

		assetDictionaryDefinition = SharedState.Instance.Importer.LookupType(typeof(AssetDictionary<,>));
		assetListDefinition = SharedState.Instance.Importer.LookupType(typeof(AssetList<>));
		keyValuePairDefinition = SharedState.Instance.Importer.LookupType(typeof(AssetPair<,>));
	}

	public static void DoPass()
	{
		Initialize();

		emittingRelease = true;
		methodDictionary.Clear();
		writeAssetAlignDefinition = MakeGenericAssetAlignMethod();
		writeAssetListDefinition = MakeGenericListMethod(false);
		writeAssetListAlignDefinition = MakeGenericListMethod(true);
		foreach (ClassGroupBase group in SharedState.Instance.AllGroups)
		{
			foreach (GeneratedClassInstance instance in group.Instances)
			{
				instance.Type.FillReleaseWriteMethod(instance.Class, instance.VersionRange.Start);
			}
		}
		CreateHelperClassForWriteMethods();

		emittingRelease = false;
		methodDictionary.Clear();
		writeAssetAlignDefinition = MakeGenericAssetAlignMethod();
		writeAssetListDefinition = MakeGenericListMethod(false);
		writeAssetListAlignDefinition = MakeGenericListMethod(true);
		foreach (ClassGroupBase group in SharedState.Instance.AllGroups)
		{
			foreach (GeneratedClassInstance instance in group.Instances)
			{
				instance.Type.FillEditorWriteMethod(instance.Class, instance.VersionRange.Start);
			}
		}
		CreateHelperClassForWriteMethods();

		methodDictionary.Clear();
	}

	private static void CreateHelperClassForWriteMethods()
	{
		TypeDefinition type = StaticClassCreator.CreateEmptyStaticClass(SharedState.Instance.Module, SharedState.HelpersNamespace, $"{WriteMethod}Methods");
		type.IsPublic = false;
		type.Methods.Add(writeAssetAlignDefinition!);
		type.Methods.Add(writeAssetListDefinition!);
		type.Methods.Add(writeAssetListAlignDefinition!);
		foreach ((string _, IMethodDescriptor method) in methodDictionary.OrderBy(pair => pair.Key))
		{
			if (method is MethodDefinition methodDefinition && methodDefinition.DeclaringType is null)
			{
				type.Methods.Add(methodDefinition);
			}
		}
		Console.WriteLine($"\t{type.Methods.Count} {WriteMethod} helper methods");
	}

	private static void FillEditorWriteMethod(this TypeDefinition type, UniversalClass klass, UnityVersion version)
	{
		MethodDefinition method = type.Methods.First(m => m.Name == WriteEditor);
		CilInstructionCollection instructions = method.GetInstructions();

		if (throwNotSupported)
		{
			instructions.AddNotSupportedException();
		}
		else
		{
			if (klass.EditorRootNode != null)
			{
				foreach (UniversalNode unityNode in klass.EditorRootNode.SubNodes)
				{
					FieldDefinition field = type.GetFieldByName(unityNode.Name, true);
					IMethodDescriptor fieldWriteMethod = GetOrMakeMethod(unityNode, field.Signature!.FieldType, version);
					instructions.Add(CilOpCodes.Ldarg_0);//this
					instructions.Add(CilOpCodes.Ldfld, field);
					instructions.Add(CilOpCodes.Ldarg_1);//writer
					instructions.AddCall(fieldWriteMethod);
				}
			}
			instructions.Add(CilOpCodes.Ret);
		}
		instructions.OptimizeMacros();
	}

	private static void FillReleaseWriteMethod(this TypeDefinition type, UniversalClass klass, UnityVersion version)
	{
		MethodDefinition method = type.Methods.First(m => m.Name == WriteRelease);
		CilInstructionCollection instructions = method.GetInstructions();

		if (throwNotSupported)
		{
			instructions.AddNotSupportedException();
		}
		else
		{
			if (klass.ReleaseRootNode != null)
			{
				foreach (UniversalNode unityNode in klass.ReleaseRootNode.SubNodes)
				{
					FieldDefinition field = type.GetFieldByName(unityNode.Name, true);
					IMethodDescriptor fieldWriteMethod = GetOrMakeMethod(unityNode, field.Signature!.FieldType, version);
					instructions.Add(CilOpCodes.Ldarg_0);//this
					instructions.Add(CilOpCodes.Ldfld, field);
					instructions.Add(CilOpCodes.Ldarg_1);//writer
					instructions.AddCall(fieldWriteMethod);
				}
			}
			instructions.Add(CilOpCodes.Ret);
		}
		instructions.OptimizeMacros();
	}

	private static IMethodDescriptor GetOrMakeMethod(UniversalNode node, TypeSignature type, UnityVersion version)
	{
		string uniqueName = UniqueNameFactory.GetReadWriteName(node, version);
		if (methodDictionary.TryGetValue(uniqueName, out IMethodDescriptor? method))
		{
			return method;
		}

		if (SharedState.Instance.SubclassGroups.TryGetValue(node.TypeName, out SubclassGroup? subclassGroup))
		{
			TypeDefinition typeDefinition = subclassGroup.GetTypeForVersion(version);
			Debug.Assert(signatureComparer.Equals(typeDefinition.ToTypeSignature(), type));
			MethodDefinition typeWriteMethod = typeDefinition.GetMethodByName(WriteMethod);
			method = node.AlignBytes ? writeAssetAlignDefinition!.MakeGenericInstanceMethod(type) : typeWriteMethod;
			methodDictionary.Add(uniqueName, method);
			return method;
		}

		switch (node.NodeType)
		{
			case NodeType.Vector:
				{
					UniversalNode arrayNode = node.SubNodes[0];
					UniversalNode elementTypeNode = arrayNode.SubNodes[1];
					bool align = node.AlignBytes || arrayNode.AlignBytes;
					if (type is GenericInstanceTypeSignature genericSignature)
					{
						Debug.Assert(genericSignature.GenericType.Name == $"{nameof(AssetList<int>)}`1");
						Debug.Assert(genericSignature.TypeArguments.Count == 1);
						method = MakeListMethod(uniqueName, elementTypeNode, genericSignature.TypeArguments[0], version, align);
					}
					else
					{
						SzArrayTypeSignature arrayType = (SzArrayTypeSignature)type;
						TypeSignature elementType = arrayType.BaseType;
						method = MakeArrayMethod(uniqueName, elementTypeNode, elementType, version, align);
					}
				}
				break;
			case NodeType.Map:
				{
					UniversalNode arrayNode = node.SubNodes[0];
					UniversalNode pairNode = arrayNode.SubNodes[1];
					UniversalNode firstTypeNode = pairNode.SubNodes[0];
					UniversalNode secondTypeNode = pairNode.SubNodes[1];
					bool align = node.AlignBytes || arrayNode.AlignBytes;
					GenericInstanceTypeSignature genericSignature = (GenericInstanceTypeSignature)type;
					Debug.Assert(genericSignature.GenericType.Name == $"{nameof(AssetDictionary<int, int>)}`2");
					Debug.Assert(genericSignature.TypeArguments.Count == 2);
					method = MakeDictionaryMethod(uniqueName, firstTypeNode, genericSignature.TypeArguments[0], secondTypeNode, genericSignature.TypeArguments[1], version, align);
				}
				break;
			case NodeType.Pair:
				{
					UniversalNode firstTypeNode = node.SubNodes[0];
					UniversalNode secondTypeNode = node.SubNodes[1];
					bool align = node.AlignBytes;
					GenericInstanceTypeSignature genericSignature = (GenericInstanceTypeSignature)type;
					Debug.Assert(genericSignature.GenericType.Name == $"{nameof(AssetPair<int, int>)}`2");
					Debug.Assert(genericSignature.TypeArguments.Count == 2);
					method = MakePairMethod(uniqueName, firstTypeNode, genericSignature.TypeArguments[0], secondTypeNode, genericSignature.TypeArguments[1], version, align);
				}
				break;
			case NodeType.TypelessData: //byte array
				{
					method = MakeTypelessDataMethod(uniqueName, node.AlignBytes);
				}
				break;
			case NodeType.Array:
				{
					UniversalNode elementTypeNode = node.SubNodes[1];
					bool align = node.AlignBytes;
					if (type is GenericInstanceTypeSignature genericSignature)
					{
						Debug.Assert(genericSignature.GenericType.Name == $"{nameof(AssetList<int>)}`1");
						Debug.Assert(genericSignature.TypeArguments.Count == 1);
						method = MakeListMethod(uniqueName, elementTypeNode, genericSignature.TypeArguments[0], version, align);
					}
					else
					{
						SzArrayTypeSignature arrayType = (SzArrayTypeSignature)type;
						TypeSignature elementType = arrayType.BaseType;
						method = MakeArrayMethod(uniqueName, elementTypeNode, elementType, version, align);
					}
				}
				break;
			default:
				method = MakePrimitiveMethod(uniqueName, node, node.AlignBytes);
				break;
		}

		if (method is null)
		{
			throw new InvalidOperationException();
		}

		methodDictionary.Add(uniqueName, method);
		return method;
	}

	private static MethodDefinition MakeGenericAssetAlignMethod()
	{
		string uniqueName = "AssetAlign";
		GenericParameterSignature elementType = new GenericParameterSignature(SharedState.Instance.Module, GenericParameterType.Method, 0);
		IMethodDefOrRef writeMethod = SharedState.Instance.Importer.ImportMethod<UnityAssetBase>(m => m.Name == WriteMethod);
		MethodDefinition method = NewWriteMethod(uniqueName, elementType);

		CilInstructionCollection instructions = method.GetInstructions();
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.Add(CilOpCodes.Callvirt, writeMethod);
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.AddCall(alignStreamMethod!);
		instructions.Add(CilOpCodes.Ret);

		GenericParameter genericParameter = new GenericParameter("T", GenericParameterAttributes.DefaultConstructorConstraint);
		genericParameter.Constraints.Add(new GenericParameterConstraint(SharedState.Instance.Importer.ImportType<UnityAssetBase>()));
		method.GenericParameters.Add(genericParameter);
		method.Signature!.GenericParameterCount = 1;
		method.Signature.IsGeneric = true;

		return method;
	}

	private static IMethodDescriptor? MakeTypelessDataMethod(string uniqueName, bool align)
	{
		IMethodDefOrRef writeBytesMethod = SharedState.Instance.Importer.ImportMethod<BinaryWriter>(m =>
		{
			return m.Name == nameof(BinaryWriter.Write)
				&& m.Parameters.Count == 1
				&& m.Parameters[0].ParameterType is SzArrayTypeSignature arrayTypeSignature
				&& arrayTypeSignature.BaseType is CorLibTypeSignature corLibTypeSignature
				&& corLibTypeSignature.ElementType == ElementType.U1;
		});
		MethodDefinition method = NewWriteMethod(uniqueName, SharedState.Instance.Importer.UInt8.MakeSzArrayType());
		CilInstructionCollection instructions = method.GetInstructions();
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldlen);
		instructions.AddCall(writeInt32Method!);

		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.AddCall(writeBytesMethod);
		if (align)
		{
			instructions.Add(CilOpCodes.Ldarg_1);
			instructions.AddCall(alignStreamMethod!);
		}
		instructions.Add(CilOpCodes.Ret);
		return method;
	}

	private static IMethodDescriptor? MakePrimitiveMethod(string uniqueName, UniversalNode node, bool align)
	{
		IMethodDescriptor primitiveMethod = GetPrimitiveMethod(node);
		MethodDefinition method = NewWriteMethod(uniqueName, primitiveMethod.Signature!.ParameterTypes[0]);
		CilInstructionCollection instructions = method.GetInstructions();
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.AddCall(primitiveMethod);
		if (align)
		{
			instructions.Add(CilOpCodes.Ldarg_1);
			instructions.AddCall(alignStreamMethod!);
		}
		instructions.Add(CilOpCodes.Ret);
		return method;
	}

	private static IMethodDescriptor? MakePairMethod(string uniqueName, UniversalNode firstTypeNode, TypeSignature typeSignature1, UniversalNode secondTypeNode, TypeSignature typeSignature2, UnityVersion version, bool align)
	{
		IMethodDescriptor firstWriteMethod = GetOrMakeMethod(firstTypeNode, typeSignature1, version);
		IMethodDescriptor secondWriteMethod = GetOrMakeMethod(secondTypeNode, typeSignature2, version);

		GenericInstanceTypeSignature genericPairType = keyValuePairReference!.MakeGenericInstanceType(typeSignature1, typeSignature2);

		IMethodDefOrRef keyMethod = MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			genericPairType,
			keyValuePairDefinition!.Methods.Single(m => m.Name == $"get_{nameof(AssetPair<int, int>.Key)}"));

		IMethodDefOrRef valueMethod = MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			genericPairType,
			keyValuePairDefinition!.Methods.Single(m => m.Name == $"get_{nameof(AssetPair<int, int>.Value)}"));

		MethodDefinition method = NewWriteMethod(uniqueName, genericPairType);
		CilInstructionCollection instructions = method.GetInstructions();
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.AddCall(keyMethod);
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.AddCall(firstWriteMethod);
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.AddCall(valueMethod);
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.AddCall(secondWriteMethod);
		if (align)
		{
			instructions.Add(CilOpCodes.Ldarg_1);
			instructions.AddCall(alignStreamMethod!);
		}
		instructions.Add(CilOpCodes.Ret);
		return method;
	}

	private static IMethodDescriptor? MakeArrayMethod(string uniqueName, UniversalNode elementTypeNode, TypeSignature elementType, UnityVersion version, bool align)
	{
		IMethodDescriptor elementWriteMethod = GetOrMakeMethod(elementTypeNode, elementType, version);

		SzArrayTypeSignature arrayTypeSignature = elementType.MakeSzArrayType();

		MethodDefinition method = NewWriteMethod(uniqueName, arrayTypeSignature);
		CilInstructionCollection instructions = method.GetInstructions();

		//Read length of array
		instructions.Add(CilOpCodes.Ldarg_0); //Load array
		instructions.Add(CilOpCodes.Ldlen); //Get length

		//Make local and store length in it
		CilLocalVariable countLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32); //Create local
		instructions.Add(CilOpCodes.Stloc, countLocal); //Store count in it

		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.Add(CilOpCodes.Ldloc, countLocal);
		instructions.AddCall(writeInt32Method!);//Write the count

		//Make an i
		CilLocalVariable iLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32); //Create local
		instructions.Add(CilOpCodes.Ldc_I4_0); //Load 0 as an int32
		instructions.Add(CilOpCodes.Stloc, iLocal); //Store in count

		CilInstructionLabel loopConditionStartLabel = new(); //Needed later

		//Create an empty, unconditional branch which will jump down to the loop condition.
		//This converts the do..while loop into a for loop.
		instructions.Add(CilOpCodes.Br, loopConditionStartLabel);

		//Now we just read pair, increment i, compare against count, and jump back to here if it's less
		ICilLabel jumpTarget = instructions.Add(CilOpCodes.Nop).CreateLabel(); //Create a dummy instruction to jump back to

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldloc, iLocal);
		instructions.AddLoadElement(elementType);
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.AddCall(elementWriteMethod);

		//Increment i
		instructions.Add(CilOpCodes.Ldloc, iLocal); //Load i local
		instructions.Add(CilOpCodes.Ldc_I4_1); //Load constant 1 as int32
		instructions.Add(CilOpCodes.Add); //Add 
		instructions.Add(CilOpCodes.Stloc, iLocal); //Store in i local

		//Jump to start of loop if i < count
		loopConditionStartLabel.Instruction = instructions.Add(CilOpCodes.Ldloc, iLocal); //Load i
		instructions.Add(CilOpCodes.Ldloc, countLocal); //Load count
		instructions.Add(CilOpCodes.Blt, jumpTarget); //Jump back up if less than

		if (align)
		{
			instructions.Add(CilOpCodes.Ldarg_1);
			instructions.AddCall(alignStreamMethod!);
		}
		instructions.Add(CilOpCodes.Ret);
		return method;
	}

	private static IMethodDescriptor MakeListMethod(string uniqueName, UniversalNode elementTypeNode, TypeSignature elementType, UnityVersion version, bool align)
	{
		if (elementType is TypeDefOrRefSignature typeDefOrRefSignature && typeDefOrRefSignature.Type is TypeDefinition)
		{
			return align
				? writeAssetListAlignDefinition!.MakeGenericInstanceMethod(elementType)
				: writeAssetListDefinition!.MakeGenericInstanceMethod(elementType);
		}
		else
		{
			IMethodDescriptor elementWriteMethod = GetOrMakeMethod(elementTypeNode, elementType, version);
			return MakeListMethod(uniqueName, elementType, elementWriteMethod, align);
		}
	}

	private static MethodDefinition MakeGenericListMethod(bool align)
	{
		string uniqueName = align ? "ArrayAlign_Asset" : "Array_Asset";
		GenericParameterSignature elementType = new GenericParameterSignature(SharedState.Instance.Module, GenericParameterType.Method, 0);
		IMethodDefOrRef writeMethod = SharedState.Instance.Importer.ImportMethod<UnityAssetBase>(m => m.Name == WriteMethod);
		MethodDefinition method = MakeListMethod(uniqueName, elementType, writeMethod, align);

		GenericParameter genericParameter = new GenericParameter("T", GenericParameterAttributes.DefaultConstructorConstraint);
		genericParameter.Constraints.Add(new GenericParameterConstraint(SharedState.Instance.Importer.ImportType<UnityAssetBase>()));
		method.GenericParameters.Add(genericParameter);
		method.Signature!.GenericParameterCount = 1;
		method.Signature.IsGeneric = true;

		return method;
	}

	private static MethodDefinition MakeListMethod(string uniqueName, TypeSignature elementType, IMethodDescriptor elementWriteMethod, bool align)
	{
		GenericInstanceTypeSignature genericListType = assetListReference!.MakeGenericInstanceType(elementType);

		IMethodDefOrRef countMethod = MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			genericListType,
			assetListDefinition!.Methods.Single(m => m.Name == $"get_{nameof(AssetList<int>.Count)}"));

		IMethodDefOrRef getElementMethod = MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			genericListType,
			assetListDefinition!.Methods.Single(m => m.Name == "get_Item"));

		MethodDefinition method = NewWriteMethod(uniqueName, genericListType);
		CilInstructionCollection instructions = method.GetInstructions();

		//Load Count
		instructions.Add(CilOpCodes.Ldarg_0); //Load list
		instructions.AddCall(countMethod); //Call get Count

		//Make local and store length in it
		CilLocalVariable countLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32); //Create local
		instructions.Add(CilOpCodes.Stloc, countLocal); //Store count in it

		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.Add(CilOpCodes.Ldloc, countLocal);
		instructions.AddCall(writeInt32Method!);//Write the count

		//Make an i
		CilLocalVariable iLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32); //Create local
		instructions.Add(CilOpCodes.Ldc_I4_0); //Load 0 as an int32
		instructions.Add(CilOpCodes.Stloc, iLocal); //Store in count

		CilInstructionLabel loopConditionStartLabel = new(); //Needed later

		//Create an empty, unconditional branch which will jump down to the loop condition.
		//This converts the do..while loop into a for loop.
		instructions.Add(CilOpCodes.Br, loopConditionStartLabel);

		//Now we just read pair, increment i, compare against count, and jump back to here if it's less
		ICilLabel jumpTarget = instructions.Add(CilOpCodes.Nop).CreateLabel(); //Create a dummy instruction to jump back to

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldloc, iLocal);
		instructions.AddCall(getElementMethod);
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.AddCall(elementWriteMethod);

		//Increment i
		instructions.Add(CilOpCodes.Ldloc, iLocal); //Load i local
		instructions.Add(CilOpCodes.Ldc_I4_1); //Load constant 1 as int32
		instructions.Add(CilOpCodes.Add); //Add 
		instructions.Add(CilOpCodes.Stloc, iLocal); //Store in i local

		//Jump to start of loop if i < count
		loopConditionStartLabel.Instruction = instructions.Add(CilOpCodes.Ldloc, iLocal); //Load i
		instructions.Add(CilOpCodes.Ldloc, countLocal); //Load count
		instructions.Add(CilOpCodes.Blt, jumpTarget); //Jump back up if less than

		if (align)
		{
			instructions.Add(CilOpCodes.Ldarg_1);
			instructions.AddCall(alignStreamMethod!);
		}
		instructions.Add(CilOpCodes.Ret);
		return method;
	}

	private static IMethodDescriptor? MakeDictionaryMethod(string uniqueName, UniversalNode firstTypeNode, TypeSignature typeSignature1, UniversalNode secondTypeNode, TypeSignature typeSignature2, UnityVersion version, bool align)
	{
		IMethodDescriptor firstWriteMethod = GetOrMakeMethod(firstTypeNode, typeSignature1, version);
		IMethodDescriptor secondWriteMethod = GetOrMakeMethod(secondTypeNode, typeSignature2, version);

		GenericInstanceTypeSignature genericPairType = keyValuePairReference!.MakeGenericInstanceType(typeSignature1, typeSignature2);

		IMethodDefOrRef keyMethod = MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			genericPairType,
			keyValuePairDefinition!.Methods.Single(m => m.Name == $"get_{nameof(AssetPair<int, int>.Key)}"));

		IMethodDefOrRef valueMethod = MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			genericPairType,
			keyValuePairDefinition!.Methods.Single(m => m.Name == $"get_{nameof(AssetPair<int, int>.Value)}"));

		GenericInstanceTypeSignature genericDictionaryType = assetDictionaryReference!.MakeGenericInstanceType(typeSignature1, typeSignature2);

		IMethodDefOrRef countMethod = MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			genericDictionaryType,
			assetDictionaryDefinition!.Methods.Single(m => m.Name == $"get_{nameof(AssetDictionary<int, int>.Count)}"));

		IMethodDefOrRef getPairMethod = MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			genericDictionaryType,
			assetDictionaryDefinition!.Methods.Single(m => m.Name == nameof(AssetDictionary<int, int>.GetPair)));

		MethodDefinition method = NewWriteMethod(uniqueName, genericDictionaryType);
		CilInstructionCollection instructions = method.GetInstructions();

		//Load Count
		instructions.Add(CilOpCodes.Ldarg_0); //Load dictionary
		instructions.AddCall(countMethod); //Call get Count

		//Make local and store length in it
		CilLocalVariable countLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32); //Create local
		instructions.Add(CilOpCodes.Stloc, countLocal); //Store count in it

		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.Add(CilOpCodes.Ldloc, countLocal);
		instructions.AddCall(writeInt32Method!);//Write the count

		//Make an i
		CilLocalVariable iLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32); //Create local
		instructions.Add(CilOpCodes.Ldc_I4_0); //Load 0 as an int32
		instructions.Add(CilOpCodes.Stloc, iLocal); //Store in count

		CilInstructionLabel loopConditionStartLabel = new(); //Needed later

		//Create an empty, unconditional branch which will jump down to the loop condition.
		//This converts the do..while loop into a for loop.
		instructions.Add(CilOpCodes.Br, loopConditionStartLabel);

		//Now we just read pair, increment i, compare against count, and jump back to here if it's less
		ICilLabel jumpTarget = instructions.Add(CilOpCodes.Nop).CreateLabel(); //Create a dummy instruction to jump back to

		CilLocalVariable pairLocal = instructions.AddLocalVariable(genericPairType);
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldloc, iLocal);
		instructions.AddCall(getPairMethod);
		instructions.Add(CilOpCodes.Stloc, pairLocal);

		instructions.Add(CilOpCodes.Ldloc, pairLocal);
		instructions.AddCall(keyMethod);
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.AddCall(firstWriteMethod);
		instructions.Add(CilOpCodes.Ldloc, pairLocal);
		instructions.AddCall(valueMethod);
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.AddCall(secondWriteMethod);

		//Increment i
		instructions.Add(CilOpCodes.Ldloc, iLocal); //Load i local
		instructions.Add(CilOpCodes.Ldc_I4_1); //Load constant 1 as int32
		instructions.Add(CilOpCodes.Add); //Add 
		instructions.Add(CilOpCodes.Stloc, iLocal); //Store in i local

		//Jump to start of loop if i < count
		loopConditionStartLabel.Instruction = instructions.Add(CilOpCodes.Ldloc, iLocal); //Load i
		instructions.Add(CilOpCodes.Ldloc, countLocal); //Load count
		instructions.Add(CilOpCodes.Blt, jumpTarget); //Jump back up if less than

		if (align)
		{
			instructions.Add(CilOpCodes.Ldarg_1);
			instructions.AddCall(alignStreamMethod!);
		}
		instructions.Add(CilOpCodes.Ret);
		return method;
	}

	private static IMethodDescriptor GetPrimitiveMethod(UniversalNode node)
	{
		return node.NodeType switch
		{
			NodeType.Boolean => ImportPrimitiveWriteMethod(ElementType.Boolean),
			NodeType.Character => ImportPrimitiveWriteMethod(ElementType.Char),
			NodeType.Int8 => ImportPrimitiveWriteMethod(ElementType.I1),
			NodeType.UInt8 => ImportPrimitiveWriteMethod(ElementType.U1),
			NodeType.Int16 => ImportPrimitiveWriteMethod(ElementType.I2),
			NodeType.UInt16 => ImportPrimitiveWriteMethod(ElementType.U2),
			NodeType.Int32 => writeInt32Method!,
			NodeType.UInt32 => ImportPrimitiveWriteMethod(ElementType.U4),
			NodeType.Int64 => ImportPrimitiveWriteMethod(ElementType.I8),
			NodeType.UInt64 => ImportPrimitiveWriteMethod(ElementType.U8),
			NodeType.Single => ImportPrimitiveWriteMethod(ElementType.R4),
			NodeType.Double => ImportPrimitiveWriteMethod(ElementType.R8),
			NodeType.String => SharedState.Instance.Importer.ImportMethod<EndianWriter>(m =>
			{
				return m.Name == nameof(EndianWriter.Write)
					&& m.Parameters.Count == 1
					&& m.Parameters[0].ParameterType is TypeDefOrRefSignature { Namespace: "AssetRipper.Primitives", Name: nameof(Utf8String) };
			}),
			_ => throw new NotSupportedException(node.TypeName),
		};
	}

	private static IMethodDescriptor ImportPrimitiveWriteMethod(ElementType elementType)
	{
		if (elementType is ElementType.Boolean or ElementType.U1 or ElementType.I1)
		{
			return SharedState.Instance.Importer.ImportMethod<BinaryWriter>(m =>
			{
				return m.Name == nameof(BinaryWriter.Write)
					&& m.Parameters.Count == 1
					&& m.Parameters[0].ParameterType is CorLibTypeSignature corLibTypeSignature
					&& corLibTypeSignature.ElementType == elementType;
			});
		}
		else
		{
			return SharedState.Instance.Importer.ImportMethod<EndianWriter>(m =>
			{
				return m.Name == nameof(EndianWriter.Write)
					&& m.Parameters.Count == 1
					&& m.Parameters[0].ParameterType is CorLibTypeSignature corLibTypeSignature
					&& corLibTypeSignature.ElementType == elementType;
			});
		}
	}

	private static MethodDefinition NewWriteMethod(string uniqueName, TypeSignature parameter)
	{
		MethodSignature methodSignature = MethodSignature.CreateStatic(SharedState.Instance.Importer.Void);
		MethodDefinition method = new MethodDefinition($"{WriteMethod}_{uniqueName}", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, methodSignature);
		method.CilMethodBody = new CilMethodBody();
		method.AddParameter(parameter, "value");
		method.AddParameter(assetWriterReference!.ToTypeSignature(), "writer");
		method.AddExtensionAttribute(SharedState.Instance.Importer);
		return method;
	}

	private static CilInstruction AddCall(this CilInstructionCollection instructions, IMethodDescriptor method)
	{
		return method is MethodDefinition definition && definition.IsStatic
			? instructions.Add(CilOpCodes.Call, method)
			: instructions.Add(CilOpCodes.Callvirt, method);
	}
}
