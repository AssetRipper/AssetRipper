using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UtinyRipper
{
	public static class DirectoryUtils
	{
		public static bool Exists(string path)
		{
			return Directory.Exists(ToLongPath(path));
		}

		public static DirectoryInfo CreateDirectory(string path)
		{
			return Directory.CreateDirectory(ToLongPath(path));
		}

		public static void Delete(string path)
		{
			Directory.Delete(ToLongPath(path));
		}

		public static void Delete(string path, bool recursive)
		{
			Directory.Delete(ToLongPath(path), recursive);
		}

		public static string[] GetFiles(string path)
		{
			return Directory.GetFiles(ToLongPath(path));
		}

		public static string[] GetFiles(string path, string searchPattern)
		{
			return Directory.GetFiles(ToLongPath(path), searchPattern);
		}

		public static string[] GetFiles(string path, string searchPattern, SearchOption searchOptions)
		{
			return Directory.GetFiles(ToLongPath(path), searchPattern, searchOptions);
		}

		public static DirectoryInfo GetParent(string path)
		{
			return Directory.GetParent(ToLongPath(path));
		}

		public static string ToLongPath(string path)
		{
			if (path.StartsWith(LongPathPrefix, StringComparison.Ordinal))
			{
				return path;
			}

			string fullPath = Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
			if (fullPath.Length >= MaxDirectoryLength)
			{
				return $@"{LongPathPrefix}{fullPath}";
			}
			return fullPath;
		}

		public static string GetMaxIndexName(string dirPath, string fileName)
		{
			if (!Directory.Exists(dirPath))
			{
				return fileName;
			}

			if (fileName.Length > 245)
			{
				fileName = fileName.Substring(0, 245);
			}
			string escapeFileName = Regex.Escape(fileName);
			Regex regex = new Regex($"(?i){escapeFileName}[_]?[\\d]*\\.[^.]+$");
			List<string> files = new List<string>();
			DirectoryInfo dirInfo = new DirectoryInfo(ToLongPath(dirPath));
			foreach(FileInfo fileInfo in dirInfo.EnumerateFiles())
			{
				if(regex.IsMatch(fileInfo.Name))
				{
					files.Add(fileInfo.Name.ToLower());
				}
			}
			if (files.Count == 0)
			{
				return fileName;
			}

			for (int i = 1; i < int.MaxValue; i++)
			{
				string newName = $"{fileName}_{i}.".ToLower();
				if (files.All(t => !t.StartsWith(newName, StringComparison.Ordinal)))
				{
					return $"{fileName}_{i}";
				}
			}
			throw new Exception($"Can't generate unique name for file {fileName} in directory {dirPath}");
		}

		public const string LongPathPrefix = @"\\?\";
		public const int MaxDirectoryLength = 248;
	}
}
