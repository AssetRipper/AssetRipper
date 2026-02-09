using AsmResolver.DotNet;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// This class provides the polyfill module used in the <see cref="AttributePolyfillGenerator"/>.
/// </summary>
internal static partial class EmbeddedAssembly
{
	/// <summary>
	/// Loads a module definition from the embedded byte array.
	/// </summary>
	/// <returns>The loaded module definition.</returns>
	public static ModuleDefinition Load()
	{
		return ModuleDefinition.FromBytes(Bytes);
	}
}
