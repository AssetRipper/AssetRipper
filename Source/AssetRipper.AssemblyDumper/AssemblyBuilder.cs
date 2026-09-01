namespace AssetRipper.AssemblyDumper;

public class AssemblyBuilder
{
	public CachedReferenceImporter Importer { get; }
	public AssemblyDefinition Assembly { get; }
	public ModuleDefinition Module { get; }
	public RuntimeContext RuntimeContext { get; }

	public AssemblyBuilder(string assemblyName, Version version, AssemblyReference corLib)
	{
		RuntimeContext = new RuntimeContext(DotNetRuntimeInfo.NetCoreApp(corLib.Version), (bool?)null, corLib);
		Assembly = new AssemblyDefinition(assemblyName, version);
		Module = new ModuleDefinition(assemblyName, corLib);
		Assembly.Modules.Add(Module);
		RuntimeContext.AddAssembly(Assembly);
		Importer = new CachedReferenceImporter(Module);
	}
}
