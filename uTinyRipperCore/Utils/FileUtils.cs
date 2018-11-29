using System;
using System.IO;

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
			return Open(ToLongPath(path), FileMode.CreateNew, FileAccess.Write);
#endif
		}

		public static void Delete(string path)
		{
			File.Delete(ToLongPath(path));
		}

		public static string ToLongPath(string path)
		{
			if(path.StartsWith(DirectoryUtils.LongPathPrefix, StringComparison.Ordinal))
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
			else if (index >= DirectoryUtils.MaxDirectoryLength)
			{
				// directory name is too long. just append prefix
				return $"{DirectoryUtils.LongPathPrefix}{fullPath}";
			}
			return path;
		}

		public const int MaxFileNameLength = 256;
	}
}
