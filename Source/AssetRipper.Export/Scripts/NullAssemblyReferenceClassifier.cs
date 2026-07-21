using ICSharpCode.Decompiler.Metadata;

namespace AssetRipper.Export.Scripts;

public sealed class NullAssemblyReferenceClassifier : AssemblyReferenceClassifier
{
	public static NullAssemblyReferenceClassifier Instance { get; } = new();
	public override bool IsGacAssembly(IAssemblyReference reference)
	{
		return false;
	}
}
