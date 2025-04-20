using AsmResolver.DotNet;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// ILSpy needs System.Runtime.CompilerServices.Unsafe and System.Runtime.InteropServices to decompile some code.
/// This generates these assemblies with forwarders to the mscorlib types.
/// </summary>
public sealed class ForwardingAssemblyGenerator : IAssetProcessor
{
	public void Process(GameData gameData) => AddCompilerHelperModules(gameData.AssemblyManager);

	private static void AddCompilerHelperModules(IAssemblyManager manager)
	{
		AssemblyDefinition? mscorlib = manager.Mscorlib;
		if (mscorlib is null)
		{
			return;
		}

		manager.ClearStreamCache();

		const string UnsafeAssemblyName = "System.Runtime.CompilerServices.Unsafe";
		if (!HasAssembly(manager, UnsafeAssemblyName))
		{
			AssemblyReference corLibReference = new(mscorlib);
			AssemblyDefinition assembly = new(UnsafeAssemblyName, (Version)mscorlib.Version.Clone());
			ModuleDefinition module = new(UnsafeAssemblyName, corLibReference);
			module.AssemblyReferences.Add(corLibReference);
			assembly.Modules.Add(module);

			module.ExportedTypes.Add(new ExportedType(corLibReference, "System.Runtime.CompilerServices", "Unsafe"));

			manager.Add(assembly);
		}

		const string InteropServicesAssemblyName = "System.Runtime.InteropServices";
		if (!HasAssembly(manager, InteropServicesAssemblyName))
		{
			AssemblyReference corLibReference = new(mscorlib);
			AssemblyDefinition assembly = new(InteropServicesAssemblyName, (Version)mscorlib.Version.Clone());
			ModuleDefinition module = new(InteropServicesAssemblyName, corLibReference);
			module.AssemblyReferences.Add(corLibReference);
			assembly.Modules.Add(module);

			const string InteropServicesNamespace = "System.Runtime.InteropServices";
			foreach (TypeDefinition type in mscorlib.ManifestModule!.TopLevelTypes.Where(t =>
				t.IsPublic &&
				t.Namespace is not null &&
				t.Namespace.Value.StartsWith(InteropServicesNamespace, StringComparison.Ordinal)))
			{
				module.ExportedTypes.Add(new ExportedType(corLibReference, type.Namespace, type.Name));
			}

			manager.Add(assembly);
		}
	}

	private static bool HasAssembly(IAssemblyManager manager, string name)
	{
		return manager.GetAssemblies().Any(a => a.Name == name);
	}
}
