using AssetRipper.Assets;
using AssetRipper.IO.Files.Utils;

namespace AssetRipper.Export.PrimaryContent;

public class SingleExportCollection<T> : ExportCollectionBase where T : IUnityObjectBase
{
	public SingleExportCollection(IContentExtractor contentExtractor, T asset)
	{
		ContentExtractor = contentExtractor ?? throw new ArgumentNullException(nameof(contentExtractor));
		Asset = asset ?? throw new ArgumentNullException(nameof(asset));
	}

	public override bool Export(string projectDirectory)
	{
		string subPath = Asset.OriginalName is not null || Asset.OriginalDirectory is not null
			? Path.Combine(projectDirectory, DirectoryUtils.FixInvalidPathCharacters(Asset.OriginalDirectory ?? ""))
			: Path.Combine(projectDirectory, Asset.ClassName);
		string fileName = GetUniqueFileName(Asset, subPath);

		Directory.CreateDirectory(subPath);

		string filePath = Path.Combine(subPath, fileName);
		return ExportInner(filePath, projectDirectory);
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
	protected virtual bool ExportInner(string filePath, string dirPath)
	{
		return ContentExtractor.Export(Asset, filePath);
	}

	public override IContentExtractor ContentExtractor { get; }
	public override IEnumerable<IUnityObjectBase> Assets
	{
		get { yield return Asset; }
	}
	public override string Name => Asset.GetBestName();
	public T Asset { get; }
}
