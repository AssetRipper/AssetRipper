using AssetRipper.AssemblyDumper.Attributes;
using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.AssemblyDumper.Types;

namespace AssetRipper.AssemblyDumper.Passes;

public static class Pass555_CreateCommonString
{
	public static void DoPass()
	{
		ThrowIfStringCountIsWrong();
		TypeDefinition newTypeDef = StaticClassCreator.CreateEmptyStaticClass(SharedState.Instance.Module, SharedState.RootNamespace, "CommonString");

		GenericInstanceTypeSignature readOnlyUintStringDictionary = SharedState.Instance.Importer.ImportType(typeof(IReadOnlyDictionary<,>))
			.MakeGenericInstanceType(SharedState.Instance.Importer.UInt32, SharedState.Instance.Importer.String);
		GenericInstanceTypeSignature uintStringDictionary = SharedState.Instance.Importer.ImportType(typeof(Dictionary<,>))
			.MakeGenericInstanceType(SharedState.Instance.Importer.UInt32, SharedState.Instance.Importer.String);
		IMethodDefOrRef dictionaryConstructor = MethodUtils.MakeConstructorOnGenericType(SharedState.Instance.Importer, uintStringDictionary, 0);
		IMethodDefOrRef addMethod = MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, uintStringDictionary, SharedState.Instance.Importer.LookupMethod(typeof(Dictionary<,>), m => m.Name == "Add"));

		const string propertyName = "Dictionary";
		FieldDefinition field = newTypeDef.AddField($"<{propertyName}>k__BackingField", readOnlyUintStringDictionary, true, Visibility.Private);
		field.Attributes |= FieldAttributes.InitOnly;
		field.AddCompilerGeneratedAttribute(SharedState.Instance.Importer);

		MethodDefinition staticConstructor = newTypeDef.AddEmptyConstructor(true);
		CilInstructionCollection instructions = staticConstructor.GetInstructions();
		instructions.Add(CilOpCodes.Newobj, dictionaryConstructor);
		foreach ((uint index, string str) in SharedState.Instance.CommonString.Strings)
		{
			instructions.Add(CilOpCodes.Dup);
			instructions.Add(CilOpCodes.Ldc_I4, (int)index);
			instructions.Add(CilOpCodes.Ldstr, str);
			instructions.Add(CilOpCodes.Call, addMethod);
		}
		instructions.Add(CilOpCodes.Stsfld, field);
		instructions.Add(CilOpCodes.Ret);

		instructions.OptimizeMacros();

		newTypeDef.ImplementGetterProperty(
				propertyName,
				MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName,
				readOnlyUintStringDictionary,
				field)
			.GetMethod!.AddCompilerGeneratedAttribute(SharedState.Instance.Importer);
	}

	private static void ThrowIfStringCountIsWrong()
	{
		int count = SharedState.Instance.CommonString.Strings.Count;
		if (count != 112)
		{
			throw new Exception($"The size of Common String has changed! {count}");
		}
	}
}
