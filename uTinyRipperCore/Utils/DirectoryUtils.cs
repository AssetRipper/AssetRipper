using System;
using System.IO;

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

		public static string ToLongPath(string path, bool force)
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

		public const string LongPathPrefix = @"\\?\";
		public const int MaxDirectoryLength = 248;
	}
}
