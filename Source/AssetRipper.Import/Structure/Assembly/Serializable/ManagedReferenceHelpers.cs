using AssetRipper.Assets;
using AssetRipper.IO.Files;

namespace AssetRipper.Import.Structure.Assembly.Serializable;

internal readonly record struct ManagedReferenceTypeKey(string AssemblyName, string Namespace, string ClassName)
{
	public bool IsEmpty => string.IsNullOrEmpty(AssemblyName) && string.IsNullOrEmpty(Namespace) && string.IsNullOrEmpty(ClassName);

	public ManagedReferenceTypeKey Normalize()
	{
		return new ManagedReferenceTypeKey(NormalizeAssemblyName(AssemblyName), Namespace, ClassName);
	}

	public static string NormalizeAssemblyName(string assemblyName)
	{
		if (string.IsNullOrWhiteSpace(assemblyName))
		{
			return assemblyName;
		}

		int separatorIndex = assemblyName.IndexOf(',');
		if (separatorIndex >= 0)
		{
			assemblyName = assemblyName[..separatorIndex];
		}

		return SpecialFileNames.FixAssemblyName(assemblyName.Trim());
	}
}

internal sealed record ManagedReferenceTypeDescriptor(string AssemblyName, string Namespace, string ClassName)
{
	public ManagedReferenceTypeKey Key => new ManagedReferenceTypeKey(AssemblyName, Namespace, ClassName).Normalize();
	public bool IsTerminus => Key == ManagedReferenceResolver.TerminusKey;
}

internal sealed class ManagedReferenceEntry
{
	public long Rid { get; set; }
	public ManagedReferenceTypeDescriptor Type { get; set; } = new ManagedReferenceTypeDescriptor("", "", "");
	public IUnityAssetBase? Data { get; set; }
}
