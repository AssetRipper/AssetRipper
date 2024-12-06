
using AssetRipper.IO.Files.Streams.Smart;
using System.Text;

namespace AssetRipper.IO.Files;

public partial class VirtualFileSystem : FileSystem
{
	private readonly HashSet<string> directories = ["/"];
	private readonly Dictionary<string, SmartStream> files = new();

	/// <summary>
	/// The number of virtual files and directories.
	/// </summary>
	public int Count => files.Count + directories.Count;

	/// <summary>
	/// Clears the virtual file system.
	/// </summary>
	public void Clear()
	{
		directories.Clear();
		files.Clear();
	}

	private string GetFullDirectoryName(string path)
	{
		string directory = Path.GetDirectoryName(path);
		return Path.GetFullPath(directory);
	}

	public partial class VirtualFileImplementation
	{
		public override Stream Create(string path)
		{
			string directory = fileSystem.GetFullDirectoryName(path);
			if (!fileSystem.directories.Contains(directory))
			{
				throw new DirectoryNotFoundException($"Directory '{directory}' not found.");
			}
			if (fileSystem.files.ContainsKey(path))
			{
				throw new IOException($"File '{path}' already exists.");
			}
			SmartStream stream = SmartStream.CreateMemory();
			fileSystem.files.Add(path, stream);
			return stream.CreateReference();
		}
		public override bool Exists(string path) => fileSystem.files.ContainsKey(path);
		public override string ReadAllText(string path) => ReadAllText(path, Encoding.UTF8);
		public override void WriteAllText(string path, ReadOnlySpan<char> contents) => WriteAllText(path, contents, Encoding.UTF8);
	}

	public partial class VirtualDirectoryImplementation
	{
		public override void Create(string path)
		{
			string fullPath = GetFullPath(path);
			while (fileSystem.directories.Add(fullPath))
			{
				int index = fullPath.LastIndexOf('/');
				if (index > 0)
				{
					fullPath = fullPath[..index];
				}
				else
				{
					break;
				}
			}
		}

		public override bool Exists(string? path) => fileSystem.directories.Contains(GetFullPath(path));

		private string GetFullPath(string? path) => Path.GetFullPath(path);
	}

	public partial class VirtualPathImplementation
	{
		public override string GetFullPath(string? path)
		{
			// The "current directory" is always the root directory.
			// We also don't support ".." and ".".

			if (path is null or "" or "/" or "\\")
			{
				return "/";
			}

			string normalizedPath = path.Replace('\\', '/');
			if (normalizedPath[^1] is '/')
			{
				normalizedPath = normalizedPath[..^1];
			}
			return normalizedPath[0] is '/' ? normalizedPath : $"/{normalizedPath}";
		}

		public override bool IsPathRooted(ReadOnlySpan<char> path)
		{
			return path.Length > 0 && path[0] is '/' or '\\';
		}
	}
}
