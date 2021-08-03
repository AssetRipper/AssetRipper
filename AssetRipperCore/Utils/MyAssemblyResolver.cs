using Mono.Cecil;

namespace AssetRipper.Core.Utils
{
	public class MyAssemblyResolver : DefaultAssemblyResolver
	{
		public void Register(AssemblyDefinition assembly)
		{
			RegisterAssembly(assembly);
		}
	}
}
