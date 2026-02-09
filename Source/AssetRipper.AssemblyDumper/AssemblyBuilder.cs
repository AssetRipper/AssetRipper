namespace AssetRipper.AssemblyDumper;

public class AssemblyBuilder
{
	public CachedReferenceImporter Importer { get; }
	public AssemblyDefinition Assembly { get; }
	public ModuleDefinition Module { get; }

	public AssemblyBuilder(string assemblyName, Version version, AssemblyReference corLib)
	{
		Assembly = new AssemblyDefinition(assemblyName, version);
		Module = new ModuleDefinition(assemblyName, corLib);
		Assembly.Modules.Add(Module);
		Importer = new CachedReferenceImporter(Module);
	}
}
