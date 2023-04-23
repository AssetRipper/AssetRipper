using System.Text.RegularExpressions;

namespace AssetRipper.IO.Files.Utils
{
	public static class DirectoryUtils
	{
		public static string FixInvalidPathCharacters(string path)
		{
			return PathRegex.Replace(path, "_").TrimEntries();
		}

		private static string TrimEntries(this string path)
		{
			if (path.Contains(" /", StringComparison.Ordinal) || path.Contains("/ ", StringComparison.Ordinal))
			{
				string[] entries = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				return string.Join('/', entries);
			}
			else
			{
				return path.Trim();
			}
		}

		private static Regex GeneratePathRegex()
		{
			string invalidChars = new string(Path.GetInvalidFileNameChars().Except(new char[] { '\\', '/' }).ToArray());
			string escapedChars = Regex.Escape(invalidChars);
			return new Regex($"[{escapedChars}]");
		}

		public static string GetRelativePath(string filePath, string folder)
		{
			Uri pathUri = new Uri(filePath);
			if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				folder += Path.DirectorySeparatorChar;
			}
			Uri folderUri = new Uri(folder);
			return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
		}

		private static readonly Regex PathRegex = GeneratePathRegex();
	}
}
