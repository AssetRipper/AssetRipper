using AssetRipper.Assets;

namespace AssetRipper.Export.PrimaryContent;

public class SingleExportCollection<T> : ExportCollectionBase where T : IUnityObjectBase
{
	public SingleExportCollection(IContentExtractor contentExtractor, T asset)
	{
		ContentExtractor = contentExtractor ?? throw new ArgumentNullException(nameof(contentExtractor));
		Asset = asset ?? throw new ArgumentNullException(nameof(asset));
	}

	public override bool Export(string projectDirectory, FileSystem fileSystem)
	{
		string subPath = fileSystem.Path.Join(projectDirectory, FileSystem.FixInvalidPathCharacters(Asset.GetBestDirectory()));
		string fileName = GetUniqueFileName(Asset, subPath, fileSystem);

		fileSystem.Directory.Create(subPath);

		string filePath = fileSystem.Path.Join(subPath, fileName);
		return ExportInner(filePath, projectDirectory, fileSystem);
	}

	public override bool Contains(IUnityObjectBase asset)
	{
		return Asset.AssetInfo == asset.AssetInfo;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="container"></param>
	/// <param name="filePath">The full path to the exported asset destination</param>
	/// <param name="dirPath">The full path to the project export directory</param>
	/// <returns>True if export was successful, false otherwise</returns>
	protected virtual bool ExportInner(string filePath, string dirPath, FileSystem fileSystem)
	{
		return ContentExtractor.Export(Asset, filePath, fileSystem);
	}

	public override IContentExtractor ContentExtractor { get; }
	public override IEnumerable<IUnityObjectBase> Assets
	{
		get { yield return Asset; }
	}
	public override string Name => Asset.GetBestName();
	public T Asset { get; }
}
