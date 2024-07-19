using AssetRipper.Assets;
using AssetRipper.IO.Files.Utils;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.PrimaryContent;

public abstract class ExportCollectionBase
{
	public abstract bool Contains(IUnityObjectBase asset);
	public abstract bool Export(string projectDirectory);
	protected void ExportAsset(IUnityObjectBase asset, string path, string name)
	{
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		string fullName = $"{name}.{ExportExtension}";
		string uniqueName = FileUtils.GetUniqueName(path, fullName, FileUtils.MaxFileNameLength);
		string filePath = Path.Combine(path, uniqueName);
		ContentExtractor.Export(asset, filePath);
	}

	protected string GetUniqueFileName(IUnityObjectBase asset, string dirPath)
	{
		string fileName = asset switch
		{
			IPrefabInstance prefab => prefab.GetName(),
			IShader shader when !string.IsNullOrEmpty(shader.OriginalName) => shader.OriginalName,
			_ => asset.GetBestName(),
		};
		fileName = FileUtils.RemoveCloneSuffixes(fileName);
		fileName = FileUtils.RemoveInstanceSuffixes(fileName);
		fileName = fileName.Trim();
		if (string.IsNullOrEmpty(fileName))
		{
			fileName = asset.ClassName;
		}
		else
		{
			fileName = FileUtils.FixInvalidNameCharacters(fileName);
		}

		fileName = $"{fileName}.{ExportExtension}";
		return GetUniqueFileName(dirPath, fileName);
	}

	protected virtual string ExportExtension => "asset";

	protected static string GetUniqueFileName(string directoryPath, string fileName)
	{
		return FileUtils.GetUniqueName(directoryPath, fileName, FileUtils.MaxFileNameLength);
	}

	public abstract IContentExtractor ContentExtractor { get; }
	public abstract IEnumerable<IUnityObjectBase> Assets { get; }
	public virtual IEnumerable<IUnityObjectBase> ExportableAssets => Assets;
	public virtual bool Exportable => true;
	public abstract string Name { get; }
}
