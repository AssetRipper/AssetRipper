namespace AssetRipper.IO.Files;

public partial class LocalFileSystem : FileSystem
{
	public static LocalFileSystem Instance { get; } = new();

	public partial class LocalFileImplementation
	{
	}

	public partial class LocalDirectoryImplementation
	{
		public override void Create(string path) => System.IO.Directory.CreateDirectory(path);
	}
}
