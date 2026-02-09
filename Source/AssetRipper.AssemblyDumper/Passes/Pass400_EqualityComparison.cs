using AssetRipper.AssemblyDumper.AST;
using AssetRipper.AssemblyDumper.InjectedTypes;
using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.AssemblyDumper.Types;
using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using System.Diagnostics;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass400_EqualityComparison
{
	/// <summary>
	/// Methods are guaranteed to be static and have signature: bool(T x, T y)
	/// </summary>
	private static readonly Dictionary<TypeSignature, IMethodDescriptor> equalsMethods = new(SignatureComparer.Default);
	/// <summary>
	/// Methods are guaranteed to be static and have signature: bool?(T x, T y, AssetEqualityComparer)
	/// </summary>
	private static readonly Dictionary<TypeSignature, IMethodDescriptor> addToEqualityComparerMethods = new(SignatureComparer.Default);

	public static void DoPass()
	{
		TypeDefinition equalityComparisonHelper = SharedState.Instance.InjectHelperType(typeof(EqualityComparisonHelper));

		ITypeDefOrRef hashCodeType = SharedState.Instance.Importer.ImportType<HashCode>();
		IMethodDefOrRef addMethod = SharedState.Instance.Importer.ImportMethod<HashCode>(
			m => m.Name == nameof(HashCode.Add) && m.Parameters.Count == 1 && m.Signature!.GenericParameterCount == 1);
		IMethodDefOrRef toHashCodeMethod = SharedState.Instance.Importer.ImportMethod<HashCode>(m => m.Name == nameof(HashCode.ToHashCode));

		ITypeDefOrRef equatableInterface = SharedState.Instance.Importer.ImportType(typeof(IEquatable<>));

		ITypeDefOrRef iunityAssetBase = SharedState.Instance.Importer.ImportType<IUnityAssetBase>();
		ITypeDefOrRef assetEqualityComparer = SharedState.Instance.Importer.ImportType<AssetEqualityComparer>();

		TypeSignature trileanTypeSignature = equalityComparisonHelper.GetMethodByName(nameof(EqualityComparisonHelper.ToTrilean)).Signature!.ReturnType;

		HashSet<SubclassGroup> subclassesWithPPtrs = new();
		foreach (SubclassGroup group in SharedState.Instance.SubclassGroups.Values.Where(g => !g.IsString && !g.IsPPtr))
		{
			foreach (GeneratedClassInstance instance in group.Instances)
			{
				TypeNode root = new(instance);
				if (root.AnyPPtrs)
				{
					//For now, we do nothing.
					subclassesWithPPtrs.Add(group);
					TypeSignature typeSignature = instance.Type.ToTypeSignature();
					addToEqualityComparerMethods.Add(typeSignature, equalityComparisonHelper
						.GetMethodByName(nameof(EqualityComparisonHelper.AssetEquals))
						.MakeGenericInstanceMethod(typeSignature));
				}
				else
				{
					//Generate IEquatable<T>, ==, !=, Equals, GetHashCode

					instance.Type.AddInterfaceImplementation(equatableInterface.MakeGenericInstanceType(instance.Type.ToTypeSignature()).ToTypeDefOrRef());

					MethodDefinition equalsMethod = GenerateEqualsMethod(instance, root, equalityComparisonHelper);

					MethodDefinition objectEqualsMethod = OverrideObjectEquals(instance, root, equalsMethod);

					OverrideGetHashCode(instance, root, hashCodeType, addMethod, toHashCodeMethod);

					AddEqualityOperators(instance, root, equalsMethod);

					OverrideAddToEqualityComparer(instance, objectEqualsMethod, iunityAssetBase, assetEqualityComparer, equalityComparisonHelper);
				}
			}
		}

		//PPtr classes
		{
			MethodDefinition pptrMethod = equalityComparisonHelper.GetMethodByName(nameof(EqualityComparisonHelper.MaybeAddDependentComparison));
			foreach (SubclassGroup group in SharedState.Instance.SubclassGroups.Values.Where(g => g.IsPPtr))
			{
				foreach (GeneratedClassInstance instance in group.Instances)
				{
					TypeDefinition type = instance.Type;
					MethodDefinition method = type.AddMethod(
						nameof(UnityAssetBase.AddToEqualityComparer),
						Pass063_CreateEmptyMethods.OverrideMethodAttributes,
						trileanTypeSignature);
					method.AddParameter(iunityAssetBase.ToTypeSignature(), "other");
					method.AddParameter(assetEqualityComparer.ToTypeSignature(), "comparer");

					CilInstructionCollection instructions = method.GetInstructions();

					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Ldarg_1);
					instructions.Add(CilOpCodes.Castclass, type);
					instructions.Add(CilOpCodes.Ldarg_2);
					instructions.Add(CilOpCodes.Call, pptrMethod);
					instructions.Add(CilOpCodes.Ret);

					//We're generating these methods for AssetList<T>, but the following method avoids a cast.
					addToEqualityComparerMethods.Add(type.ToTypeSignature(), pptrMethod);
				}
			}
		}

		//Equals methods have been cached.
		foreach (SubclassGroup group in subclassesWithPPtrs)
		{
			foreach (GeneratedClassInstance instance in group.Instances)
			{
				TypeNode root = new(instance);
				if (!root.AnyPPtrs)
				{
					// Equals methods have already been generated.
					continue;
				}

				GenerateAddToEqualityComparer(equalityComparisonHelper, iunityAssetBase, assetEqualityComparer, trileanTypeSignature, instance, root);
			}
		}

		foreach (ClassGroup group in SharedState.Instance.ClassGroups.Values)
		{
			foreach (GeneratedClassInstance instance in group.Instances)
			{
				if (instance.Type.IsAbstract)
				{
					//We don't need to generate equality methods for abstract classes.
					continue;
				}

				TypeNode root = new(instance);

				GenerateAddToEqualityComparer(equalityComparisonHelper, iunityAssetBase, assetEqualityComparer, trileanTypeSignature, instance, root);
			}
		}

		equalsMethods.Clear();
		addToEqualityComparerMethods.Clear();
	}

	private static void GenerateAddToEqualityComparer(TypeDefinition equalityComparisonHelper, ITypeDefOrRef iunityAssetBase, ITypeDefOrRef assetEqualityComparer, TypeSignature trileanTypeSignature, GeneratedClassInstance instance, TypeNode root)
	{
		TypeDefinition type = instance.Type;
		MethodDefinition method = type.AddMethod(
			nameof(UnityAssetBase.AddToEqualityComparer),
			Pass063_CreateEmptyMethods.OverrideMethodAttributes,
			trileanTypeSignature);
		method.AddParameter(iunityAssetBase.ToTypeSignature(), "other");
		method.AddParameter(assetEqualityComparer.ToTypeSignature(), "comparer");

		MethodDefinition getTrueMethod = equalityComparisonHelper.GetMethodByName(nameof(EqualityComparisonHelper.GetTrue));
		MethodDefinition getFalseMethod = equalityComparisonHelper.GetMethodByName(nameof(EqualityComparisonHelper.GetFalse));
		MethodDefinition getNullMethod = equalityComparisonHelper.GetMethodByName(nameof(EqualityComparisonHelper.GetNull));

		MethodDefinition isFalseMethod = equalityComparisonHelper.GetMethodByName(nameof(EqualityComparisonHelper.IsFalse));
		MethodDefinition isNullMethod = equalityComparisonHelper.GetMethodByName(nameof(EqualityComparisonHelper.IsNull));

		CilInstructionCollection instructions = method.GetInstructions();

		CilLocalVariable otherLocal = instructions.AddLocalVariable(type.ToTypeSignature());
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.Add(CilOpCodes.Castclass, type);
		instructions.Add(CilOpCodes.Stloc, otherLocal);

		CilLocalVariable resultLocal = instructions.AddLocalVariable(trileanTypeSignature);
		instructions.Add(CilOpCodes.Call, getTrueMethod);
		instructions.Add(CilOpCodes.Stloc, resultLocal);

		foreach (FieldNode field in root.Children)
		{
			if (!field.AnyPPtrs)
			{
				CilInstructionLabel nextFieldLabel = new();
				instructions.Add(CilOpCodes.Ldarg_0);
				instructions.Add(CilOpCodes.Ldfld, field.Field);
				instructions.Add(CilOpCodes.Ldloc, otherLocal);
				instructions.Add(CilOpCodes.Ldfld, field.Field);
				instructions.Add(CilOpCodes.Call, GetEqualsMethod(equalityComparisonHelper, field));
				instructions.Add(CilOpCodes.Brtrue, nextFieldLabel);
				instructions.Add(CilOpCodes.Call, getFalseMethod);
				instructions.Add(CilOpCodes.Ret);
				nextFieldLabel.Instruction = instructions.Add(CilOpCodes.Nop);
			}
			else
			{
				CilLocalVariable fieldResultLocal = instructions.AddLocalVariable(trileanTypeSignature);

				instructions.Add(CilOpCodes.Ldarg_0);
				instructions.Add(CilOpCodes.Ldfld, field.Field);
				instructions.Add(CilOpCodes.Ldloc, otherLocal);
				instructions.Add(CilOpCodes.Ldfld, field.Field);
				instructions.Add(CilOpCodes.Ldarg_2);
				instructions.Add(CilOpCodes.Call, GetAddToEqualityComparerMethod(equalityComparisonHelper, field));
				instructions.Add(CilOpCodes.Stloc, fieldResultLocal);

				CilInstructionLabel notFalseLabel = new();
				instructions.Add(CilOpCodes.Ldloc, fieldResultLocal);
				instructions.Add(CilOpCodes.Call, isFalseMethod);
				instructions.Add(CilOpCodes.Brfalse, notFalseLabel);
				instructions.Add(CilOpCodes.Call, getFalseMethod);
				instructions.Add(CilOpCodes.Ret);
				notFalseLabel.Instruction = instructions.Add(CilOpCodes.Nop);

				CilInstructionLabel nextFieldLabel = new();
				instructions.Add(CilOpCodes.Ldloc, fieldResultLocal);
				instructions.Add(CilOpCodes.Call, isNullMethod);
				instructions.Add(CilOpCodes.Brfalse, nextFieldLabel);
				instructions.Add(CilOpCodes.Call, getNullMethod);
				instructions.Add(CilOpCodes.Stloc, resultLocal);
				nextFieldLabel.Instruction = instructions.Add(CilOpCodes.Nop);
			}
		}

		if (root.ClassInstance.Group.ID is 114 or 2089858483) // MonoBehaviour or ScriptedImporter
		{
			MethodDefinition structureEqualsMethod = equalityComparisonHelper.GetMethodByName(nameof(EqualityComparisonHelper.MonoBehaviourStructureEquals));

			// The structure field doesn't exist yet, so we work around that by creating a MemberReference.
			MemberReference structureField = new MemberReference(type, "m_Structure", new FieldSignature(structureEqualsMethod.Signature!.ParameterTypes[0]));

			CilLocalVariable fieldResultLocal = instructions.AddLocalVariable(trileanTypeSignature);

			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldfld, structureField);
			instructions.Add(CilOpCodes.Ldloc, otherLocal);
			instructions.Add(CilOpCodes.Ldfld, structureField);
			instructions.Add(CilOpCodes.Ldarg_2);
			instructions.Add(CilOpCodes.Call, structureEqualsMethod);
			instructions.Add(CilOpCodes.Stloc, fieldResultLocal);

			CilInstructionLabel notFalseLabel = new();
			instructions.Add(CilOpCodes.Ldloc, fieldResultLocal);
			instructions.Add(CilOpCodes.Call, isFalseMethod);
			instructions.Add(CilOpCodes.Brfalse, notFalseLabel);
			instructions.Add(CilOpCodes.Call, getFalseMethod);
			instructions.Add(CilOpCodes.Ret);
			notFalseLabel.Instruction = instructions.Add(CilOpCodes.Nop);

			CilInstructionLabel nextFieldLabel = new();
			instructions.Add(CilOpCodes.Ldloc, fieldResultLocal);
			instructions.Add(CilOpCodes.Call, isNullMethod);
			instructions.Add(CilOpCodes.Brfalse, nextFieldLabel);
			instructions.Add(CilOpCodes.Call, getNullMethod);
			instructions.Add(CilOpCodes.Stloc, resultLocal);
			nextFieldLabel.Instruction = instructions.Add(CilOpCodes.Nop);
		}

		instructions.Add(CilOpCodes.Ldloc, resultLocal);
		instructions.Add(CilOpCodes.Ret);
	}

	private static void OverrideAddToEqualityComparer(GeneratedClassInstance instance, MethodDefinition objectEqualsMethod, ITypeDefOrRef iunityAssetBase, ITypeDefOrRef assetEqualityComparer, TypeDefinition equalityComparisonHelper)
	{
		TypeDefinition type = instance.Type;
		MethodDefinition method = type.AddMethod(
			nameof(UnityAssetBase.AddToEqualityComparer),
			Pass063_CreateEmptyMethods.OverrideMethodAttributes,
			equalityComparisonHelper.GetMethodByName(nameof(EqualityComparisonHelper.GetTrue)).Signature!.ReturnType);
		method.AddParameter(iunityAssetBase.ToTypeSignature(), "other");
		method.AddParameter(assetEqualityComparer.ToTypeSignature(), "comparer");
		CilInstructionCollection instructions = method.GetInstructions();

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.Add(CilOpCodes.Callvirt, objectEqualsMethod);
		instructions.Add(CilOpCodes.Call, equalityComparisonHelper.GetMethodByName(nameof(EqualityComparisonHelper.ToTrilean)));
		instructions.Add(CilOpCodes.Ret);
	}

	private static MethodDefinition GenerateEqualsMethod(GeneratedClassInstance instance, TypeNode root, TypeDefinition equalityComparisonHelper)
	{
		TypeDefinition type = instance.Type;
		MethodDefinition method = type.AddMethod(
			nameof(object.Equals),
			InterfaceUtils.InterfaceMethodImplementation,
			SharedState.Instance.Importer.Boolean);
		method.AddParameter(type.ToTypeSignature(), "other");
		CilInstructionCollection instructions = method.GetInstructions();

		CilInstructionLabel falseLabel = new();

		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.Add(CilOpCodes.Brfalse, falseLabel);

		for (int i = 0; i < root.Children.Count; i++)
		{
			FieldNode field = root.Children[i];
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldfld, field.Field);
			instructions.Add(CilOpCodes.Ldarg_1);
			instructions.Add(CilOpCodes.Ldfld, field.Field);
			instructions.Add(CilOpCodes.Call, GetEqualsMethod(equalityComparisonHelper, field));

			if (i < root.Children.Count - 1)
			{
				instructions.Add(CilOpCodes.Brfalse, falseLabel);
			}
		}

		CilInstructionLabel returnLabel = new();
		instructions.Add(CilOpCodes.Br, returnLabel);

		falseLabel.Instruction = instructions.Add(CilOpCodes.Ldc_I4_0);

		returnLabel.Instruction = instructions.Add(CilOpCodes.Ret);

		instructions.OptimizeMacros();

		return method;
	}

	private static IMethodDescriptor GetEqualsMethod(TypeDefinition equalityComparisonHelper, FieldNode field)
	{
		if (!equalsMethods.TryGetValue(field.TypeSignature, out IMethodDescriptor? cachedMethod))
		{
			switch (field.Child)
			{
				case DictionaryNode dictionary:
					if (dictionary.Child.Key.Equatable && dictionary.Child.Value.Equatable)
					{
						cachedMethod = equalityComparisonHelper
							.GetMethodByName(nameof(EqualityComparisonHelper.EquatableDictionaryEquals))
							.MakeGenericInstanceMethod(dictionary.KeyTypeSignature, dictionary.ValueTypeSignature);
					}
					else if (dictionary.Child.Key.Equatable && dictionary.Child.Value.Child is ListNode listValue && listValue.Child.Equatable)
					{
						cachedMethod = equalityComparisonHelper
							.GetMethodByName(nameof(EqualityComparisonHelper.EquatableDictionaryListEquals))
							.MakeGenericInstanceMethod(dictionary.KeyTypeSignature, listValue.ElementTypeSignature);
					}
					else
					{
						throw new NotImplementedException();
					}

					break;
				case ListNode list:
					if (list.Child is ListNode childList)
					{
						if (!childList.Child.Equatable)
						{
							throw new NotImplementedException();
						}

						cachedMethod = equalityComparisonHelper
							.GetMethodByName(nameof(EqualityComparisonHelper.EquatableListListEquals))
							.MakeGenericInstanceMethod(childList.ElementTypeSignature);
					}
					else if (!list.Child.Equatable)
					{
						throw new NotImplementedException();
					}
					else
					{
						cachedMethod = equalityComparisonHelper
							.GetMethodByName(nameof(EqualityComparisonHelper.EquatableListEquals))
							.MakeGenericInstanceMethod(list.ElementTypeSignature);
					}
					break;
				case PairNode pair:
					if (!pair.Equatable)
					{
						throw new NotImplementedException();
					}
					goto default;
				case PrimitiveNode primitive:
					if (primitive.TypeSignature is SzArrayTypeSignature)
					{
						Debug.Assert(primitive.TypeSignature is SzArrayTypeSignature { BaseType: CorLibTypeSignature { ElementType: ElementType.U1 } });
						cachedMethod = equalityComparisonHelper.GetMethodByName(nameof(EqualityComparisonHelper.ByteArrayEquals));
					}
					else
					{
						goto default;
					}
					break;
				default:
					cachedMethod = equalityComparisonHelper
						.GetMethodByName(nameof(EqualityComparisonHelper.EquatableEquals))
						.MakeGenericInstanceMethod(field.TypeSignature);
					break;
			}

			equalsMethods.Add(field.TypeSignature, cachedMethod);
		}

		return cachedMethod;
	}

	private static IMethodDescriptor GetAddToEqualityComparerMethod(TypeDefinition equalityComparisonHelper, FieldNode field)
	{
		if (!addToEqualityComparerMethods.TryGetValue(field.TypeSignature, out IMethodDescriptor? cachedMethod))
		{
			switch (field.Child)
			{
				case TypeNode:
					cachedMethod = equalityComparisonHelper
						.GetMethodByName(nameof(EqualityComparisonHelper.AssetEquals))
						.MakeGenericInstanceMethod(field.TypeSignature);
					break;
				case DictionaryNode dictionary:
					if (dictionary.Child.Key.Child is TypeNode or PPtrNode or PrimitiveNode { TypeSignature: not SzArrayTypeSignature }
						&& dictionary.Child.Value.Child is TypeNode or PPtrNode or PrimitiveNode { TypeSignature: not SzArrayTypeSignature })
					{
						cachedMethod = equalityComparisonHelper
							.GetMethodByName(nameof(EqualityComparisonHelper.AssetDictionaryEquals))
							.MakeGenericInstanceMethod(dictionary.KeyTypeSignature, dictionary.ValueTypeSignature);
					}
					else if (dictionary.Child.Key.Child is TypeNode or PPtrNode or PrimitiveNode { TypeSignature: not SzArrayTypeSignature }
						&& dictionary.Child.Value.Child is ListNode childList
						&& childList.Child is TypeNode or PPtrNode or PrimitiveNode { TypeSignature: not SzArrayTypeSignature })
					{
						cachedMethod = equalityComparisonHelper
							.GetMethodByName(nameof(EqualityComparisonHelper.AssetDictionaryListEquals))
							.MakeGenericInstanceMethod(dictionary.KeyTypeSignature, childList.ElementTypeSignature);
					}
					else if (dictionary.Child.Key.Child is PairNode pairKey
						&& pairKey.Key.Child is TypeNode or PPtrNode or PrimitiveNode { TypeSignature: not SzArrayTypeSignature }
						&& pairKey.Value.Child is TypeNode or PPtrNode or PrimitiveNode { TypeSignature: not SzArrayTypeSignature }
						&& dictionary.Child.Value.Child is TypeNode or PPtrNode or PrimitiveNode { TypeSignature: not SzArrayTypeSignature })
					{
						cachedMethod = equalityComparisonHelper
							.GetMethodByName(nameof(EqualityComparisonHelper.AssetDictionaryPairEquals))
							.MakeGenericInstanceMethod(pairKey.Key.TypeSignature, pairKey.Value.TypeSignature, dictionary.ValueTypeSignature);
					}
					else
					{
						//Dictionary<Pair, Asset>
						throw new NotImplementedException();
					}
					break;
				case ListNode list:
					if (list.Child is TypeNode or PPtrNode)
					{
						cachedMethod = equalityComparisonHelper
							.GetMethodByName(nameof(EqualityComparisonHelper.AssetListEquals))
							.MakeGenericInstanceMethod(list.ElementTypeSignature);
					}
					else if (list.Child is PairNode
					{
						Key.Child: TypeNode or PPtrNode or PrimitiveNode { TypeSignature: not SzArrayTypeSignature },
						Value.Child: TypeNode or PPtrNode or PrimitiveNode { TypeSignature: not SzArrayTypeSignature },
					} childPair)
					{
						cachedMethod = equalityComparisonHelper
							.GetMethodByName(nameof(EqualityComparisonHelper.AssetPairListEquals))
							.MakeGenericInstanceMethod(childPair.Key.TypeSignature, childPair.Value.TypeSignature);
					}
					else
					{
						throw new NotImplementedException();
					}
					break;
				case PairNode pair:
					throw new NotImplementedException();
				default:
					throw new InvalidOperationException();
			}

			addToEqualityComparerMethods.Add(field.TypeSignature, cachedMethod);
		}

		return cachedMethod;
	}

	private static MethodDefinition OverrideObjectEquals(GeneratedClassInstance instance, TypeNode root, MethodDefinition equalsMethod)
	{
		TypeDefinition type = instance.Type;
		MethodDefinition method = type.AddMethod(
			nameof(object.Equals),
			MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
			SharedState.Instance.Importer.Boolean);
		method.AddParameter(SharedState.Instance.Importer.Object, "obj");
		CilInstructionCollection instructions = method.GetInstructions();

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.Add(CilOpCodes.Isinst, type);
		instructions.Add(CilOpCodes.Callvirt, equalsMethod);
		instructions.Add(CilOpCodes.Ret);

		return method;
	}

	private static void OverrideGetHashCode(GeneratedClassInstance instance, TypeNode root, ITypeDefOrRef hashCodeType, IMethodDefOrRef addMethod, IMethodDefOrRef toHashCodeMethod)
	{
		TypeDefinition type = instance.Type;
		MethodDefinition method = type.AddMethod(
			nameof(object.GetHashCode),
			MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
			SharedState.Instance.Importer.Int32);
		CilInstructionCollection instructions = method.GetInstructions();

		CilLocalVariable variable = instructions.AddLocalVariable(hashCodeType.ToTypeSignature());

		instructions.Add(CilOpCodes.Ldloca, variable);
		instructions.Add(CilOpCodes.Initobj, hashCodeType);

		foreach (FieldNode field in root.Children)
		{
			instructions.Add(CilOpCodes.Ldloca, variable);
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldfld, field.Field);
			switch (field.Child)
			{
				case DictionaryNode dictionary:
					instructions.Add(CilOpCodes.Callvirt, dictionary.GetCount);
					instructions.Add(CilOpCodes.Call, addMethod.MakeGenericInstanceMethod(SharedState.Instance.Importer.Int32));
					break;
				case ListNode list:
					instructions.Add(CilOpCodes.Callvirt, list.GetCount);
					instructions.Add(CilOpCodes.Call, addMethod.MakeGenericInstanceMethod(SharedState.Instance.Importer.Int32));
					break;
				case PrimitiveNode primitive:
					if (primitive.TypeSignature is SzArrayTypeSignature)
					{
						instructions.Add(CilOpCodes.Ldlen);
						instructions.Add(CilOpCodes.Call, addMethod.MakeGenericInstanceMethod(SharedState.Instance.Importer.Int32));
						break;
					}
					else
					{
						goto default;
					}
				default:
					instructions.Add(CilOpCodes.Call, addMethod.MakeGenericInstanceMethod(field.TypeSignature));
					break;
			}
		}

		instructions.Add(CilOpCodes.Ldloca, variable);
		instructions.Add(CilOpCodes.Call, toHashCodeMethod);
		instructions.Add(CilOpCodes.Ret);

		instructions.OptimizeMacros();
	}

	private static void AddEqualityOperators(GeneratedClassInstance instance, TypeNode root, MethodDefinition equalsMethod)
	{
		//Fine for now, but maybe we should use the equalsMethod to generate the operators.
		instance.Type.AddDefaultEqualityOperators(
			SharedState.Instance.Importer,
			out MethodDefinition equalityMethod,
			out MethodDefinition inequalityMethod);

		equalityMethod.AddNullableContextAttribute(NullableAnnotation.MaybeNull);
		inequalityMethod.AddNullableContextAttribute(NullableAnnotation.MaybeNull);
	}
}
