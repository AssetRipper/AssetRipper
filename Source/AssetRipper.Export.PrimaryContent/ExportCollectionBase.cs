using AssetRipper.Assets;

namespace AssetRipper.Export.PrimaryContent;

public abstract class ExportCollectionBase
{
	public abstract bool Contains(IUnityObjectBase asset);
	public abstract bool Export(string projectDirectory, FileSystem fileSystem);
	protected void ExportAsset(IUnityObjectBase asset, string path, string name, FileSystem fileSystem)
	{
		if (!fileSystem.Directory.Exists(path))
		{
			fileSystem.Directory.Create(path);
		}

		string fullName = $"{name}.{ExportExtension}";
		string uniqueName = fileSystem.GetUniqueName(path, fullName, FileSystem.MaxFileNameLength);
		string filePath = fileSystem.Path.Join(path, uniqueName);
		ContentExtractor.Export(asset, filePath, fileSystem);
	}

	protected string GetUniqueFileName(IUnityObjectBase asset, string dirPath, FileSystem fileSystem)
	{
		string fileName = asset.GetBestName();
		fileName = FileSystem.RemoveCloneSuffixes(fileName);
		fileName = FileSystem.RemoveInstanceSuffixes(fileName);
		fileName = fileName.Trim();
		if (string.IsNullOrEmpty(fileName))
		{
			fileName = asset.ClassName;
		}
		else
		{
			fileName = FileSystem.FixInvalidFileNameCharacters(fileName);
		}

		fileName = $"{fileName}.{ExportExtension}";
		return GetUniqueFileName(dirPath, fileName, fileSystem);
	}

	protected virtual string ExportExtension => "asset";

	protected static string GetUniqueFileName(string directoryPath, string fileName, FileSystem fileSystem)
	{
		return fileSystem.GetUniqueName(directoryPath, fileName, FileSystem.MaxFileNameLength);
	}

	public abstract IContentExtractor ContentExtractor { get; }
	public abstract IEnumerable<IUnityObjectBase> Assets { get; }
	public virtual IEnumerable<IUnityObjectBase> ExportableAssets => Assets;
	public virtual bool Exportable => true;
	public abstract string Name { get; }
}
