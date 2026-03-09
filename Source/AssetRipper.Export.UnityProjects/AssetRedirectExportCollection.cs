using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files.SerializedFiles;
using System.Diagnostics;

namespace AssetRipper.Export.UnityProjects;

/// <summary>
/// Redirects one asset to the export identity of another asset.
/// </summary>
public sealed record class AssetRedirectExportCollection(IUnityObjectBase Asset, IUnityObjectBase TargetAsset) : IExportCollection
{
	AssetCollection IExportCollection.File => Asset.Collection;

	TransferInstructionFlags IExportCollection.Flags => Asset.Collection.Flags;

	IEnumerable<IUnityObjectBase> IExportCollection.Assets => [Asset];

	public string Name => Asset.GetBestName();

	bool IExportCollection.Contains(IUnityObjectBase asset)
	{
		return ReferenceEquals(Asset, asset);
	}

	MetaPtr IExportCollection.CreateExportPointer(IExportContainer container, IUnityObjectBase asset, bool isLocal)
	{
		ThrowIfNotAsset(asset);
		return container.CreateExportPointer(TargetAsset);
	}

	bool IExportCollection.Export(IExportContainer container, string projectDirectory, FileSystem fileSystem)
	{
		throw new NotSupportedException();
	}

	bool IExportCollection.Exportable => false;

	long IExportCollection.GetExportID(IExportContainer container, IUnityObjectBase asset)
	{
		ThrowIfNotAsset(asset);
		return container.GetExportID(TargetAsset);
	}

	[StackTraceHidden]
	private void ThrowIfNotAsset(IUnityObjectBase asset)
	{
		if (!ReferenceEquals(Asset, asset))
		{
			throw new ArgumentException($"The asset must be the same one referenced in this collection.", nameof(asset));
		}
	}
}
