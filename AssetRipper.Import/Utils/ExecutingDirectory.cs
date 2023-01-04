namespace AssetRipper.Import.Utils
{
	public static class ExecutingDirectory
	{
		static ExecutingDirectory()
		{
			Info = new DirectoryInfo(AppContext.BaseDirectory);
		}

		public static DirectoryInfo Info { get; }
		public static string Name => Info.Name;
		public static string Path => Info.FullName;

		public static string Combine(string relativePath) => System.IO.Path.Combine(Path, relativePath);
		public static string Combine(string path1, string path2) => System.IO.Path.Combine(Path, path1, path2);
		public static string Combine(string path1, string path2, string path3) => System.IO.Path.Combine(Path, path1, path2, path3);
		public static string Combine(params string[] parameters) => System.IO.Path.Combine(Path, System.IO.Path.Combine(parameters));
	}
}
