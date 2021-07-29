using Mono.Cecil;

namespace AssetRipper.Utils
{
	public class MyAssemblyResolver : DefaultAssemblyResolver
	{
		public void Register(AssemblyDefinition assembly)
		{
			RegisterAssembly(assembly);
		}
	}
}
