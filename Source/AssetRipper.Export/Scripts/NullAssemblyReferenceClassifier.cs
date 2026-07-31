using ICSharpCode.Decompiler.Metadata;

namespace AssetRipper.Export.Scripts;

public sealed class NullAssemblyReferenceClassifier : IAssemblyReferenceClassifier
{
	public static NullAssemblyReferenceClassifier Instance { get; } = new();
	bool IAssemblyReferenceClassifier.IsGacAssembly(IAssemblyReference reference)
	{
		return false;
	}

	bool IAssemblyReferenceClassifier.IsSharedAssembly(IAssemblyReference reference, [NotNullWhen(true)] out string? runtimePack)
	{
		runtimePack = null;
		return false;
	}
}
