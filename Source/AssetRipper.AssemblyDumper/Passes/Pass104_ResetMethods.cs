using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.AssemblyDumper.Types;
using AssetRipper.Assets;
using AssetRipper.Assets.Generics;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass104_ResetMethods
{
#nullable disable
	private static IMethodDefOrRef emptyString;
#nullable enable
	public static void DoPass()
	{
		emptyString = SharedState.Instance.Importer.ImportMethod<Utf8String>(method => method.Name == $"get_{nameof(Utf8String.Empty)}");
		foreach (ClassGroupBase group in SharedState.Instance.AllGroups)
		{
			foreach (GeneratedClassInstance instance in group.Instances)
			{
				MethodDefinition method = instance.Type.GetMethodByName(nameof(UnityAssetBase.Reset));
				CilInstructionCollection instructions = method.GetInstructions();
				instructions.FillProcessor(instance);
			}
		}
	}

	private static void FillProcessor(this CilInstructionCollection instructions, GeneratedClassInstance instance)
	{
		foreach (ClassProperty classProperty in instance.Properties)
		{
			if (classProperty.BackingField is FieldDefinition field)
			{
				TypeSignature fieldTypeSignature = field.Signature!.FieldType;
				if (fieldTypeSignature is CorLibTypeSignature corLibTypeSignature)
				{
					instructions.Add(CilOpCodes.Ldarg_0);
					ElementType elementType = corLibTypeSignature.ElementType;
					LoadDefaultPrimitiveValue(instructions, elementType);
					instructions.Add(CilOpCodes.Stfld, field);
				}
				else if (fieldTypeSignature is TypeDefOrRefSignature typeDefOrRefSignature)
				{
					if (typeDefOrRefSignature is { Namespace: "AssetRipper.Primitives", Name: nameof(Utf8String) })
					{
						instructions.Add(CilOpCodes.Ldarg_0);
						instructions.Add(CilOpCodes.Call, emptyString);
						instructions.Add(CilOpCodes.Stfld, field);
					}
					else
					{
						TypeDefinition fieldType = (TypeDefinition)typeDefOrRefSignature.Type;

						instructions.Add(CilOpCodes.Ldarg_0);
						instructions.Add(CilOpCodes.Ldfld, field);
						instructions.Add(CilOpCodes.Callvirt, fieldType.GetMethodByName(nameof(UnityAssetBase.Reset)));
					}
				}
				else if (fieldTypeSignature is SzArrayTypeSignature arrayTypeSignature)
				{
					MethodSpecification emptyArray = MakeEmptyArrayMethod(arrayTypeSignature);
					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Call, emptyArray);
					instructions.Add(CilOpCodes.Stfld, field);
				}
				else
				{
					GenericInstanceTypeSignature genericSignature = (GenericInstanceTypeSignature)fieldTypeSignature;
					if (genericSignature.GenericType.Name == $"{nameof(AssetDictionary<int, int>)}`2")
					{
						IMethodDefOrRef clearMethod = MakeDictionaryClearMethod(genericSignature);
						instructions.Add(CilOpCodes.Ldarg_0);
						instructions.Add(CilOpCodes.Ldfld, field);
						instructions.Add(CilOpCodes.Call, clearMethod);
					}
					else if (genericSignature.GenericType.Name == $"{nameof(AssetList<int>)}`1")
					{
						IMethodDefOrRef clearMethod = MakeListClearMethod(genericSignature);
						instructions.Add(CilOpCodes.Ldarg_0);
						instructions.Add(CilOpCodes.Ldfld, field);
						instructions.Add(CilOpCodes.Call, clearMethod);
					}
					else if (genericSignature.GenericType.Name == $"{nameof(AssetPair<int, int>)}`2")
					{
						TypeSignature keySignature = genericSignature.TypeArguments[0];
						if (keySignature is CorLibTypeSignature keyCorLibTypeSignature)
						{
							IMethodDefOrRef setKeyMethod = MakeSetKeyMethod(genericSignature);
							instructions.Add(CilOpCodes.Ldarg_0);
							instructions.Add(CilOpCodes.Ldfld, field);
							ElementType elementType = keyCorLibTypeSignature.ElementType;
							LoadDefaultPrimitiveValue(instructions, elementType);
							instructions.Add(CilOpCodes.Call, setKeyMethod);
						}
						else if (keySignature is TypeDefOrRefSignature keyTypeDefOrRefSignature)
						{
							TypeDefinition keyType = (TypeDefinition)keyTypeDefOrRefSignature.Type;

							IMethodDefOrRef getKeyMethod = MakeGetKeyMethod(genericSignature);
							instructions.Add(CilOpCodes.Ldarg_0);
							instructions.Add(CilOpCodes.Ldfld, field);
							instructions.Add(CilOpCodes.Call, getKeyMethod);
							instructions.Add(CilOpCodes.Callvirt, keyType.GetMethodByName(nameof(UnityAssetBase.Reset)));
						}
						else
						{
							throw new NotSupportedException();
						}

						TypeSignature valueSignature = genericSignature.TypeArguments[1];
						if (valueSignature is CorLibTypeSignature valueCorLibTypeSignature)
						{
							IMethodDefOrRef setValueMethod = MakeSetValueMethod(genericSignature);
							instructions.Add(CilOpCodes.Ldarg_0);
							instructions.Add(CilOpCodes.Ldfld, field);
							ElementType elementType = valueCorLibTypeSignature.ElementType;
							LoadDefaultPrimitiveValue(instructions, elementType);
							instructions.Add(CilOpCodes.Call, setValueMethod);
						}
						else if (valueSignature is TypeDefOrRefSignature valueTypeDefOrRefSignature)
						{
							TypeDefinition valueType = (TypeDefinition)valueTypeDefOrRefSignature.Type;

							IMethodDefOrRef getValueMethod = MakeGetValueMethod(genericSignature);
							instructions.Add(CilOpCodes.Ldarg_0);
							instructions.Add(CilOpCodes.Ldfld, field);
							instructions.Add(CilOpCodes.Call, getValueMethod);
							instructions.Add(CilOpCodes.Callvirt, valueType.GetMethodByName(nameof(UnityAssetBase.Reset)));
						}
						else
						{
							throw new NotSupportedException();
						}
					}
					else
					{
						throw new NotSupportedException();
					}
				}
			}
		}
		instructions.Add(CilOpCodes.Ret);
	}

	private static IMethodDefOrRef MakeDictionaryClearMethod(GenericInstanceTypeSignature genericSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			genericSignature,
			SharedState.Instance.Importer.ImportMethod(typeof(AssetDictionary<,>),
				m => m.Name == nameof(AssetDictionary<int, int>.Clear)));
	}

	private static IMethodDefOrRef MakeListClearMethod(GenericInstanceTypeSignature genericSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			genericSignature,
			SharedState.Instance.Importer.ImportMethod(typeof(AssetList<>),
				m => m.Name == nameof(AssetList<int>.Clear)));
	}

	private static IMethodDefOrRef MakeGetKeyMethod(GenericInstanceTypeSignature genericSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			genericSignature,
			SharedState.Instance.Importer.ImportMethod(typeof(AssetPair<,>),
				m => m.Name == $"get_{nameof(AssetPair<int, int>.Key)}"));
	}

	private static IMethodDefOrRef MakeSetKeyMethod(GenericInstanceTypeSignature genericSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			genericSignature,
			SharedState.Instance.Importer.ImportMethod(typeof(AssetPair<,>),
				m => m.Name == $"set_{nameof(AssetPair<int, int>.Key)}"));
	}

	private static IMethodDefOrRef MakeGetValueMethod(GenericInstanceTypeSignature genericSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			genericSignature,
			SharedState.Instance.Importer.ImportMethod(typeof(AssetPair<,>),
				m => m.Name == $"get_{nameof(AssetPair<int, int>.Value)}"));
	}

	private static IMethodDefOrRef MakeSetValueMethod(GenericInstanceTypeSignature genericSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			genericSignature,
			SharedState.Instance.Importer.ImportMethod(typeof(AssetPair<,>),
				m => m.Name == $"set_{nameof(AssetPair<int, int>.Value)}"));
	}

	private static MethodSpecification MakeEmptyArrayMethod(SzArrayTypeSignature arrayTypeSignature)
	{
		TypeSignature elementType = arrayTypeSignature.BaseType;
		MethodSpecification emptyArray = SharedState.Instance.Importer
			.ImportMethod(typeof(Array), m => m.Name == nameof(Array.Empty))
			.MakeGenericInstanceMethod(elementType);
		return emptyArray;
	}

	private static void LoadDefaultPrimitiveValue(CilInstructionCollection instructions, ElementType elementType)
	{
		switch (elementType)
		{
			case ElementType.String:
				throw new NotSupportedException();
			case ElementType.R4:
				instructions.Add(CilOpCodes.Ldc_R4, 0f);
				break;
			case ElementType.R8:
				instructions.Add(CilOpCodes.Ldc_R8, 0d);
				break;
			case ElementType.I8 or ElementType.U8:
				instructions.Add(CilOpCodes.Ldc_I4_0);
				instructions.Add(CilOpCodes.Conv_I8);
				break;
			default:
				instructions.Add(CilOpCodes.Ldc_I4_0);
				break;
		}
	}
}
