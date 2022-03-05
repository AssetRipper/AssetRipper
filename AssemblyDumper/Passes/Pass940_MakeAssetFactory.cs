using AssemblyDumper.Utils;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;

namespace AssemblyDumper.Passes
{
	public static class Pass940_MakeAssetFactory
	{
		const TypeAttributes SealedClassAttributes = TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Public | TypeAttributes.Sealed;
		const MethodAttributes MethodOverrideAttributes = MethodAttributes.Public | MethodAttributes.ReuseSlot | MethodAttributes.HideBySig | MethodAttributes.Virtual;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public static TypeDefinition FactoryDefinition { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public static void DoPass()
		{
			System.Console.WriteLine("Pass 940: Make Asset Factory");
			FactoryDefinition = CreateFactoryDefinition();
			FactoryDefinition.AddCreateAsset();
		}

		private static TypeDefinition CreateFactoryDefinition()
		{
			TypeDefinition? result = new TypeDefinition(SharedState.RootNamespace, "AssetFactory", SealedClassAttributes, SharedState.Importer.ImportCommonType<AssetFactoryBase>());
			SharedState.Module.TopLevelTypes.Add(result);
			ConstructorUtils.AddDefaultConstructor(result);
			return result;
		}

		private static void AddCreateAsset(this TypeDefinition factoryDefinition)
		{
			ITypeDefOrRef iunityObjectBase = SharedState.Importer.ImportCommonType<IUnityObjectBase>();
			ITypeDefOrRef assetInfoType = SharedState.Importer.ImportCommonType<AssetInfo>();
			MethodDefinition createAsset = factoryDefinition.AddMethod("CreateAsset", MethodOverrideAttributes, iunityObjectBase);
			Parameter parameter = createAsset.AddParameter("assetInfo", assetInfoType);
			
			createAsset.CilMethodBody!.InitializeLocals = true;
			createAsset.CilMethodBody.Instructions.EmitSwitchStatement(parameter);
		}

		private static void EmitSwitchStatement(this CilInstructionCollection processor, Parameter parameter)
		{
			List<(int, IMethodDefOrRef)> constructors = GetAllAssetInfoConstructors();
			int count = constructors.Count;

			CilLocalVariable? switchCondition = new CilLocalVariable(SystemTypeGetter.Int32);
			processor.Owner.LocalVariables.Add(switchCondition);
			{
				processor.Add(CilOpCodes.Ldarg, parameter);
				IMethodDefOrRef propertyRef = SharedState.Importer.ImportCommonMethod<AssetInfo>(m => m.Name == "get_ClassNumber");
				processor.Add(CilOpCodes.Call, propertyRef);
			}
			processor.Add(CilOpCodes.Stloc, switchCondition);

			CilInstructionLabel[] nopInstructions = Enumerable.Range(0, count).Select(i => new CilInstructionLabel()).ToArray();
			CilInstructionLabel defaultNop = new CilInstructionLabel();
			for(int i = 0; i < count; i++)
			{
				processor.Add(CilOpCodes.Ldloc, switchCondition);
				processor.Add(CilOpCodes.Ldc_I4, constructors[i].Item1);
				processor.Add(CilOpCodes.Beq, nopInstructions[i]);
			}
			processor.Add(CilOpCodes.Br, defaultNop);
			for (int i = 0; i < count; i++)
			{
				nopInstructions[i].Instruction = processor.Add(CilOpCodes.Nop);
				processor.Add(CilOpCodes.Ldarg, parameter);
				processor.Add(CilOpCodes.Newobj, constructors[i].Item2);
				processor.Add(CilOpCodes.Ret);
			}
			defaultNop.Instruction = processor.Add(CilOpCodes.Nop);
			processor.Add(CilOpCodes.Ldnull);
			processor.Add(CilOpCodes.Ret);
		}

		private static List<(int, IMethodDefOrRef)> GetAllAssetInfoConstructors()
		{
			List<(int, IMethodDefOrRef)>? result = new List<(int, IMethodDefOrRef)>();
			foreach(KeyValuePair<string, Unity.UnityClass> pair in SharedState.ClassDictionary)
			{
				if(pair.Value.TypeID >= 0 && !pair.Value.IsAbstract) //Is an object and not abstract
				{
					result.Add((pair.Value.TypeID, SharedState.TypeDictionary[pair.Key].GetAssetInfoConstructor()));
				}
			}
			return result;
		}

		private static MethodDefinition GetAssetInfoConstructor(this TypeDefinition typeDefinition)
		{
			return typeDefinition.Methods.Where(x => x.IsConstructor && x.Parameters.Count == 1 && x.Parameters[0].ParameterType.Name == "AssetInfo").Single();
		}
	}
}
