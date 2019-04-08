using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace uTinyRipper
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
		
		public static void CreateVirtualDirectory(string path)
		{
#if !VIRTUAL
			CreateDirectory(path);
#endif
		}

		public static void Delete(string path)
		{
			Directory.Delete(ToLongPath(path, true));
		}

		public static void Delete(string path, bool recursive)
		{
			Directory.Delete(ToLongPath(path, true), recursive);
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
			return ToLongPath(path, false);
		}

		private static string ToLongPath(string path, bool force)
		{
			if (path.StartsWith(LongPathPrefix, StringComparison.Ordinal))
			{
				return path;
			}

			string fullPath = Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
			if (force || fullPath.Length >= MaxDirectoryLength)
			{
				return $"{LongPathPrefix}{fullPath}";
			}
			return path;
		}

		public static string GetUniqueName(string dirPath, string fileName)
		{
			dirPath = ToLongPath(dirPath);
			if (!Directory.Exists(dirPath))
			{
				return fileName;
			}

			string filePath = Path.Combine(dirPath, fileName);
			if (!File.Exists(filePath))
			{
				return fileName;
			}

			string name = Path.GetFileNameWithoutExtension(fileName);
			string ext = Path.GetExtension(fileName);
			if (name.Length > 245)
			{
				name = name.Substring(0, 245);
			}

			string escapedName = Regex.Escape(name);
			List<string> files = new List<string>();
			DirectoryInfo dirInfo = new DirectoryInfo(ToLongPath(dirPath));
			Regex regex = new Regex($@"(?i)^{escapedName}(_[\d]+)?\.[^\.]+$");
			foreach (FileInfo fileInfo in dirInfo.EnumerateFiles($"{name}_*{ext}"))
			{
				if (regex.IsMatch(fileInfo.Name))
				{
					files.Add(fileInfo.Name.ToLower());
				}
			}
			if (files.Count == 0)
			{
				return $"{name}_0{ext}";
			}

			string lowName = name.ToLower();
			for (int i = 1; i < int.MaxValue; i++)
			{
				string newName = $"{lowName}_{i}.";
				if (files.All(t => !t.StartsWith(newName, StringComparison.Ordinal)))
				{
					return $"{name}_{i}{ext}";
				}
			}
			throw new Exception($"Can't generate unique name for file {fileName} in directory {dirPath}");
		}

		public const string LongPathPrefix = @"\\?\";
		public const int MaxDirectoryLength = 248;
	}
}
