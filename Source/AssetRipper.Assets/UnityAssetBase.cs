using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.IO.Endian;
using System.Runtime.CompilerServices;

namespace AssetRipper.Assets;

/// <summary>
/// The artificial base class for all generated Unity classes
/// </summary>
public abstract class UnityAssetBase : IUnityAssetBase
{
	public virtual int SerializedVersion => 1;

	public virtual bool FlowMappedInYaml => false;

	public virtual bool IgnoreFieldInMetaFiles(string fieldName) => false;

	public virtual void ReadEditor(ref EndianSpanReader reader) => throw MethodNotSupported();

	public virtual void ReadRelease(ref EndianSpanReader reader) => throw MethodNotSupported();

	public virtual void WriteEditor(AssetWriter writer) => throw MethodNotSupported();

	public virtual void WriteRelease(AssetWriter writer) => throw MethodNotSupported();

	public virtual IEnumerable<(string, PPtr)> FetchDependencies()
	{
		return Enumerable.Empty<(string, PPtr)>();
	}

	public override string ToString()
	{
		string? name = (this as INamed)?.Name;
		return string.IsNullOrEmpty(name) ? GetType().Name : name;
	}

	public virtual void Reset() => throw MethodNotSupported();

	public virtual void CopyValues(IUnityAssetBase? source, PPtrConverter converter)
	{
	}

	public virtual void WalkEditor(AssetWalker walker) => WalkStandard(walker);

	public virtual void WalkRelease(AssetWalker walker) => WalkStandard(walker);

	public virtual void WalkStandard(AssetWalker walker)
	{
		if (walker.EnterAsset(this))
		{
			walker.ExitAsset(this);
		}
	}

	public virtual bool? AddToEqualityComparer(IUnityAssetBase other, AssetEqualityComparer comparer)
	{
		throw MethodNotSupported();
	}

	private NotSupportedException MethodNotSupported([CallerMemberName] string? methodName = null)
	{
		return new NotSupportedException($"{methodName} is not supported for {GetType().FullName}");
	}
}
