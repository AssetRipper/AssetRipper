using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace AssetRipper.Core.Utils
{
	public static class DirectoryUtils
	{
		public static DirectoryInfo ExecutingDirectoryInfo => new FileInfo(Assembly.GetEntryAssembly().Location).Directory;
		public static string ExecutingDirectoryName => ExecutingDirectoryInfo.Name;
		public static string ExecutingDirectoryPath => ExecutingDirectoryInfo.FullName;

		public static bool Exists(string path)
		{
			return Directory.Exists(ToLongPath(path));
		}

		public static string CombineWithExecutingDirectory(string relativePath)
		{
			return Path.Combine(ExecutingDirectoryPath, relativePath);
		}

		public static string CombineWithExecutingDirectory(params string[] names)
		{
			return Path.Combine(ExecutingDirectoryPath, Path.Combine(names));
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
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				if (RunetimeUtils.IsRunningOnNetCore)
				{
					return path;
				}
				if (path.StartsWith(LongPathPrefix, StringComparison.Ordinal))
				{
					return path;
				}

				string fullPath = FileUtils.GetFullPath(path);
				if (force || fullPath.Length >= MaxDirectoryLength)
				{
					return $"{LongPathPrefix}{fullPath}";
				}
			}
			return path;
		}

		public static string FixInvalidPathCharacters(string path)
		{
			return PathRegex.Replace(path, string.Empty);
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

		public const string LongPathPrefix = @"\\?\";
		public const int MaxDirectoryLength = 248;

		private static readonly Regex PathRegex = GeneratePathRegex();
	}
}
