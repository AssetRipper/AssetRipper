using AssetRipper.AssemblyDumper.Attributes;
using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.AssemblyDumper.Types;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass558_TypeCache
{
	public static void DoPass()
	{
		TypeDefinition newTypeDef = StaticClassCreator.CreateEmptyStaticClass(SharedState.Instance.Module, SharedState.RootNamespace, "ClassIDTypeMap");

		GenericInstanceTypeSignature readOnlyDictionarySignature = SharedState.Instance.Importer.ImportType(typeof(IReadOnlyDictionary<,>))
			.MakeGenericInstanceType(SharedState.Instance.Importer.ImportTypeSignature<Type>(), Pass556_CreateClassIDTypeEnum.ClassIdTypeDefintion!.ToTypeSignature());
		GenericInstanceTypeSignature dictionarySignature = SharedState.Instance.Importer.ImportType(typeof(Dictionary<,>))
			.MakeGenericInstanceType(SharedState.Instance.Importer.ImportTypeSignature<Type>(), Pass556_CreateClassIDTypeEnum.ClassIdTypeDefintion!.ToTypeSignature());
		IMethodDefOrRef dictionaryConstructor = MethodUtils.MakeConstructorOnGenericType(SharedState.Instance.Importer, dictionarySignature, 0);
		IMethodDefOrRef addMethod = MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, dictionarySignature, SharedState.Instance.Importer.LookupMethod(typeof(Dictionary<,>), m => m.Name == "Add"));
		IMethodDefOrRef getTypeFromHandleMethod = SharedState.Instance.Importer.ImportMethod(typeof(Type), m => m.Name == nameof(Type.GetTypeFromHandle));

		const string propertyName = "Dictionary";
		FieldDefinition field = newTypeDef.AddField($"<{propertyName}>k__BackingField", readOnlyDictionarySignature, true, Visibility.Private);
		field.Attributes |= FieldAttributes.InitOnly;
		field.AddCompilerGeneratedAttribute(SharedState.Instance.Importer);

		MethodDefinition staticConstructor = newTypeDef.AddEmptyConstructor(true);
		CilInstructionCollection instructions = staticConstructor.GetInstructions();
		instructions.Add(CilOpCodes.Newobj, dictionaryConstructor);
		foreach ((int id, ClassGroup group) in SharedState.Instance.ClassGroups)
		{
			instructions.Add(CilOpCodes.Dup);
			instructions.Add(CilOpCodes.Ldtoken, group.Interface);
			instructions.Add(CilOpCodes.Call, getTypeFromHandleMethod);
			instructions.Add(CilOpCodes.Ldc_I4, id);
			instructions.Add(CilOpCodes.Call, addMethod);

			foreach (TypeDefinition type in group.Types)
			{
				instructions.Add(CilOpCodes.Dup);
				instructions.Add(CilOpCodes.Ldtoken, type);
				instructions.Add(CilOpCodes.Call, getTypeFromHandleMethod);
				instructions.Add(CilOpCodes.Ldc_I4, id);
				instructions.Add(CilOpCodes.Call, addMethod);
			}
		}
		instructions.Add(CilOpCodes.Stsfld, field);
		instructions.Add(CilOpCodes.Ret);

		instructions.OptimizeMacros();

		newTypeDef.ImplementGetterProperty(
				propertyName,
				MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName,
				readOnlyDictionarySignature,
				field)
			.GetMethod!.AddCompilerGeneratedAttribute(SharedState.Instance.Importer);
	}
}
