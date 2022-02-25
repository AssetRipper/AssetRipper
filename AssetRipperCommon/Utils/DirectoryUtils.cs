using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AssetRipper.Core.Utils
{
	public static class DirectoryUtils
	{
		public static string FixInvalidPathCharacters(string path)
		{
			return PathRegex.Replace(path, "_");
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
