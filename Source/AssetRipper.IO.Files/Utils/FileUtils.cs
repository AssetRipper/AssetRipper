using System.Text;
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
			string validFileName = fileName;
			if (Encoding.UTF8.GetByteCount(fileName) > maxNameLength)
			{
				ext = Path.GetExtension(validFileName);
				name = TruncateToUTF8ByteLength(fileName, maxNameLength - ext.Length);
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
			return OperatingSystem.IsWindows() && ReservedNames.Contains(name.ToLowerInvariant());
		}

		private static Regex GenerateFileNameRegex()
		{
			string invalidChars = GetInvalidFileNameChars();
			string escapedChars = Regex.Escape(invalidChars);
			// Updated regex to include commas, square brackets, and ASCII control characters
			return new Regex($@"[{escapedChars},\[\]\x00-\x1F]");
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

		/// <summary>
		/// <see href="https://en.wikipedia.org/wiki/Comparison_of_file_systems#Limits"/>
		/// </summary>
		public const int MaxFileNameLength = 255;

		private static readonly HashSet<string> ReservedNames =
		[
			"aux", "con", "nul", "prn",
			"com1", "com2", "com3", "com4", "com5", "com6", "com7", "com8", "com9",
			"lpt1", "lpt2", "lpt3", "lpt4", "lpt5", "lpt6", "lpt7", "lpt8", "lpt9",
		];
		private static readonly Regex FileNameRegex = GenerateFileNameRegex();

		private static string TruncateToUTF8ByteLength(string str, int byteLength)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			int validLength = FindValidByteLength(bytes, byteLength);
			return Encoding.UTF8.GetString(bytes[..validLength]);
		}

		private static int FindValidByteLength(byte[] bytes, int byteLength)
		{
			int validByteLength = byteLength;

			// ascii char:      0_
			// two-byte char:   110_   10_
			// three-byte char: 1110_  10_ _10_
			// three-byte char: 11110_ 10_ _10_ _10

			// move to end of the last full sequence
			for (int i = byteLength - 1; i >= 0; i--)
			{
				byte currentByte = bytes[i];

				if ((currentByte & 0b11_000000) == 0b10_000000)
				{
					// continuation byte
					validByteLength--;
				}
				else if ((currentByte & 0b10000000) == 0b10000000)
				{
					// start of multi-byte sequence
					validByteLength--;
					break;
				}
				else
				{
					// ascii char
					break;
				}
			}

			return validByteLength;
		}
	}
}
