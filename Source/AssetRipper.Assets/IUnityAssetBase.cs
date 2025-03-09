using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Assets;

public interface IUnityAssetBase : IEndianSpanReadable, IAssetWritable
{
	int SerializedVersion { get; }
	/// <summary>
	/// <see cref="TransferMetaFlags.TransferUsingFlowMappingStyle"/>
	/// </summary>
	bool FlowMappedInYaml { get; }
	/// <summary>
	/// <see cref="TransferMetaFlags.IgnoreInMetaFiles"/>
	/// </summary>
	/// <param name="fieldName">The field's name, according to the original naming.</param>
	/// <returns>True if the field should not be emitted in yaml meta files.</returns>
	bool IgnoreFieldInMetaFiles(string fieldName);
	void CopyValues(IUnityAssetBase? source, PPtrConverter converter);
	void Reset();
	/// <summary>
	/// Walk this asset using original naming.
	/// </summary>
	/// <param name="walker">A walker for traversal.</param>
	void WalkEditor(AssetWalker walker);
	/// <summary>
	/// Walk this asset using original naming.
	/// </summary>
	/// <param name="walker">A walker for traversal.</param>
	void WalkRelease(AssetWalker walker);
	/// <summary>
	/// Walk this asset using standardized naming.
	/// </summary>
	/// <param name="walker">A walker for traversal.</param>
	void WalkStandard(AssetWalker walker);
	IEnumerable<(string, PPtr)> FetchDependencies();
	/// <summary>
	/// Compares this object to another object for deep value equality.
	/// </summary>
	/// <remarks>
	/// <paramref name="other"/> is expected to be not null and of the same type as this object.
	/// </remarks>
	/// <param name="other">The other object.</param>
	/// <param name="comparer">The <see cref="AssetEqualityComparer"/> to which any dependent comparisons are added.</param>
	/// <returns>Null if it could not be immediately determined</returns>
	bool? AddToEqualityComparer(IUnityAssetBase other, AssetEqualityComparer comparer);
}
public static class UnityAssetBaseExtensions
{
	public static void Read(this IUnityAssetBase asset, ref EndianSpanReader reader, TransferInstructionFlags flags)
	{
		if (flags.IsRelease())
		{
			asset.ReadRelease(ref reader);
		}
		else
		{
			asset.ReadEditor(ref reader);
		}
	}
}
