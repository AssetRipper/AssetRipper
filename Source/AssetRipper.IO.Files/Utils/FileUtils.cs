using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace AssetRipper.IO.Files.Utils
{
	public static class FileUtils
	{
		private static readonly Dictionary<string, int> UniqueNamesByInitialPath = new();

		/// <summary>
		/// Reads a file to determine its length
		/// </summary>
		/// <param name="path">The path to the file being investigated</param>
		/// <returns>The number of bytes in the file</returns>
		public static long GetFileSize(string path)
		{
			using FileStream stream = File.OpenRead(path);
			return stream.Length;
		}

		public static string FixInvalidNameCharacters(string path)
		{
			return FileNameRegex.Replace(path, "_");
		}

		public static string RemoveCloneSuffixes(string path)
		{
			return path.Replace("(Clone)", string.Empty);
		}

		public static string RemoveInstanceSuffixes(string path)
		{
			return path.Replace("(Instance)", string.Empty);
		}

		public static string GetUniqueName(string dirPath, string fileName, int maxNameLength)
		{
			string? ext = null;
			string? name = null;
			int maxLength = maxNameLength - 4;
			string validFileName = fileName;
			if (validFileName.Length > maxLength)
			{
				ext = Path.GetExtension(validFileName);
				name = validFileName.Substring(0, maxLength - ext.Length);
				validFileName = name + ext;
			}

			if (!Directory.Exists(dirPath))
			{
				return validFileName;
			}

			name ??= Path.GetFileNameWithoutExtension(validFileName);
			if (!IsReservedName(name))
			{
				if (!File.Exists(Path.Combine(dirPath, validFileName)))
				{
					return validFileName;
				}
			}

			ext ??= Path.GetExtension(validFileName);

			string key = Path.Combine(dirPath, $"{name}{ext}");
			UniqueNamesByInitialPath.TryGetValue(key, out int initial);

			for (int counter = initial; counter < int.MaxValue; counter++)
			{
				string proposedName = $"{name}_{counter}{ext}";
				if (!File.Exists(Path.Combine(dirPath, proposedName)))
				{
					UniqueNamesByInitialPath[key] = counter;
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
			string invalidChars = GetInvalidFileNameChars();
			string escapedChars = Regex.Escape(invalidChars);
			return new Regex($"[{escapedChars}]");
		}

		/// <summary>
		/// Gets all the invalid characters including the colon on Linux
		/// </summary>
		/// <returns></returns>
		private static string GetInvalidFileNameChars()
		{
			char[] defaultBadCharacters = Path.GetInvalidFileNameChars();
			string result = new string(defaultBadCharacters);
			if (defaultBadCharacters.Contains(':'))
			{
				return result;
			}
			else
			{
				return result + ':';
			}
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
