using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace uTinyRipper
{
	public static class FileUtils
	{
		public static bool Exists(string path)
		{
			return File.Exists(ToLongPath(path));
		}

		public static FileStream Open(string path, FileMode mode)
		{
			return File.Open(ToLongPath(path), mode);
		}

		public static FileStream Open(string path, FileMode mode, FileAccess access)
		{
			return File.Open(ToLongPath(path), mode, access);
		}

		public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
		{
			return File.Open(ToLongPath(path), mode, access, share);
		}

		public static FileStream OpenRead(string path)
		{
			return File.OpenRead(ToLongPath(path));
		}

		public static FileStream OpenWrite(string path)
		{
			return File.OpenWrite(ToLongPath(path));
		}

		public static StreamReader OpenText(string path)
		{
			return File.OpenText(ToLongPath(path));
		}

		public static FileStream Create(string path)
		{
			return File.Create(ToLongPath(path));
		}

		public static FileStream Create(string path, int bufferSize)
		{
			return File.Create(ToLongPath(path), bufferSize);
		}

		public static FileStream Create(string path, int bufferSize, FileOptions options)
		{
			return File.Create(ToLongPath(path), bufferSize, options);
		}

		public static StreamWriter CreateText(string path)
		{
			return File.CreateText(ToLongPath(path));
		}

		public static Stream CreateVirtualFile(string path)
		{
#if VIRTUAL
			return new MemoryStream();
#else
			return Open(path, FileMode.CreateNew, FileAccess.Write);
#endif
		}

		public static void Delete(string path)
		{
			File.Delete(ToLongPath(path));
		}

		public static string ToLongPath(string path)
		{
			if (path.StartsWith(DirectoryUtils.LongPathPrefix, StringComparison.Ordinal))
			{
				return path;
			}

			string fullPath = Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
			int sepIndex = fullPath.LastIndexOf(Path.DirectorySeparatorChar);
			int asepIndex = fullPath.LastIndexOf(Path.AltDirectorySeparatorChar);
			int index = Math.Max(sepIndex, asepIndex);
			if (fullPath.Length - index > MaxFileNameLength)
			{
				// file name is too long. need to shrink
				fullPath = $"{DirectoryUtils.LongPathPrefix}{fullPath}";
				string directory = Path.GetDirectoryName(fullPath);
				string fileName = Path.GetFileNameWithoutExtension(fullPath);
				string extension = Path.GetExtension(fullPath);
				fileName = fileName.Substring(0, MaxFileNameLength - extension.Length - 1);
				return Path.Combine(directory, fileName + extension);
			}
			else if (fullPath.Length >= MaxFilePathLength)
			{
				// name is ok but whole path is too long. just add a prefix
				return $"{DirectoryUtils.LongPathPrefix}{fullPath}";
			}
			return path;
		}

		public static string GetUniqueName(string dirPath, string fileName)
		{
			return GetUniqueName(dirPath, fileName, MaxFileNameLength);
		}

		public static string GetUniqueName(string dirPath, string fileName, int maxNameLength)
		{
			string ext = null;
			string name = null;
			int maxLength = maxNameLength - 4;
			string validFileName = fileName;
			if (validFileName.Length > maxLength)
			{
				ext = Path.GetExtension(validFileName);
				name = Path.GetFileNameWithoutExtension(validFileName).Substring(0, maxLength - ext.Length);
				validFileName = name + ext;
			}

			dirPath = DirectoryUtils.ToLongPath(dirPath);
			if (!Directory.Exists(dirPath))
			{
				return validFileName;
			}

			string filePath = ToLongPath(Path.Combine(dirPath, validFileName));
			if (!File.Exists(filePath))
			{
				return validFileName;
			}

			ext = ext ?? Path.GetExtension(validFileName);
			name = name ?? Path.GetFileNameWithoutExtension(validFileName);

			string escapedName = Regex.Escape(name);
			List<string> files = new List<string>();
			DirectoryInfo dirInfo = new DirectoryInfo(DirectoryUtils.ToLongPath(dirPath, true));
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

		public const int MaxFileNameLength = 256;
		public const int MaxFilePathLength = 260;
	}
}
