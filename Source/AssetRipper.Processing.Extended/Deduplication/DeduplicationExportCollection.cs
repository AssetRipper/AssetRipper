using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Export.UnityProjects;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Processing.Extended.Deduplication;

/// <summary>
/// Suppresses a redundant asset and points every reference at the canonical copy.
/// </summary>
/// <remarks>
/// Deliberately not built on <see cref="DeletedAssetsInformation"/>: that path returns
/// <c>MetaPtr.CreateMissingReference</c>, which would break every reference to the duplicate.
/// Redirecting through the container resolves the canonical asset's real pointer at export
/// time, which is what keeps references intact.
/// </remarks>
internal sealed class DeduplicationExportCollection(IUnityObjectBase Duplicate, IUnityObjectBase Canonical) : IExportCollection
{
	bool IExportCollection.Exportable => false;

	AssetCollection IExportCollection.File => Duplicate.Collection;

	TransferInstructionFlags IExportCollection.Flags => Duplicate.Collection.Flags;

	IEnumerable<IUnityObjectBase> IExportCollection.Assets => [Duplicate];

	string IExportCollection.Name => nameof(DeduplicationExportCollection);

	bool IExportCollection.Contains(IUnityObjectBase asset) => ReferenceEquals(asset, Duplicate);

	MetaPtr IExportCollection.CreateExportPointer(IExportContainer container, IUnityObjectBase asset, bool isLocal)
	{
		return container.CreateExportPointer(Canonical);
	}

	bool IExportCollection.Export(IExportContainer container, string projectDirectory, FileSystem fileSystem)
	{
		throw new NotSupportedException();
	}

	long IExportCollection.GetExportID(IExportContainer container, IUnityObjectBase asset)
	{
		throw new NotSupportedException();
	}
}
