using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				if (RunetimeUtils.IsRunningOnNetCore)
				{
					return path;
				}
				if (path.StartsWith(DirectoryUtils.LongPathPrefix, StringComparison.Ordinal))
				{
					return path;
				}

				string fullPath = GetFullPath(path);
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
			}
			return path;
		}

		public static string FixInvalidNameCharacters(string path)
		{
			return FileNameRegex.Replace(path, string.Empty);
		}

		public static string GetFullPath(string path)
		{
			if (Path.IsPathRooted(path))
			{
				return path.Replace('/', '\\');
			}
			else
			{
				return Path.GetFullPath(path);
			}
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
				name = validFileName.Substring(0, maxLength - ext.Length);
				validFileName = name + ext;
			}

			dirPath = DirectoryUtils.ToLongPath(dirPath, true);
			if (!Directory.Exists(dirPath))
			{
				return validFileName;
			}

			name = name ?? Path.GetFileNameWithoutExtension(validFileName);
			if (!IsReservedName(name))
			{
				if (!File.Exists(Path.Combine(dirPath, validFileName)))
				{
					return validFileName;
				}
			}

			ext = ext ?? Path.GetExtension(validFileName);
			for (int counter = 0; counter < int.MaxValue; counter++)
			{
				string proposedName = $"{name}_{counter}{ext}";
				if (!File.Exists(Path.Combine(dirPath, proposedName)))
				{
					return proposedName;
				}
			}
			throw new Exception($"Can't generate unique name for file {fileName} in directory {dirPath}");
		}

		public static bool IsReservedName(string name)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return ReservedNames.Contains(name.ToLower());
			}
			return false;
		}

		private static Regex GenerateFileNameRegex()
		{
			string invalidChars = new string(Path.GetInvalidFileNameChars());
			string escapedChars = Regex.Escape(invalidChars);
			return new Regex($"[{escapedChars}]");
		}

		public const int MaxFileNameLength = 256;
		public const int MaxFilePathLength = 260;

		private static readonly HashSet<string> ReservedNames = new HashSet<string>()
		{
			"aux", "con", "nul", "prn",
			"com1", "com2", "com3", "com4", "com5", "com6", "com7", "com8", "com9",
			"lpt1", "lpt2", "lpt3", "lpt4", "lpt5", "lpt6", "lpt7", "lpt8", "lpt9",
		};
		private static readonly Regex FileNameRegex = GenerateFileNameRegex();
	}
}
