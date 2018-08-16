using System;
using System.IO;

namespace UtinyRipper
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
			if (path.LastIndexOf(Path.DirectorySeparatorChar) >= DirectoryUtils.MaxDirectoryLength ||
				path.LastIndexOf(Path.AltDirectorySeparatorChar) >= DirectoryUtils.MaxDirectoryLength)
			{
				return $@"{DirectoryUtils.LongPathPrefix}{fullPath}";
			}
			return fullPath;
		}
	}
}
