using System.Buffers;
using System.Text;
using System.Text.RegularExpressions;

namespace AssetRipper.IO.Files;

public partial class FileSystem
{
	/// <summary>
	/// <see href="https://en.wikipedia.org/wiki/Comparison_of_file_systems#Limits"/>
	/// </summary>
	private const int ActualMaxFileNameLength = 255;
	/// <summary>
	/// We reserve 10 characters for handling file name conflicts, an underscore and up to 9 digits.
	/// This allows us to handle up to 1 billion duplicates, far more than we'll ever need.
	/// </summary>
	private const int ReservedCharacterCount = 10;
	public const int MaxFileNameLength = ActualMaxFileNameLength - ReservedCharacterCount;

	public partial class FileImplementation
	{
	}

	public partial class DirectoryImplementation
	{
		public virtual void Create(string path) => throw new NotSupportedException();
	}

	public static string GetUniqueName(string dirPath, string fileName, int maxNameLength)
	{
		string? ext = null;
		string? name = null;
		string validFileName = fileName;
		if (Encoding.UTF8.GetByteCount(fileName) > maxNameLength)
		{
			ext = System.IO.Path.GetExtension(validFileName);
			name = Utf8Truncation.TruncateToUTF8ByteLength(fileName, maxNameLength - Encoding.UTF8.GetByteCount(ext));
			validFileName = name + ext;
		}

		if (!System.IO.Directory.Exists(dirPath))
		{
			return validFileName;
		}

		name ??= System.IO.Path.GetFileNameWithoutExtension(validFileName);
		if (!IsReservedName(name))
		{
			if (!System.IO.File.Exists(System.IO.Path.Join(dirPath, validFileName)))
			{
				return validFileName;
			}
		}

		ext ??= System.IO.Path.GetExtension(validFileName);

		string key = System.IO.Path.Join(dirPath, $"{name}{ext}");
		UniqueNamesByInitialPath.TryGetValue(key, out int initial);

		for (int counter = initial; counter < int.MaxValue; counter++)
		{
			string proposedName = $"{name}_{counter}{ext}";
			if (!System.IO.File.Exists(System.IO.Path.Join(dirPath, proposedName)))
			{
				UniqueNamesByInitialPath[key] = counter;
				return proposedName;
			}
		}
		throw new Exception($"Can't generate unique name for file {fileName} in directory {dirPath}");
	}

	private static readonly Dictionary<string, int> UniqueNamesByInitialPath = new();

	public static string RemoveCloneSuffixes(string path)
	{
		return path.Replace("(Clone)", string.Empty);
	}

	public static string RemoveInstanceSuffixes(string path)
	{
		return path.Replace("(Instance)", string.Empty);
	}

	public static string FixInvalidFileNameCharacters(string path)
	{
		return FileNameRegex.Replace(path, "_");
	}

	private static Regex CreateFileNameRegex()
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
		char[] defaultBadCharacters = System.IO.Path.GetInvalidFileNameChars();
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

	private static readonly Regex FileNameRegex = CreateFileNameRegex();

	public static string FixInvalidPathCharacters(string path)
	{
		return TrimEntries(PathRegex.Replace(path, "_"));

		static string TrimEntries(string path)
		{
			if (path.Contains(" /", StringComparison.Ordinal) || path.Contains("/ ", StringComparison.Ordinal))
			{
				string[] entries = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				return string.Join('/', entries);
			}
			else
			{
				return path.Trim();
			}
		}
	}

	private static Regex CreatePathRegex()
	{
		string invalidChars = new string(System.IO.Path.GetInvalidFileNameChars().Except(new char[] { '\\', '/' }).ToArray());
		string escapedChars = Regex.Escape(invalidChars);
		// Updated regex to include commas, square brackets, and ASCII control characters
		return new Regex($@"[{escapedChars},\[\]\x00-\x1F]");
	}

	private static readonly Regex PathRegex = CreatePathRegex();

	public static bool IsReservedName(string name)
	{
		return OperatingSystem.IsWindows() && name.Length is 3 or 4 && ReservedNames.Contains(name.ToLowerInvariant());
	}

	private static readonly HashSet<string> ReservedNames =
	[
		"aux", "con", "nul", "prn",
		"com1", "com2", "com3", "com4", "com5", "com6", "com7", "com8", "com9",
		"lpt1", "lpt2", "lpt3", "lpt4", "lpt5", "lpt6", "lpt7", "lpt8", "lpt9",
	];
}
