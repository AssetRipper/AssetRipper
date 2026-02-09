using ICSharpCode.Decompiler.Metadata;
using IAssemblyResolver = ICSharpCode.Decompiler.Metadata.IAssemblyResolver;

namespace AssetRipper.Export.UnityProjects.Scripts;

internal static class AssemblyResolverExtensions
{
	public static bool TryResolve(this IAssemblyResolver resolver, IAssemblyReference reference, [NotNullWhen(true)] out MetadataFile? module)
	{
		module = resolver.Resolve(reference);
		return module is not null;
	}

	public static bool TryResolveModule(this IAssemblyResolver resolver, MetadataFile mainModule, string moduleName, [NotNullWhen(true)] out MetadataFile? module)
	{
		module = resolver.ResolveModule(mainModule, moduleName);
		return module is not null;
	}
}
