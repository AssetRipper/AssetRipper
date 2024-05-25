namespace AssetRipper.IO.Files.Utils;

public static class TemporaryFileStorage
{
	private static string ExecutingDirectory => AppContext.BaseDirectory;

	public static string LocalTemporaryDirectory { get; } = Path.Combine(ExecutingDirectory, "temp", GetRandomString()[0..4]);

	private static string temporaryDirectory = LocalTemporaryDirectory;

	public static string TemporaryDirectory
	{
		get => temporaryDirectory;
		set
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				temporaryDirectory = Path.GetFullPath(value);
			}
		}
	}

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
		string path = Path.Combine(TemporaryDirectory, GetRandomString()[0..8]);
		Directory.CreateDirectory(path);
		return path;
	}

	public static string CreateTemporaryFile()
	{
		Directory.CreateDirectory(TemporaryDirectory);
		string path = Path.Combine(TemporaryDirectory, GetRandomString());
		File.Create(path).Dispose();
		return path;
	}
}
