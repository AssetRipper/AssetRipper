using AssetRipper.AssemblyDumper.AST;
using AssetRipper.AssemblyDumper.InjectedTypes;
using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using System.Collections;

namespace AssetRipper.AssemblyDumper.Passes;

public static partial class Pass103_FillDependencyMethods
{
	public static void DoPass()
	{
		TypeDefinition injectedBaseType = SharedState.Instance.InjectHelperType(typeof(FetchDependenciesEnumerableBase));
		FieldDefinition currentField = injectedBaseType.Fields.First(t => t.Name == "_current");
		MethodDefinition baseConstructor = injectedBaseType.GetDefaultConstructor();

		ITypeDefOrRef commonPPtrTypeRef = SharedState.Instance.Importer.ImportType(typeof(PPtr));
		ITypeDefOrRef ienumerableRef = SharedState.Instance.Importer.ImportType(typeof(IEnumerable<>));
		ITypeDefOrRef valueTupleRef = SharedState.Instance.Importer.ImportType(typeof(ValueTuple<,>));
		GenericInstanceTypeSignature tupleGenericInstance = valueTupleRef.MakeGenericInstanceType(SharedState.Instance.Importer.String, commonPPtrTypeRef.ToTypeSignature());
		IMethodDescriptor emptyEnumerableMethod = SharedState.Instance.Importer.ImportMethod(typeof(Enumerable), m => m.Name == nameof(Enumerable.Empty))
			.MakeGenericInstanceMethod(tupleGenericInstance);
		GenericInstanceTypeSignature returnType = ienumerableRef.MakeGenericInstanceType(tupleGenericInstance);
		foreach (ClassGroupBase group in SharedState.Instance.AllGroups)
		{
			foreach (GeneratedClassInstance instance in group.Instances)
			{
				TypeNode rootNode = new(instance);

				bool anyPPtrs = rootNode.AnyPPtrs;
				if (!anyPPtrs && group is SubclassGroup)
				{
					//No need to generate a duplicate of the method in UnityAssetBase
					continue;
				}

				MethodDefinition method = instance.Type.AddMethod(nameof(UnityAssetBase.FetchDependencies), Pass063_CreateEmptyMethods.OverrideMethodAttributes, returnType);
				CilInstructionCollection instructions = method.GetInstructions();
				if (anyPPtrs)
				{
					MethodDefinition enumerableConstructor = MakeEnumerableType(injectedBaseType, currentField, baseConstructor, instance, rootNode);
					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Newobj, enumerableConstructor);
					instructions.Add(CilOpCodes.Ret);
				}
				else
				{
					instructions.Add(CilOpCodes.Call, emptyEnumerableMethod);
					instructions.Add(CilOpCodes.Ret);
				}
			}
		}
	}

	private static MethodDefinition MakeEnumerableType(TypeDefinition injectedBaseType, FieldDefinition currentField, MethodDefinition baseConstructor, GeneratedClassInstance instance, TypeNode rootNode)
	{
		TypeDefinition enumerableType = new(
			null,
			"FetchDependenciesEnumerable",
			TypeAttributes.NestedPrivate | TypeAttributes.Sealed,
			injectedBaseType);
		instance.Type.NestedTypes.Add(enumerableType);

		FieldDefinition thisField = enumerableType.AddField("_this", instance.Type.ToTypeSignature(), false, Visibility.Private);
		thisField.IsInitOnly = true;

		MethodDefinition enumerableConstructor = enumerableType.AddEmptyConstructor();
		enumerableConstructor.AddParameter(instance.Type.ToTypeSignature(), "_this");
		{
			CilInstructionCollection instructions = enumerableConstructor.GetInstructions();
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Call, baseConstructor);
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldarg_1);
			instructions.Add(CilOpCodes.Stfld, thisField);
			instructions.Add(CilOpCodes.Ret);
		}

		// CreateNew
		{
			MethodDefinition createNewMethod = enumerableType.AddMethod("CreateNew", MethodAttributes.FamilyAndAssembly | MethodAttributes.Virtual | MethodAttributes.HideBySig, enumerableType.ToTypeSignature());
			{
				CilInstructionCollection instructions = createNewMethod.GetInstructions();
				instructions.Add(CilOpCodes.Ldarg_0);
				instructions.Add(CilOpCodes.Ldfld, thisField);
				instructions.Add(CilOpCodes.Newobj, enumerableConstructor);
				instructions.Add(CilOpCodes.Ret);
			}
		}

		// MoveNext
		{
			MethodDefinition moveNextMethod = enumerableType.AddMethod(nameof(IEnumerator.MoveNext), MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig, SharedState.Instance.Importer.Boolean);
			DependencyMethodContext context = new()
			{
				Processor = moveNextMethod.GetInstructions(),
				Type = enumerableType,
				CurrentField = currentField,
				ThisField = thisField,
			};
			TypeNodeHelper.ApplyAsRoot(rootNode, context);
			context.Processor.OptimizeMacros();
		}

		return enumerableConstructor;
	}
}
