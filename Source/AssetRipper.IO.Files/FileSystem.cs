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

	public abstract string TemporaryDirectory { get; set; }

	public partial class FileImplementation
	{
		public string CreateTemporary()
		{
			Directory.Create(Parent.TemporaryDirectory);
			string path = Path.Join(Parent.TemporaryDirectory, GetRandomString());
			File.Create(path).Dispose();
			return path;
		}
	}

	public partial class DirectoryImplementation
	{
		public virtual void Create(string path) => throw new NotSupportedException();

		public virtual void Delete(string path) => throw new NotSupportedException();

		public string CreateTemporary()
		{
			string path = Path.Join(Parent.TemporaryDirectory, GetRandomString()[0..8]);
			Directory.Create(path);
			return path;
		}
	}

	public void DeleteTemporaryDirectory()
	{
		if (Directory.Exists(TemporaryDirectory))
		{
			Directory.Delete(TemporaryDirectory);
		}
	}

	public string GetUniqueName(string dirPath, string fileName, int maxNameLength)
	{
		string? ext = null;
		string? name = null;
		string validFileName = fileName;
		if (Encoding.UTF8.GetByteCount(fileName) > maxNameLength)
		{
			ext = Path.GetExtension(validFileName);
			name = Utf8Truncation.TruncateToUTF8ByteLength(fileName, maxNameLength - Encoding.UTF8.GetByteCount(ext));
			validFileName = name + ext;
		}

		if (!Directory.Exists(dirPath))
		{
			return validFileName;
		}

		name ??= Path.GetFileNameWithoutExtension(validFileName);
		if (!IsReservedName(name))
		{
			if (!File.Exists(Path.Join(dirPath, validFileName)))
			{
				return validFileName;
			}
		}

		ext ??= Path.GetExtension(validFileName);

		string key = Path.Join(dirPath, $"{name}{ext}");
		UniqueNamesByInitialPath.TryGetValue(key, out int initial);

		for (int counter = initial; counter < int.MaxValue; counter++)
		{
			string proposedName = $"{name}_{counter}{ext}";
			if (!File.Exists(Path.Join(dirPath, proposedName)))
			{
				UniqueNamesByInitialPath[key] = counter;
				return proposedName;
			}
		}
		throw new Exception($"Can't generate unique name for file {fileName} in directory {dirPath}");
	}

	private Dictionary<string, int> UniqueNamesByInitialPath { get; } = [];

	public static string RemoveCloneSuffixes(string path)
	{
		return path.Replace("(Clone)", null);
	}

	public static string RemoveInstanceSuffixes(string path)
	{
		return path.Replace("(Instance)", null);
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
		return new Regex($@"[{escapedChars},\[\]\x00-\x1F]", RegexOptions.Compiled);
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

	private static Regex FileNameRegex { get; } = CreateFileNameRegex();

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
		string invalidChars = new string(System.IO.Path.GetInvalidFileNameChars().Except(['\\', '/']).ToArray());
		string escapedChars = Regex.Escape(invalidChars);
		// Updated regex to include commas, square brackets, and ASCII control characters
		return new Regex($@"[{escapedChars},\[\]\x00-\x1F]", RegexOptions.Compiled);
	}

	private static Regex PathRegex { get; } = CreatePathRegex();

	public static bool IsReservedName(string name)
	{
		return OperatingSystem.IsWindows() && name.Length is 3 or 4 && ReservedNames.Contains(name);
	}

	private static HashSet<string> ReservedNames { get; } = new(StringComparer.OrdinalIgnoreCase)
	{
		"aux", "con", "nul", "prn",
		"com1", "com2", "com3", "com4", "com5", "com6", "com7", "com8", "com9",
		"lpt1", "lpt2", "lpt3", "lpt4", "lpt5", "lpt6", "lpt7", "lpt8", "lpt9",
	};

	private protected static string GetRandomString() => Guid.NewGuid().ToString();
}
