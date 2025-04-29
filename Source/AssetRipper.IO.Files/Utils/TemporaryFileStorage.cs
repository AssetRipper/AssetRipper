namespace AssetRipper.IO.Files.Utils;

public static class TemporaryFileStorage
{
	private static string ExecutingDirectory => AppContext.BaseDirectory;

	public static string LocalTemporaryDirectory { get; } = Path.Join(ExecutingDirectory, "temp", GetRandomString()[0..4]);

	public static string TemporaryDirectory
	{
		get;
		set
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				field = Path.GetFullPath(value);
			}
		}
	} = LocalTemporaryDirectory;

	public static void DeleteTemporaryDirectory()
	{
		if (Directory.Exists(TemporaryDirectory))
		{
			Directory.Delete(TemporaryDirectory, true);
		}
	}

	private static string GetRandomString() => Guid.NewGuid().ToString();

	public static string CreateTemporaryFolder()
	{
		string path = Path.Join(TemporaryDirectory, GetRandomString()[0..8]);
		Directory.CreateDirectory(path);
		return path;
	}

	public static string CreateTemporaryFile()
	{
		Directory.CreateDirectory(TemporaryDirectory);
		string path = Path.Join(TemporaryDirectory, GetRandomString());
		File.Create(path).Dispose();
		return path;
	}
}
