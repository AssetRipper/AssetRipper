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
