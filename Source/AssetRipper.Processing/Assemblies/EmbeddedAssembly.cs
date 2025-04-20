using AsmResolver.DotNet;

namespace AssetRipper.Processing.Assemblies;

internal static partial class EmbeddedAssembly
{
	public static ModuleDefinition Load()
	{
		return ModuleDefinition.FromBytes(Bytes);
	}
}
