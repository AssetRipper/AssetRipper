using AsmResolver.DotNet;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// Removes System.Reflection.AssemblyKeyFileAttribute from assemblies.
/// It causes compile errors because the key file is not present.
/// </summary>
public sealed class RemoveAssemblyKeyFileAttributeProcessor : IAssetProcessor
{
	public void Process(GameData gameData) => Process(gameData.AssemblyManager);
	private static void Process(IAssemblyManager manager)
	{
		foreach (AssemblyDefinition assembly in manager.GetAssemblies())
		{
			for (int i = assembly.CustomAttributes.Count - 1; i >= 0; i--)
			{
				if (assembly.CustomAttributes[i].IsType("System.Reflection", "AssemblyKeyFileAttribute"))
				{
					assembly.CustomAttributes.RemoveAt(i);
				}
			}
		}
	}
}
