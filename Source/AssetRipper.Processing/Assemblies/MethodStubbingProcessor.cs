using AsmResolver.DotNet;
using AssetRipper.CIL;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// This processor replaces the method bodies of all methods with a minimal implementation.
/// </summary>
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
				foreach (MethodDefinition method in type.Methods)
				{
					method.ReplaceMethodBodyWithMinimalImplementation();
				}
			}
		}
	}
}
