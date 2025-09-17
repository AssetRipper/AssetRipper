using AssetRipper.AssemblyDumper.Documentation;
using AssetRipper.AssemblyDumper.Types;
using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.AssemblyDumper.Passes;

public static class Pass940_MakeAssetFactory
{
	private const MethodAttributes CreateAssetAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static;
#nullable disable
	private static TypeSignature iunityObjectBase;
	private static TypeSignature assetInfoType;
	private static TypeSignature assetCollectionType;
	private static TypeSignature unityVersionType;
	private static TypeDefinition abstractClassException;
	private static MethodDefinition abstractClassExceptionConstructor;
	private static IMethodDefOrRef unityVersionIsGreaterEqualMethod;
	private static IMethodDefOrRef assetInfoConstructor;
	private static IMethodDefOrRef assetInfoCollectionGetMethod;
	private static IMethodDefOrRef assetCollectionVersionGetMethod;
#nullable enable
	private static readonly HashSet<int> importerClassIDs = new();

	public static void DoPass()
	{
		FindImporterGroups();

		iunityObjectBase = SharedState.Instance.Importer.ImportTypeSignature<IUnityObjectBase>();
		assetInfoType = SharedState.Instance.Importer.ImportTypeSignature<AssetInfo>();
		assetCollectionType = SharedState.Instance.Importer.ImportTypeSignature<AssetCollection>();
		unityVersionType = SharedState.Instance.Importer.ImportTypeSignature<UnityVersion>();

		abstractClassException = ExceptionCreator.CreateSimpleException(
			SharedState.Instance.Importer,
			SharedState.ExceptionsNamespace,
			"AbstractClassException",
			"Abstract class could not be created");
		abstractClassExceptionConstructor = abstractClassException.GetDefaultConstructor();

		unityVersionIsGreaterEqualMethod = SharedState.Instance.Importer.ImportMethod<UnityVersion>(m =>
			m.Name == nameof(UnityVersion.GreaterThanOrEquals) && m.Parameters.Count == 5);
		assetInfoConstructor = SharedState.Instance.Importer.ImportMethod<AssetInfo>(method =>
			method.Name == ".ctor"
			&& method.Parameters.Count == 3);
		assetInfoCollectionGetMethod = SharedState.Instance.Importer.ImportMethod<AssetInfo>(method =>
			method.Name == $"get_{nameof(AssetInfo.Collection)}");
		assetCollectionVersionGetMethod = SharedState.Instance.Importer.ImportMethod<AssetCollection>(method =>
			method.Name == $"get_{nameof(AssetCollection.Version)}");

		TypeDefinition factoryDefinition = CreateFactoryDefinition();
		List<GeneratedMethodInformation> assetInfoMethods = AddAllClassCreationMethods();
		MethodDefinition creationMethod = factoryDefinition.AddAssetInfoCreationMethod("Create", assetInfoMethods);
		AddMethodWithoutVersionParameter(creationMethod);

		// CreateSerialized
		{
			// On Unity 5 and later, we want to return null for ClassID 194 (NavMesh).
			// It was made obsolete and contains no data, so it's safe to ignore.
			// This is done in CreateSerialized so that version changing can still use the Create method.
			// See pass 1 for more information.
			MethodDefinition method = factoryDefinition.AddMethod("CreateSerialized", CreateAssetAttributes, iunityObjectBase);
			Parameter assetInfoParameter = method.AddParameter(assetInfoType, "info");
			Parameter versionParameter = method.AddParameter(unityVersionType, "version");

			CilInstructionCollection instructions = method.GetInstructions();

			CilLocalVariable switchCondition = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32);
			instructions.Add(CilOpCodes.Ldarga, assetInfoParameter);
			IMethodDefOrRef propertyRef = SharedState.Instance.Importer.ImportMethod<AssetInfo>(m => m.Name == $"get_{nameof(AssetInfo.ClassID)}");
			instructions.Add(CilOpCodes.Call, propertyRef);
			instructions.Add(CilOpCodes.Stloc, switchCondition);

			CilInstructionLabel defaultLabel = new();
			CilInstructionLabel returnLabel = new();

			instructions.Add(CilOpCodes.Ldloc, switchCondition);
			instructions.Add(CilOpCodes.Ldc_I4, 194);
			instructions.Add(CilOpCodes.Ceq);
			instructions.Add(CilOpCodes.Brfalse, defaultLabel);

			instructions.AddIsGreaterOrEqualToVersion(versionParameter, new UnityVersion(5));
			instructions.Add(CilOpCodes.Brfalse, defaultLabel);

			instructions.Add(CilOpCodes.Ldnull);
			instructions.Add(CilOpCodes.Br, returnLabel);

			defaultLabel.Instruction = instructions.Add(CilOpCodes.Nop);
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldarg_1);
			instructions.Add(CilOpCodes.Call, creationMethod);

			returnLabel.Instruction = instructions.Add(CilOpCodes.Ret);

			AddMethodWithoutVersionParameter(method);
		}
	}

	private static TypeDefinition CreateFactoryDefinition()
	{
		return StaticClassCreator.CreateEmptyStaticClass(SharedState.Instance.Module, SharedState.RootNamespace, "AssetFactory");
	}

	private static MethodDefinition AddAssetInfoCreationMethod(this TypeDefinition factoryDefinition, string methodName, List<GeneratedMethodInformation> constructors)
	{
		MethodDefinition method = factoryDefinition.AddMethod(methodName, CreateAssetAttributes, iunityObjectBase);
		Parameter assetInfoParameter = method.AddParameter(assetInfoType, "info");
		Parameter versionParameter = method.AddParameter(unityVersionType, "version");
		method.GetInstructions().EmitIdSwitchStatement(assetInfoParameter, versionParameter, constructors);
		return method;
	}

	private static void EmitIdSwitchStatement(this CilInstructionCollection instructions, Parameter assetInfoParameter, Parameter versionParameter, List<GeneratedMethodInformation> constructors)
	{
		int count = constructors.Count;

		CilLocalVariable switchCondition = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32);
		instructions.Add(CilOpCodes.Ldarga, assetInfoParameter);
		IMethodDefOrRef propertyRef = SharedState.Instance.Importer.ImportMethod<AssetInfo>(m => m.Name == $"get_{nameof(AssetInfo.ClassID)}");
		instructions.Add(CilOpCodes.Call, propertyRef);
		instructions.Add(CilOpCodes.Stloc, switchCondition);

		CilInstructionLabel[] nopInstructions = Enumerable.Range(0, count).Select(i => new CilInstructionLabel()).ToArray();
		CilInstructionLabel defaultNop = new CilInstructionLabel();
		for (int i = 0; i < count; i++)
		{
			instructions.Add(CilOpCodes.Ldloc, switchCondition);
			instructions.Add(CilOpCodes.Ldc_I4, constructors[i].ClassID);
			instructions.Add(CilOpCodes.Beq, nopInstructions[i]);
		}
		instructions.Add(CilOpCodes.Br, defaultNop);
		for (int i = 0; i < count; i++)
		{
			nopInstructions[i].Instruction = instructions.Add(CilOpCodes.Nop);
			GeneratedMethodInformation tuple = constructors[i];
			if (tuple.AssetInfoMethod is null)
			{
				instructions.AddThrowAbstractClassException();
			}
			else
			{
				instructions.Add(CilOpCodes.Ldarg, assetInfoParameter);

				if (tuple.HasVersionParameter)
				{
					instructions.Add(CilOpCodes.Ldarg, versionParameter);
				}

				instructions.Add(CilOpCodes.Call, tuple.AssetInfoMethod);
				instructions.Add(CilOpCodes.Ret);
			}
		}
		defaultNop.Instruction = instructions.Add(CilOpCodes.Nop);
		instructions.Add(CilOpCodes.Ldnull);
		instructions.Add(CilOpCodes.Ret);
		instructions.OptimizeMacros();
	}

	private static void AddMethodWithoutVersionParameter(MethodDefinition mainMethod)
	{
		//The main method has two parameters: UnityVersion and AssetInfo.
		//This generates a second method with one parameter: AssetInfo.
		//UnityVersion is pulled from AssetInfo.Collection.Version.
		MethodDefinition method = mainMethod.DeclaringType!.AddMethod(mainMethod.Name, mainMethod.Attributes, mainMethod.Signature!.ReturnType);
		Parameter parameter = method.AddParameter(assetInfoType, "info");
		CilInstructionCollection instructions = method.GetInstructions();

		//Load Parameter 1
		instructions.Add(CilOpCodes.Ldarg_0);

		//Load Parameter 2
		instructions.Add(CilOpCodes.Ldarga, parameter);
		instructions.Add(CilOpCodes.Call, assetInfoCollectionGetMethod);
		instructions.Add(CilOpCodes.Call, assetCollectionVersionGetMethod);

		//Call main method and return
		instructions.Add(CilOpCodes.Call, mainMethod);
		instructions.Add(CilOpCodes.Ret);
	}

	private static List<GeneratedMethodInformation> AddAllClassCreationMethods()
	{
		List<GeneratedMethodInformation> assetInfoMethods = new();
		foreach (ClassGroup group in SharedState.Instance.ClassGroups.Values.OrderBy(g => g.ID))
		{
			group.AddMethodsForGroup(
				out MethodDefinition? assetInfoMethod,
				out bool usesVersion);
			if (usesVersion && assetInfoMethod is not null)
			{
				AddMethodWithoutVersionParameter(assetInfoMethod);
			}
			assetInfoMethods.Add((group.ID, assetInfoMethod, usesVersion));
			if (Pass001_MergeMovedGroups.Changes.TryGetValue(group.ID, out IReadOnlyList<int>? aliasIDs))
			{
				foreach (int aliasID in aliasIDs)
				{
					assetInfoMethods.Add((aliasID, assetInfoMethod, usesVersion));
				}
			}
		}
		foreach (SubclassGroup group in SharedState.Instance.SubclassGroups.Values)
		{
			group.AddMethodsForGroup(out _, out _);
		}
		assetInfoMethods.Sort((a, b) => a.ClassID.CompareTo(b.ClassID));
		return assetInfoMethods;
	}

	private static void AddMethodsForGroup(
		this ClassGroupBase group,
		out MethodDefinition? assetInfoMethod,
		out bool usesVersion)
	{
		if (group.IsAbstractGroup())
		{
			usesVersion = false;
			assetInfoMethod = null;
		}
		else if (group.Instances.Count == 1)
		{
			usesVersion = false;
			TypeDefinition factoryClass = group.GetOrCreateMainClass();
			MethodDefinition generatedMethod = ImplementSingleCreationMethod(group, factoryClass);
			if (group is SubclassGroup)
			{
				assetInfoMethod = null;
			}
			else
			{
				assetInfoMethod = generatedMethod;
				MaybeImplementImporterCreationMethod(group, assetInfoMethod, usesVersion, factoryClass);
			}
		}
		else
		{
			usesVersion = true;
			TypeDefinition factoryClass = group.GetOrCreateMainClass();
			MethodDefinition generatedMethod = ImplementNormalCreationMethod(group, factoryClass);
			if (group is SubclassGroup)
			{
				assetInfoMethod = null;
			}
			else
			{
				assetInfoMethod = generatedMethod;
				MaybeImplementImporterCreationMethod(group, assetInfoMethod, usesVersion, factoryClass);
			}
		}

		static void MaybeImplementImporterCreationMethod(ClassGroupBase group, MethodDefinition assetInfoMethod, bool usesVersion, TypeDefinition factoryClass)
		{
			if (importerClassIDs.Contains(group.ID))
			{
				ImplementImporterCreationMethod(group, factoryClass, assetInfoMethod, usesVersion);
			}
		}
	}

	private static MethodDefinition ImplementSingleCreationMethod(ClassGroupBase group, TypeDefinition factoryClass)
	{
		MethodDefinition method = factoryClass.AddMethod("Create", CreateAssetAttributes, group.GetSingularTypeOrInterface().ToTypeSignature());
		CilInstructionCollection instructions = method.GetInstructions();
		Parameter? assetInfoParameter = group is SubclassGroup ? null : method.AddParameter(assetInfoType, "info");
		instructions.AddReturnNewConstructedObject(group.Instances[0].Type, assetInfoParameter);
		return method;
	}

	private static MethodDefinition ImplementNormalCreationMethod(ClassGroupBase group, TypeDefinition factoryClass)
	{
		MethodDefinition method = factoryClass.AddMethod("Create", CreateAssetAttributes, group.GetSingularTypeOrInterface().ToTypeSignature());
		Parameter? assetInfoParameter = group is SubclassGroup ? null : method.AddParameter(assetInfoType, "info");
		Parameter versionParameter = method.AddParameter(unityVersionType, "version");
		CilInstructionCollection instructions = method.GetInstructions();
		instructions.FillNormalCreationMethod(group, versionParameter, assetInfoParameter);
		return method;
	}

	private static MethodDefinition ImplementImporterCreationMethod(ClassGroupBase group, TypeDefinition factoryClass, MethodDefinition assetInfoMethod, bool hasVersion)
	{
		MethodDefinition method = factoryClass.AddMethod("Create",
			CreateAssetAttributes,
			group.GetSingularTypeOrInterface().ToTypeSignature());

		DocumentationHandler.AddMethodDefinitionLine(method, $"This is a special factory method for creating a temporary {SeeXmlTagGenerator.MakeCRef(group.Interface)} with no PathID.");

		CilInstructionCollection instructions = method.GetInstructions();

		method.AddParameter(assetCollectionType, "collection");
		instructions.Add(CilOpCodes.Ldarg_0);

		instructions.Add(CilOpCodes.Ldc_I4_0);//PathID
		instructions.Add(CilOpCodes.Conv_I8);
		instructions.Add(CilOpCodes.Ldc_I4, group.ID);//ClassID
		instructions.Add(CilOpCodes.Newobj, assetInfoConstructor);

		if (hasVersion)
		{
			method.AddParameter(unityVersionType, "version");
			instructions.Add(CilOpCodes.Ldarg_1);
		}

		instructions.Add(CilOpCodes.Call, assetInfoMethod);
		instructions.Add(CilOpCodes.Ret);
		instructions.OptimizeMacros();
		return method;
	}

	private static bool IsAbstractGroup(this ClassGroupBase group)
	{
		return group.Types.All(t => t.IsAbstract);
	}

	private static void AddThrowExceptionOrReturnNewObject(this CilInstructionCollection instructions, TypeDefinition objectType, Parameter? assetInfoParameter)
	{
		if (objectType.IsAbstract)
		{
			instructions.AddThrowAbstractClassException();
		}
		else
		{
			instructions.AddReturnNewConstructedObject(objectType, assetInfoParameter);
		}
	}

	private static void AddThrowAbstractClassException(this CilInstructionCollection instructions)
	{
		instructions.Add(CilOpCodes.Newobj, abstractClassExceptionConstructor);
		instructions.Add(CilOpCodes.Throw);
	}

	private static void AddReturnNewConstructedObject(this CilInstructionCollection instructions, TypeDefinition objectType, Parameter? assetInfoParameter)
	{
		if (assetInfoParameter is not null)
		{
			instructions.Add(CilOpCodes.Ldarg, assetInfoParameter);
			instructions.Add(CilOpCodes.Newobj, objectType.GetAssetInfoConstructor());
		}
		else
		{
			instructions.Add(CilOpCodes.Newobj, objectType.GetDefaultConstructor());
		}
		instructions.Add(CilOpCodes.Ret);
	}

	private static void AddIsGreaterOrEqualToVersion(this CilInstructionCollection instructions, Parameter versionParameter, UnityVersion versionToCompareWith)
	{
		instructions.Add(CilOpCodes.Ldarga, versionParameter);
		instructions.Add(CilOpCodes.Ldc_I4, (int)versionToCompareWith.Major);
		instructions.Add(CilOpCodes.Ldc_I4, (int)versionToCompareWith.Minor);
		instructions.Add(CilOpCodes.Ldc_I4, (int)versionToCompareWith.Build);
		instructions.Add(CilOpCodes.Ldc_I4, (int)versionToCompareWith.Type);
		instructions.Add(CilOpCodes.Ldc_I4, (int)versionToCompareWith.TypeNumber);
		instructions.Add(CilOpCodes.Call, unityVersionIsGreaterEqualMethod);
	}

	private static void FillNormalCreationMethod(this CilInstructionCollection instructions, ClassGroupBase group, Parameter versionParameter, Parameter? assetInfoParameter)
	{
		for (int i = group.Instances.Count - 1; i > 0; i--)
		{
			CilInstructionLabel label = new();
			UnityVersion startVersion = group.Instances[i].VersionRange.Start;
			instructions.AddIsGreaterOrEqualToVersion(versionParameter, startVersion);
			instructions.Add(CilOpCodes.Brfalse, label);
			instructions.AddThrowExceptionOrReturnNewObject(group.Instances[i].Type, assetInfoParameter);
			label.Instruction = instructions.Add(CilOpCodes.Nop);
		}
		instructions.AddThrowExceptionOrReturnNewObject(group.Instances[0].Type, assetInfoParameter);
		instructions.OptimizeMacros();
	}

	private static MethodDefinition GetAssetInfoConstructor(this TypeDefinition typeDefinition)
	{
		return typeDefinition.Methods.Where(x => x.IsConstructor && x.Parameters.Count == 1 && x.Parameters[0].ParameterType.Name == nameof(AssetInfo)).Single();
	}

	private static void FindImporterGroups()
	{
		foreach ((int id, ClassGroup group) in SharedState.Instance.ClassGroups)
		{
			if (group.Instances.Any(i => i.InheritsFromAssetImporter()))
			{
				importerClassIDs.Add(id);
			}
		}
	}

	private readonly record struct GeneratedMethodInformation(int ClassID, MethodDefinition? AssetInfoMethod, bool HasVersionParameter)
	{
		public static implicit operator GeneratedMethodInformation((int, MethodDefinition?, bool) tuple)
		{
			return new GeneratedMethodInformation(tuple.Item1, tuple.Item2, tuple.Item3);
		}
	}
}
