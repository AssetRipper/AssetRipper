using AsmResolver.DotNet;
using AssetRipper.CIL;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;
public sealed class MethodStubbingProcessor : IAssetProcessor
{
	public void Process(GameData gameData) => Process(gameData.AssemblyManager);

	private static void Process(IAssemblyManager manager)
	{
		if (manager.ScriptingBackend == ScriptingBackend.IL2Cpp)
		{
			return;
		}

		manager.ClearStreamCache();

		foreach (ModuleDefinition module in manager.GetAssemblies().SelectMany(a => a.Modules).Where(m => m.TopLevelTypes.Count > 0))
		{
			foreach (TypeDefinition type in module.GetAllTypes())
			{
				//RemoveCompilerGeneratedNestedTypes(type);
				foreach (MethodDefinition method in type.Methods)
				{
					method.FillMethodBodyWithStub();
				}
			}
		}
	}

	//Possible improvement for the future: removing unnecessary nested types.
	//If this is implemented, it might be better for it to be its own processor.
	private static void RemoveCompilerGeneratedNestedTypes(TypeDefinition type)
	{
		for (int i = type.NestedTypes.Count - 1; i >= 0; i--)
		{
			TypeDefinition nestedType = type.NestedTypes[i];

			//This check is insufficient to determine if a nested type can be removed.
			//The problem is that it may be used in a member signature.
			//Solving this requires checking each of the members on the declaring type
			//for any reference to the nested type.
			//It has to be thorough since the nested type could be in a generic, array, by ref, etc.
			if (nestedType.IsNestedPrivate && nestedType.IsCompilerGenerated())
			{
				type.NestedTypes.RemoveAt(i);
			}
		}
	}
}
