using AssetRipper.AssemblyDumper.Attributes;
using AssetRipper.AssemblyDumper.InjectedTypes;
using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.AssemblyDumper.Types;
using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.IO.Endian;
using System.Diagnostics;

namespace AssetRipper.AssemblyDumper.Passes;

public static class Pass100_FillReadMethods
{
#nullable disable
	private static IMethodDefOrRef alignStreamMethod;
	private static IMethodDefOrRef readInt32Method;
	private static IMethodDefOrRef readBytesMethod;
	/// <summary>
	/// TypeSignature for <see langword="ref"/> <see cref="EndianSpanReader"/>
	/// </summary>
	private static TypeSignature endianSpanReaderReference;

	private static ITypeDefOrRef assetDictionaryReference;
	private static TypeDefinition assetDictionaryDefinition;
	private static ITypeDefOrRef assetListReference;
	private static TypeDefinition assetListDefinition;
	private static ITypeDefOrRef assetPairReference;
	private static TypeDefinition assetPairDefinition;

	private static MethodDefinition readAssetAlignDefinition;
	private static MethodDefinition readAssetListDefinition;
	private static MethodDefinition readAssetListAlignDefinition;
	private static MethodDefinition readAssetPairDefinition;
	private static MethodDefinition readAssetPairAlignDefinition;
	private static MethodDefinition readAssetDictionaryDefinition;
	private static MethodDefinition readAssetDictionaryAlignDefinition;
#nullable enable

	private static readonly Dictionary<ElementType, IMethodDefOrRef> primitiveReadMethods = new();

	private static string ReadMethod => emittingRelease ? ReadRelease : ReadEditor;
	private const string ReadRelease = nameof(UnityAssetBase.ReadRelease);
	private const string ReadEditor = nameof(UnityAssetBase.ReadEditor);
	private const int MaxArraySize = 1024;

	private static readonly Dictionary<string, IMethodDescriptor> methodDictionary = new();
	private static readonly SignatureComparer signatureComparer = new(SignatureComparisonFlags.VersionAgnostic);
	private static bool emittingRelease;

	public static void DoPass()
	{
		methodDictionary.Clear();
		Initialize();

		emittingRelease = true;
		readAssetAlignDefinition = MakeGenericAssetAlignMethod();
		readAssetListDefinition = MakeGenericListMethod(false);
		readAssetListAlignDefinition = MakeGenericListMethod(true);
		readAssetPairDefinition = MakeGenericPairMethod(false);
		readAssetPairAlignDefinition = MakeGenericPairMethod(true);
		readAssetDictionaryDefinition = MakeGenericDictionaryMethod(false);
		readAssetDictionaryAlignDefinition = MakeGenericDictionaryMethod(true);
		foreach (ClassGroupBase group in SharedState.Instance.AllGroups)
		{
			foreach (GeneratedClassInstance instance in group.Instances)
			{
				instance.Type.FillReleaseWriteMethod(instance.Class, instance.VersionRange.Start);
			}
		}
		CreateHelperClassForWriteMethods();
		methodDictionary.Clear();

		emittingRelease = false;
		readAssetAlignDefinition = MakeGenericAssetAlignMethod();
		readAssetListDefinition = MakeGenericListMethod(false);
		readAssetListAlignDefinition = MakeGenericListMethod(true);
		readAssetPairDefinition = MakeGenericPairMethod(false);
		readAssetPairAlignDefinition = MakeGenericPairMethod(true);
		readAssetDictionaryDefinition = MakeGenericDictionaryMethod(false);
		readAssetDictionaryAlignDefinition = MakeGenericDictionaryMethod(true);
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

	private static void Initialize()
	{
		primitiveReadMethods.Add(ElementType.Boolean, SharedState.Instance.Importer.ImportMethod(typeof(EndianSpanReader), m => m.Name == nameof(EndianSpanReader.ReadBoolean)));
		primitiveReadMethods.Add(ElementType.Char, SharedState.Instance.Importer.ImportMethod(typeof(EndianSpanReader), m => m.Name == nameof(EndianSpanReader.ReadChar)));
		primitiveReadMethods.Add(ElementType.I1, SharedState.Instance.Importer.ImportMethod(typeof(EndianSpanReader), m => m.Name == nameof(EndianSpanReader.ReadSByte)));
		primitiveReadMethods.Add(ElementType.U1, SharedState.Instance.Importer.ImportMethod(typeof(EndianSpanReader), m => m.Name == nameof(EndianSpanReader.ReadByte)));
		primitiveReadMethods.Add(ElementType.I2, SharedState.Instance.Importer.ImportMethod(typeof(EndianSpanReader), m => m.Name == nameof(EndianSpanReader.ReadInt16)));
		primitiveReadMethods.Add(ElementType.U2, SharedState.Instance.Importer.ImportMethod(typeof(EndianSpanReader), m => m.Name == nameof(EndianSpanReader.ReadUInt16)));
		primitiveReadMethods.Add(ElementType.I4, SharedState.Instance.Importer.ImportMethod(typeof(EndianSpanReader), m => m.Name == nameof(EndianSpanReader.ReadInt32)));
		primitiveReadMethods.Add(ElementType.U4, SharedState.Instance.Importer.ImportMethod(typeof(EndianSpanReader), m => m.Name == nameof(EndianSpanReader.ReadUInt32)));
		primitiveReadMethods.Add(ElementType.I8, SharedState.Instance.Importer.ImportMethod(typeof(EndianSpanReader), m => m.Name == nameof(EndianSpanReader.ReadInt64)));
		primitiveReadMethods.Add(ElementType.U8, SharedState.Instance.Importer.ImportMethod(typeof(EndianSpanReader), m => m.Name == nameof(EndianSpanReader.ReadUInt64)));
		primitiveReadMethods.Add(ElementType.R4, SharedState.Instance.Importer.ImportMethod(typeof(EndianSpanReader), m => m.Name == nameof(EndianSpanReader.ReadSingle)));
		primitiveReadMethods.Add(ElementType.R8, SharedState.Instance.Importer.ImportMethod(typeof(EndianSpanReader), m => m.Name == nameof(EndianSpanReader.ReadDouble)));
		primitiveReadMethods.Add(ElementType.String, SharedState.Instance.Importer.ImportMethod(typeof(EndianSpanReader), m => m.Name == nameof(EndianSpanReader.ReadUtf8String)));

		readInt32Method = primitiveReadMethods[ElementType.I4];

		alignStreamMethod = SharedState.Instance.Importer.ImportMethod(typeof(EndianSpanReader), m => m.Name == nameof(EndianSpanReader.Align));
		endianSpanReaderReference = SharedState.Instance.Importer.ImportTypeSignature(typeof(EndianSpanReader)).MakeByReferenceType();

		readBytesMethod = SharedState.Instance.InjectHelperType(typeof(TypelessDataHelper)).Methods.Single();

		assetDictionaryReference = SharedState.Instance.Importer.ImportType(typeof(AssetDictionary<,>));
		assetListReference = SharedState.Instance.Importer.ImportType(typeof(AssetList<>));
		assetPairReference = SharedState.Instance.Importer.ImportType(typeof(AssetPair<,>));

		assetDictionaryDefinition = SharedState.Instance.Importer.LookupType(typeof(AssetDictionary<,>));
		assetListDefinition = SharedState.Instance.Importer.LookupType(typeof(AssetList<>));
		assetPairDefinition = SharedState.Instance.Importer.LookupType(typeof(AssetPair<,>));
	}

	private static void CreateHelperClassForWriteMethods()
	{
		TypeDefinition type = StaticClassCreator.CreateEmptyStaticClass(SharedState.Instance.Module, SharedState.HelpersNamespace, $"{ReadMethod}Methods");
		type.IsPublic = false;
		type.Methods.Add(readAssetAlignDefinition);
		type.Methods.Add(readAssetListDefinition);
		type.Methods.Add(readAssetListAlignDefinition);
		type.Methods.Add(readAssetPairDefinition);
		type.Methods.Add(readAssetPairAlignDefinition);
		type.Methods.Add(readAssetDictionaryDefinition);
		type.Methods.Add(readAssetDictionaryAlignDefinition);
		foreach ((string _, IMethodDescriptor method) in methodDictionary.OrderBy(pair => pair.Key))
		{
			if (method is MethodDefinition methodDefinition && methodDefinition.DeclaringType is null)
			{
				type.Methods.Add(methodDefinition);
			}
		}
		Console.WriteLine($"\t{type.Methods.Count} {ReadMethod} helper methods");
	}

	private static void FillEditorWriteMethod(this TypeDefinition type, UniversalClass klass, UnityVersion version)
	{
		type.FillMethod(ReadEditor, klass.EditorRootNode, version);
	}

	private static void FillReleaseWriteMethod(this TypeDefinition type, UniversalClass klass, UnityVersion version)
	{
		type.FillMethod(ReadRelease, klass.ReleaseRootNode, version);
	}

	private static void FillMethod(this TypeDefinition type, string methodName, UniversalNode? rootNode, UnityVersion version)
	{
		MethodDefinition method = type.Methods.First(m => m.Name == methodName);
		CilInstructionCollection instructions = method.GetInstructions();

		if (rootNode is not null)
		{
			foreach (UniversalNode unityNode in rootNode.SubNodes)
			{
				FieldDefinition field = type.GetFieldByName(unityNode.Name, true);
				IMethodDescriptor fieldReadMethod = GetOrMakeMethod(unityNode, field.Signature!.FieldType, version);
				if (field.Signature.FieldType.IsArrayOrPrimitive())
				{
					instructions.Add(CilOpCodes.Ldarg_0);//this
					instructions.Add(CilOpCodes.Ldarg_1);//reader
					instructions.AddCall(fieldReadMethod);
					instructions.Add(CilOpCodes.Stfld, field);
				}
				else
				{
					instructions.Add(CilOpCodes.Ldarg_0);//this
					instructions.Add(CilOpCodes.Ldfld, field);
					instructions.Add(CilOpCodes.Ldarg_1);//reader
					instructions.AddCall(fieldReadMethod);
				}
			}
		}
		instructions.Add(CilOpCodes.Ret);
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
			MethodDefinition typeReadMethod = typeDefinition.GetMethodByName(ReadMethod);
			method = node.AlignBytes ? readAssetAlignDefinition.MakeGenericInstanceMethod(type) : typeReadMethod;
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
					bool align = node.AlignBytes || arrayNode.AlignBytes;
					GenericInstanceTypeSignature genericSignature = (GenericInstanceTypeSignature)type;
					Debug.Assert(genericSignature.GenericType.Name == $"{nameof(AssetDictionary<int, int>)}`2");
					Debug.Assert(genericSignature.TypeArguments.Count == 2);
					GenericInstanceTypeSignature pairSignature = assetPairReference.MakeGenericInstanceType(genericSignature.TypeArguments[0], genericSignature.TypeArguments[1]);
					method = MakeDictionaryMethod(uniqueName, pairNode, pairSignature, version, align);
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

		methodDictionary.Add(uniqueName, method);
		return method;
	}

	private static MethodDefinition MakeGenericAssetAlignMethod()
	{
		string uniqueName = "AssetAlign";
		GenericParameterSignature elementType = new GenericParameterSignature(SharedState.Instance.Module, GenericParameterType.Method, 0);
		IMethodDefOrRef readMethod = SharedState.Instance.Importer.ImportMethod<UnityAssetBase>(m => m.Name == ReadMethod && m.Parameters[0].ParameterType is ByReferenceTypeSignature);
		MethodDefinition method = NewMethod(uniqueName, elementType);

		CilInstructionCollection instructions = method.GetInstructions();
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.Add(CilOpCodes.Callvirt, readMethod);
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.AddCall(alignStreamMethod);
		instructions.Add(CilOpCodes.Ret);

		GenericParameter genericParameter = new GenericParameter("T");
		genericParameter.Constraints.Add(new GenericParameterConstraint(SharedState.Instance.Importer.ImportType<UnityAssetBase>()));
		method.GenericParameters.Add(genericParameter);
		method.Signature!.GenericParameterCount = 1;
		method.Signature.IsGeneric = true;

		return method;
	}

	private static IMethodDescriptor MakeDictionaryMethod(string uniqueName, UniversalNode pairNode, GenericInstanceTypeSignature pairSignature, UnityVersion version, bool align)
	{
		TypeSignature keySignature = pairSignature.TypeArguments[0];
		TypeSignature valueSignature = pairSignature.TypeArguments[1];
		if (keySignature.IsTypeDefinition() && valueSignature.IsTypeDefinition())
		{
			return align
				? readAssetDictionaryAlignDefinition.MakeGenericInstanceMethod(keySignature, valueSignature)
				: readAssetDictionaryDefinition.MakeGenericInstanceMethod(keySignature, valueSignature);
		}
		else
		{
			IMethodDescriptor pairReadMethod = GetOrMakeMethod(pairNode, pairSignature, version);
			return MakeDictionaryMethod(uniqueName, pairReadMethod, keySignature, valueSignature, align);
		}
	}

	private static MethodDefinition MakeGenericDictionaryMethod(bool align)
	{
		string uniqueName = align ? "MapAlign_Asset_Asset" : "Map_Asset_Asset";
		GenericParameterSignature keyType = new GenericParameterSignature(SharedState.Instance.Module, GenericParameterType.Method, 0);
		GenericParameterSignature valueType = new GenericParameterSignature(SharedState.Instance.Module, GenericParameterType.Method, 1);
		IMethodDescriptor readMethod = readAssetPairDefinition.MakeGenericInstanceMethod(keyType, valueType);
		MethodDefinition method = MakeDictionaryMethod(uniqueName, readMethod, keyType, valueType, align);

		GenericParameter keyParameter = new GenericParameter("TKey", GenericParameterAttributes.DefaultConstructorConstraint);
		keyParameter.Constraints.Add(new GenericParameterConstraint(SharedState.Instance.Importer.ImportType<UnityAssetBase>()));
		method.GenericParameters.Add(keyParameter);
		GenericParameter valueParameter = new GenericParameter("TValue", GenericParameterAttributes.DefaultConstructorConstraint);
		valueParameter.Constraints.Add(new GenericParameterConstraint(SharedState.Instance.Importer.ImportType<UnityAssetBase>()));
		method.GenericParameters.Add(valueParameter);
		method.Signature!.GenericParameterCount = 2;
		method.Signature.IsGeneric = true;

		return method;
	}

	private static MethodDefinition MakeDictionaryMethod(string uniqueName, IMethodDescriptor pairReadMethod, TypeSignature keySignature, TypeSignature valueSignature, bool align)
	{
		GenericInstanceTypeSignature genericDictionaryType = assetDictionaryReference.MakeGenericInstanceType(keySignature, valueSignature);

		MethodDefinition clearMethodDefinition = SharedState.Instance.Importer.LookupMethod(typeof(AssetDictionary<,>), m => m.Name == nameof(AssetDictionary<int, int>.Clear));
		IMethodDefOrRef clearMethodReference = MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, genericDictionaryType, clearMethodDefinition);

		MethodDefinition method = NewMethod(uniqueName, genericDictionaryType);
		CilInstructionCollection instructions = method.GetInstructions();

		CilLocalVariable countLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32);
		CilLocalVariable iLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32);

		CilInstructionLabel loopConditionStartList = new();

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Call, clearMethodReference);

		//Read count
		instructions.Add(CilOpCodes.Ldarg_1);//reader
		instructions.AddCall(readInt32Method);
		instructions.Add(CilOpCodes.Stloc, countLocal);

		instructions.Add(CilOpCodes.Ldc_I4_0); //Load 0 as an int32
		instructions.Add(CilOpCodes.Stloc, iLocal); //Store in count

		//Create an empty, unconditional branch which will jump down to the loop condition.
		//This converts the do..while loop into a for loop.
		instructions.Add(CilOpCodes.Br, loopConditionStartList);

		//Now we just read pair, increment i, compare against count, and jump back to here if it's less
		ICilLabel jumpTargetList = instructions.Add(CilOpCodes.Nop).CreateLabel(); //Create a dummy instruction to jump back to

		MethodDefinition addNewMethodDefinition = SharedState.Instance.Importer.LookupMethod(typeof(AssetDictionary<,>), m => m.Name == nameof(AssetDictionary<int, int>.AddNew));
		IMethodDefOrRef addNewMethodReference = MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, genericDictionaryType, addNewMethodDefinition);

		//Add new and read pair
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.AddCall(addNewMethodReference);
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.AddCall(pairReadMethod);

		//Increment i
		instructions.Add(CilOpCodes.Ldloc, iLocal); //Load i local
		instructions.Add(CilOpCodes.Ldc_I4_1); //Load constant 1 as int32
		instructions.Add(CilOpCodes.Add); //Add 
		instructions.Add(CilOpCodes.Stloc, iLocal); //Store in i local

		//Jump to start of loop if i < count
		loopConditionStartList.Instruction = instructions.Add(CilOpCodes.Ldloc, iLocal); //Load i
		instructions.Add(CilOpCodes.Ldloc, countLocal); //Load count
		instructions.Add(CilOpCodes.Blt, jumpTargetList); //Jump back up if less than

		if (align)
		{
			instructions.Add(CilOpCodes.Ldarg_1);
			instructions.AddCall(alignStreamMethod);
		}
		instructions.Add(CilOpCodes.Ret);
		instructions.OptimizeMacros();
		return method;
	}

	private static IMethodDescriptor MakePairMethod(string uniqueName, UniversalNode keyTypeNode, TypeSignature keySignature, UniversalNode valueTypeNode, TypeSignature valueSignature, UnityVersion version, bool align)
	{
		if (keySignature.IsTypeDefinition() && valueSignature.IsTypeDefinition())
		{
			return align
				? readAssetPairAlignDefinition.MakeGenericInstanceMethod(keySignature, valueSignature)
				: readAssetPairDefinition.MakeGenericInstanceMethod(keySignature, valueSignature);
		}
		else
		{
			IMethodDescriptor keyReadMethod = GetOrMakeMethod(keyTypeNode, keySignature, version);
			IMethodDescriptor valueReadMethod = GetOrMakeMethod(valueTypeNode, valueSignature, version);
			return MakePairMethod(uniqueName, keyReadMethod, keySignature, valueReadMethod, valueSignature, align);
		}
	}

	private static MethodDefinition MakeGenericPairMethod(bool align)
	{
		string uniqueName = align ? "PairAlign_Asset_Asset" : "Pair_Asset_Asset";
		GenericParameterSignature keyType = new GenericParameterSignature(SharedState.Instance.Module, GenericParameterType.Method, 0);
		GenericParameterSignature valueType = new GenericParameterSignature(SharedState.Instance.Module, GenericParameterType.Method, 1);
		IMethodDefOrRef readMethod = SharedState.Instance.Importer.ImportMethod<UnityAssetBase>(m => m.Name == ReadMethod && m.Parameters[0].ParameterType is ByReferenceTypeSignature);
		MethodDefinition method = MakePairMethod(uniqueName, readMethod, keyType, readMethod, valueType, align);

		GenericParameter keyParameter = new GenericParameter("TKey", GenericParameterAttributes.DefaultConstructorConstraint);
		keyParameter.Constraints.Add(new GenericParameterConstraint(SharedState.Instance.Importer.ImportType<UnityAssetBase>()));
		method.GenericParameters.Add(keyParameter);
		GenericParameter valueParameter = new GenericParameter("TValue", GenericParameterAttributes.DefaultConstructorConstraint);
		valueParameter.Constraints.Add(new GenericParameterConstraint(SharedState.Instance.Importer.ImportType<UnityAssetBase>()));
		method.GenericParameters.Add(valueParameter);
		method.Signature!.GenericParameterCount = 2;
		method.Signature.IsGeneric = true;

		return method;
	}

	private static MethodDefinition MakePairMethod(string uniqueName, IMethodDescriptor keyReadMethod, TypeSignature keySignature, IMethodDescriptor valueReadMethod, TypeSignature valueSignature, bool align)
	{
		GenericInstanceTypeSignature genericPairType = assetPairReference.MakeGenericInstanceType(keySignature, valueSignature);

		MethodDefinition method = NewMethod(uniqueName, genericPairType);
		CilInstructionCollection instructions = method.GetInstructions();

		if (keySignature.IsArrayOrPrimitive())
		{
			IMethodDefOrRef setKeyMethod = MethodUtils.MakeMethodOnGenericType(
				SharedState.Instance.Importer,
				genericPairType,
				assetPairDefinition.Methods.Single(m => m.Name == $"set_{nameof(AssetPair<int, int>.Key)}"));

			instructions.Add(CilOpCodes.Ldarg_0);//pair
			instructions.Add(CilOpCodes.Ldarg_1);//reader
			instructions.AddCall(keyReadMethod);
			instructions.AddCall(setKeyMethod);
		}
		else
		{
			IMethodDefOrRef getKeyMethod = MethodUtils.MakeMethodOnGenericType(
				SharedState.Instance.Importer,
				genericPairType,
				assetPairDefinition.Methods.Single(m => m.Name == $"get_{nameof(AssetPair<int, int>.Key)}"));

			instructions.Add(CilOpCodes.Ldarg_0);//pair
			instructions.AddCall(getKeyMethod);
			instructions.Add(CilOpCodes.Ldarg_1);//reader
			instructions.AddCall(keyReadMethod);
		}

		if (valueSignature.IsArrayOrPrimitive())
		{
			IMethodDefOrRef setValueMethod = MethodUtils.MakeMethodOnGenericType(
				SharedState.Instance.Importer,
				genericPairType,
				assetPairDefinition.Methods.Single(m => m.Name == $"set_{nameof(AssetPair<int, int>.Value)}"));

			instructions.Add(CilOpCodes.Ldarg_0);//pair
			instructions.Add(CilOpCodes.Ldarg_1);//reader
			instructions.AddCall(valueReadMethod);
			instructions.AddCall(setValueMethod);
		}
		else
		{
			IMethodDefOrRef getValueMethod = MethodUtils.MakeMethodOnGenericType(
				SharedState.Instance.Importer,
				genericPairType,
				assetPairDefinition.Methods.Single(m => m.Name == $"get_{nameof(AssetPair<int, int>.Value)}"));

			instructions.Add(CilOpCodes.Ldarg_0);//pair
			instructions.AddCall(getValueMethod);
			instructions.Add(CilOpCodes.Ldarg_1);//reader
			instructions.AddCall(valueReadMethod);
		}

		if (align)
		{
			instructions.Add(CilOpCodes.Ldarg_1);//reader
			instructions.AddCall(alignStreamMethod);
		}
		instructions.Add(CilOpCodes.Ret);
		return method;
	}

	private static IMethodDescriptor MakeTypelessDataMethod(string uniqueName, bool align)
	{
		CorLibTypeSignature elementType = SharedState.Instance.Importer.UInt8;
		SzArrayTypeSignature arrayType = elementType.MakeSzArrayType();
		MethodDefinition method = NewMethod(uniqueName, arrayType);
		CilInstructionCollection instructions = method.GetInstructions();

		CilLocalVariable countLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32);
		CilLocalVariable arrayLocal = instructions.AddLocalVariable(arrayType);

		//Read count
		instructions.Add(CilOpCodes.Ldarg_0);//reader
		instructions.AddCall(readInt32Method);
		instructions.Add(CilOpCodes.Stloc, countLocal);

		instructions.Add(CilOpCodes.Ldarg_0);//reader
		instructions.Add(CilOpCodes.Ldloc, countLocal);
		instructions.AddCall(readBytesMethod);
		instructions.Add(CilOpCodes.Stloc, arrayLocal);

		if (align)
		{
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.AddCall(alignStreamMethod);
		}
		instructions.Add(CilOpCodes.Ldloc, arrayLocal);
		instructions.Add(CilOpCodes.Ret);
		instructions.OptimizeMacros();
		return method;
	}

	private static IMethodDescriptor MakeListMethod(string uniqueName, UniversalNode elementTypeNode, TypeSignature elementType, UnityVersion version, bool align)
	{
		if (elementType.IsTypeDefinition())
		{
			return align
				? readAssetListAlignDefinition.MakeGenericInstanceMethod(elementType)
				: readAssetListDefinition.MakeGenericInstanceMethod(elementType);
		}
		else
		{
			IMethodDescriptor elementReadMethod = GetOrMakeMethod(elementTypeNode, elementType, version);
			return MakeListMethod(uniqueName, elementType, elementReadMethod, align);
		}
	}

	private static MethodDefinition MakeGenericListMethod(bool align)
	{
		string uniqueName = align ? "ArrayAlign_Asset" : "Array_Asset";
		GenericParameterSignature elementType = new GenericParameterSignature(SharedState.Instance.Module, GenericParameterType.Method, 0);
		IMethodDefOrRef readMethod = SharedState.Instance.Importer.ImportMethod<UnityAssetBase>(m => m.Name == ReadMethod && m.Parameters[0].ParameterType is ByReferenceTypeSignature);
		MethodDefinition method = MakeListMethod(uniqueName, elementType, readMethod, align);

		GenericParameter genericParameter = new GenericParameter("T", GenericParameterAttributes.DefaultConstructorConstraint);
		genericParameter.Constraints.Add(new GenericParameterConstraint(SharedState.Instance.Importer.ImportType<UnityAssetBase>()));
		method.GenericParameters.Add(genericParameter);
		method.Signature!.GenericParameterCount = 1;
		method.Signature.IsGeneric = true;

		return method;
	}

	private static MethodDefinition MakeListMethod(string uniqueName, TypeSignature elementType, IMethodDescriptor elementReadMethod, bool align)
	{
		GenericInstanceTypeSignature genericListType = assetListReference.MakeGenericInstanceType(elementType);

		MethodDefinition clearMethodDefinition = SharedState.Instance.Importer.LookupMethod(typeof(AssetList<>), m => m.Name == nameof(AssetList<int>.Clear));
		IMethodDefOrRef clearMethodReference = MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, genericListType, clearMethodDefinition);

		MethodDefinition method = NewMethod(uniqueName, genericListType);
		CilInstructionCollection instructions = method.GetInstructions();

		CilLocalVariable countLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32);
		CilLocalVariable iLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32);

		CilInstructionLabel loopConditionStartList = new();

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Call, clearMethodReference);

		//Read count
		instructions.Add(CilOpCodes.Ldarg_1);//reader
		instructions.AddCall(readInt32Method);
		instructions.Add(CilOpCodes.Stloc, countLocal);

		instructions.Add(CilOpCodes.Ldc_I4_0); //Load 0 as an int32
		instructions.Add(CilOpCodes.Stloc, iLocal); //Store in count

		//Create an empty, unconditional branch which will jump down to the loop condition.
		//This converts the do..while loop into a for loop.
		instructions.Add(CilOpCodes.Br, loopConditionStartList);

		//Now we just read pair, increment i, compare against count, and jump back to here if it's less
		ICilLabel jumpTargetList = instructions.Add(CilOpCodes.Nop).CreateLabel(); //Create a dummy instruction to jump back to

		//Read and add to list
		if (elementType.IsArrayOrPrimitive())
		{
			MethodDefinition addMethodDefinition = SharedState.Instance.Importer.LookupMethod(typeof(AssetList<>), m => m.Name == nameof(AssetList<int>.Add));
			IMethodDefOrRef addMethodReference = MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, genericListType, addMethodDefinition);
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldarg_1);
			instructions.AddCall(elementReadMethod);
			instructions.AddCall(addMethodReference);
		}
		else
		{
			MethodDefinition addNewMethodDefinition = SharedState.Instance.Importer.LookupMethod(typeof(AssetList<>), m => m.Name == nameof(AssetList<int>.AddNew));
			IMethodDefOrRef addNewMethodReference = MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, genericListType, addNewMethodDefinition);
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.AddCall(addNewMethodReference);
			instructions.Add(CilOpCodes.Ldarg_1);
			instructions.AddCall(elementReadMethod);
		}

		//Increment i
		instructions.Add(CilOpCodes.Ldloc, iLocal); //Load i local
		instructions.Add(CilOpCodes.Ldc_I4_1); //Load constant 1 as int32
		instructions.Add(CilOpCodes.Add); //Add 
		instructions.Add(CilOpCodes.Stloc, iLocal); //Store in i local

		//Jump to start of loop if i < count
		loopConditionStartList.Instruction = instructions.Add(CilOpCodes.Ldloc, iLocal); //Load i
		instructions.Add(CilOpCodes.Ldloc, countLocal); //Load count
		instructions.Add(CilOpCodes.Blt, jumpTargetList); //Jump back up if less than

		if (align)
		{
			instructions.Add(CilOpCodes.Ldarg_1);
			instructions.AddCall(alignStreamMethod);
		}
		instructions.Add(CilOpCodes.Ret);
		instructions.OptimizeMacros();

		return method;
	}

	private static IMethodDescriptor MakeArrayMethod(string uniqueName, UniversalNode elementTypeNode, TypeSignature elementType, UnityVersion version, bool align)
	{
		if (elementType is CorLibTypeSignature corLibTypeSignature && corLibTypeSignature.ElementType == ElementType.U1)
		{
			return MakeTypelessDataMethod(uniqueName, align);
		}

		bool throwForNonByteArrays = true;
		if (throwForNonByteArrays)
		{
			throw new NotSupportedException();
		}

		IMethodDescriptor elementReadMethod = GetOrMakeMethod(elementTypeNode, elementType, version);

		SzArrayTypeSignature arrayType = elementType.MakeSzArrayType();
		GenericInstanceTypeSignature listType = SharedState.Instance.Importer.ImportType(typeof(List<>)).MakeGenericInstanceType(elementType);
		MethodDefinition method = NewMethod(uniqueName, arrayType);
		CilInstructionCollection instructions = method.GetInstructions();

		CilLocalVariable countLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32);
		CilLocalVariable iLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32);
		CilLocalVariable arrayLocal = instructions.AddLocalVariable(arrayType);
		CilLocalVariable listLocal = instructions.AddLocalVariable(listType);

		//Read count
		instructions.Add(CilOpCodes.Ldarg_0);//reader
		instructions.AddCall(readInt32Method);
		instructions.Add(CilOpCodes.Stloc, countLocal);

		CilInstructionLabel readAsListInstruction = new();
		CilInstructionLabel loopConditionStartArray = new();
		CilInstructionLabel loopConditionStartList = new();
		CilInstructionLabel returnInstruction = new();

		//Check size of count
		instructions.Add(CilOpCodes.Ldloc, countLocal);
		instructions.Add(CilOpCodes.Ldc_I4, MaxArraySize);
		instructions.Add(CilOpCodes.Bgt, readAsListInstruction);

		//Read into array
		instructions.Add(CilOpCodes.Ldloc, countLocal);
		instructions.Add(CilOpCodes.Newarr, elementType.ToTypeDefOrRef());
		instructions.Add(CilOpCodes.Stloc, arrayLocal);

		instructions.Add(CilOpCodes.Ldc_I4_0); //Load 0 as an int32
		instructions.Add(CilOpCodes.Stloc, iLocal); //Store in count

		//Create an empty, unconditional branch which will jump down to the loop condition.
		//This converts the do..while loop into a for loop.
		instructions.Add(CilOpCodes.Br, loopConditionStartArray);

		//Now we just read pair, increment i, compare against count, and jump back to here if it's less
		ICilLabel jumpTargetArray = instructions.Add(CilOpCodes.Nop).CreateLabel(); //Create a dummy instruction to jump back to

		//Read and add to array
		instructions.Add(CilOpCodes.Ldloc, arrayLocal);
		instructions.Add(CilOpCodes.Ldloc, iLocal);
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.AddCall(elementReadMethod);
		instructions.AddStoreElement(elementType);

		//Increment i
		instructions.Add(CilOpCodes.Ldloc, iLocal); //Load i local
		instructions.Add(CilOpCodes.Ldc_I4_1); //Load constant 1 as int32
		instructions.Add(CilOpCodes.Add); //Add 
		instructions.Add(CilOpCodes.Stloc, iLocal); //Store in i local

		//Jump to start of loop if i < count
		loopConditionStartArray.Instruction = instructions.Add(CilOpCodes.Ldloc, iLocal); //Load i
		instructions.Add(CilOpCodes.Ldloc, countLocal); //Load count
		instructions.Add(CilOpCodes.Blt, jumpTargetArray); //Jump back up if less than

		instructions.Add(CilOpCodes.Br, returnInstruction);//Jump to return statement

		//Read into list (because we don't trust large counts)

		MethodDefinition listConstructorDefinition = SharedState.Instance.Importer.LookupMethod(typeof(List<>), m =>
		{
			return m.IsConstructor
				&& m.Parameters.Count == 1
				&& m.Parameters[0].ParameterType is CorLibTypeSignature corLibTypeSignature
				&& corLibTypeSignature.ElementType == ElementType.I4;
		});
		IMethodDefOrRef listConstructorReference = MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, listType, listConstructorDefinition);
		MethodDefinition addMethodDefinition = SharedState.Instance.Importer.LookupMethod(typeof(List<>), m => m.Name == nameof(List<int>.Add));
		IMethodDefOrRef addMethodReference = MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, listType, addMethodDefinition);
		MethodDefinition toArrayMethodDefinition = SharedState.Instance.Importer.LookupMethod(typeof(List<>), m => m.Name == nameof(List<int>.ToArray));
		IMethodDefOrRef toArrayMethodReference = MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, listType, toArrayMethodDefinition);

		readAsListInstruction.Instruction = instructions.Add(CilOpCodes.Ldc_I4, MaxArraySize);
		instructions.Add(CilOpCodes.Newobj, listConstructorReference);
		instructions.Add(CilOpCodes.Stloc, listLocal);

		instructions.Add(CilOpCodes.Ldc_I4_0); //Load 0 as an int32
		instructions.Add(CilOpCodes.Stloc, iLocal); //Store in count

		//Create an empty, unconditional branch which will jump down to the loop condition.
		//This converts the do..while loop into a for loop.
		instructions.Add(CilOpCodes.Br, loopConditionStartList);

		//Now we just read pair, increment i, compare against count, and jump back to here if it's less
		ICilLabel jumpTargetList = instructions.Add(CilOpCodes.Nop).CreateLabel(); //Create a dummy instruction to jump back to

		//Read byte and add to list
		instructions.Add(CilOpCodes.Ldloc, listLocal);
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.AddCall(elementReadMethod);
		instructions.AddCall(addMethodReference);

		//Increment i
		instructions.Add(CilOpCodes.Ldloc, iLocal); //Load i local
		instructions.Add(CilOpCodes.Ldc_I4_1); //Load constant 1 as int32
		instructions.Add(CilOpCodes.Add); //Add 
		instructions.Add(CilOpCodes.Stloc, iLocal); //Store in i local

		//Jump to start of loop if i < count
		loopConditionStartList.Instruction = instructions.Add(CilOpCodes.Ldloc, iLocal); //Load i
		instructions.Add(CilOpCodes.Ldloc, countLocal); //Load count
		instructions.Add(CilOpCodes.Blt, jumpTargetList); //Jump back up if less than

		instructions.Add(CilOpCodes.Ldloc, listLocal);
		instructions.AddCall(toArrayMethodReference);
		instructions.Add(CilOpCodes.Stloc, arrayLocal);

		returnInstruction.Instruction = instructions.Add(CilOpCodes.Nop);
		if (align)
		{
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.AddCall(alignStreamMethod);
		}
		instructions.Add(CilOpCodes.Ldloc, arrayLocal);
		instructions.Add(CilOpCodes.Ret);
		instructions.OptimizeMacros();
		return method;
	}

	private static IMethodDescriptor MakePrimitiveMethod(string uniqueName, UniversalNode node, bool align)
	{
		IMethodDescriptor primitiveMethod = GetPrimitiveMethod(node);
		if (align)
		{
			MethodDefinition method = NewMethod(uniqueName, primitiveMethod.Signature!.ReturnType);
			CilInstructionCollection instructions = method.GetInstructions();
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.AddCall(primitiveMethod);
			if (align)
			{
				instructions.Add(CilOpCodes.Ldarg_0);
				instructions.AddCall(alignStreamMethod);
			}
			instructions.Add(CilOpCodes.Ret);
			return method;
		}
		else
		{
			return primitiveMethod;
		}
	}

	/// <summary>
	/// Array and primitive read methods have the Func&lt;AssetReader, T&gt; signature.<br/>
	/// Others have the Action&lt;T, AssetReader&gt; signature.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	private static bool IsArrayOrPrimitive(this TypeSignature type)
	{
		return type is SzArrayTypeSignature or CorLibTypeSignature or TypeDefOrRefSignature { Namespace: "AssetRipper.Primitives", Name: nameof(Utf8String) };
	}

	private static bool IsTypeDefinition(this TypeSignature type)
	{
		return type is TypeDefOrRefSignature defOrRefSignature && defOrRefSignature.Type is TypeDefinition;
	}

	private static IMethodDescriptor GetPrimitiveMethod(UniversalNode node)
	{
		return node.NodeType switch
		{
			NodeType.Boolean => primitiveReadMethods[ElementType.Boolean],
			NodeType.Character => primitiveReadMethods[ElementType.Char],
			NodeType.Int8 => primitiveReadMethods[ElementType.I1],
			NodeType.UInt8 => primitiveReadMethods[ElementType.U1],
			NodeType.Int16 => primitiveReadMethods[ElementType.I2],
			NodeType.UInt16 => primitiveReadMethods[ElementType.U2],
			NodeType.Int32 => primitiveReadMethods[ElementType.I4],
			NodeType.UInt32 => primitiveReadMethods[ElementType.U4],
			NodeType.Int64 => primitiveReadMethods[ElementType.I8],
			NodeType.UInt64 => primitiveReadMethods[ElementType.U8],
			NodeType.Single => primitiveReadMethods[ElementType.R4],
			NodeType.Double => primitiveReadMethods[ElementType.R8],
			NodeType.String => primitiveReadMethods[ElementType.String],
			_ => throw new NotSupportedException(node.TypeName),
		};
	}

	private static MethodDefinition NewMethod(string uniqueName, TypeSignature parameter)
	{
		if (parameter.IsArrayOrPrimitive())
		{
			MethodSignature methodSignature = MethodSignature.CreateStatic(parameter);
			MethodDefinition method = new MethodDefinition($"{ReadMethod}_{uniqueName}", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, methodSignature);
			method.CilMethodBody = new CilMethodBody();
			method.AddParameter(endianSpanReaderReference, "reader");
			method.AddExtensionAttribute(SharedState.Instance.Importer);
			return method;
		}
		else
		{
			MethodSignature methodSignature = MethodSignature.CreateStatic(SharedState.Instance.Importer.Void);
			MethodDefinition method = new MethodDefinition($"{ReadMethod}_{uniqueName}", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, methodSignature);
			method.CilMethodBody = new CilMethodBody();
			method.AddParameter(parameter, "value");
			method.AddParameter(endianSpanReaderReference, "reader");
			method.AddExtensionAttribute(SharedState.Instance.Importer);
			return method;
		}
	}

	private static CilInstruction AddCall(this CilInstructionCollection instructions, IMethodDescriptor method)
	{
		return method is MethodDefinition definition && definition.IsStatic
			? instructions.Add(CilOpCodes.Call, method)
			: instructions.Add(CilOpCodes.Callvirt, method);
	}

	private static IMethodDefOrRef GetDefaultConstructor(this TypeSignature type)
	{
		return type switch
		{
			TypeDefOrRefSignature typeDefOrRefSignature => typeDefOrRefSignature.Type is TypeDefinition typeDefinition
										? typeDefinition.GetDefaultConstructor()
										: throw new InvalidOperationException(),
			GenericInstanceTypeSignature genericInstanceTypeSignature => MethodUtils.MakeConstructorOnGenericType(SharedState.Instance.Importer, genericInstanceTypeSignature, 0),
			_ => throw new NotSupportedException(),
		};
	}
}
