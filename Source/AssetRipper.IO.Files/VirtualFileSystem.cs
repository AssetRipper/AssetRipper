using AssetRipper.IO.Files.Streams.Smart;
using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace AssetRipper.IO.Files;

public partial class VirtualFileSystem : FileSystem
{
	private readonly DirectoryEntry root = new("", null);

	private sealed class DirectoryEntry
	{
		public string Name { get; }
		public DirectoryEntry? Parent { get; }
		public Dictionary<string, DirectoryEntry> Children { get; } = [];
		public Dictionary<string, FileEntry> Files { get; } = [];

		[MemberNotNullWhen(false, nameof(Parent))]
		public bool IsRoot => Parent is null;

		public string FullName
		{
			get
			{
				if (IsRoot)
				{
					return "/";
				}
				else if (Parent.IsRoot)
				{
					return $"/{Name}";
				}
				else
				{
					return $"{Parent.FullName}/{Name}";
				}
			}
		}

		public int TotalFileCount
		{
			get
			{
				int count = Files.Count;
				foreach (DirectoryEntry child in Children.Values)
				{
					count += child.TotalFileCount;
				}
				return count;
			}
		}

		public int TotalDirectoryCount
		{
			get
			{
				int count = Children.Count;
				foreach (DirectoryEntry child in Children.Values)
				{
					count += child.TotalDirectoryCount;
				}
				return count;
			}
		}

		public DirectoryEntry(string name, DirectoryEntry? parent)
		{
			Name = name;
			Parent = parent;
		}

		public DirectoryEntry CreateDirectory(string name)
		{
			if (!Children.TryGetValue(name, out DirectoryEntry? directory))
			{
				directory = new(name, this);
				Children[name] = directory;
			}
			return directory;
		}

		public DirectoryEntry OpenDirectory(string name)
		{
			return TryOpenDirectory(name) ?? throw new DirectoryNotFoundException($"Directory '{name}' not found.");
		}

		public DirectoryEntry? TryOpenDirectory(string name)
		{
			if (!Children.TryGetValue(name, out DirectoryEntry? directory))
			{
				return null;
			}
			return directory;
		}

		public FileEntry CreateFile(string name)
		{
			if (Files.TryGetValue(name, out FileEntry? file))
			{
				file.Stream.SetLength(0);
			}
			else
			{
				SmartStream stream = SmartStream.CreateMemory();
				file = new(name, this, stream);
				Files[name] = file;
			}

			return file;
		}

		public FileEntry OpenFile(string name)
		{
			return TryOpenFile(name) ?? throw new FileNotFoundException($"File '{name}' not found.");
		}

		public FileEntry? TryOpenFile(string name)
		{
			if (!Files.TryGetValue(name, out FileEntry? file))
			{
				return null;
			}
			return file;
		}
	}

	private sealed class FileEntry
	{
		public string Name { get; }
		public DirectoryEntry Parent { get; }
		public SmartStream Stream { get; }
		public string FullName
		{
			get
			{
				if (Parent.IsRoot)
				{
					return $"/{Name}";
				}
				else
				{
					return $"{Parent.FullName}/{Name}";
				}
			}
		}
		public FileEntry(string name, DirectoryEntry parent, SmartStream stream)
		{
			Name = name;
			Parent = parent;
			Stream = stream;
		}
	}

	/// <summary>
	/// The number of virtual files and directories.
	/// </summary>
	public int Count => root.TotalFileCount + root.TotalDirectoryCount + 1;

	public override string TemporaryDirectory
	{
		get;
		set
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				field = Path.GetFullPath(value);
			}
		}
	} = "/temp";

	/// <summary>
	/// Clears the virtual file system.
	/// </summary>
	public void Clear()
	{
		root.Children.Clear();
		root.Files.Clear();
	}

	private DirectoryEntry OpenDirectory(ReadOnlySpan<string> parts)
	{
		DirectoryEntry current = root;
		foreach (string part in parts)
		{
			current = current.OpenDirectory(part);
		}
		return current;
	}

	private DirectoryEntry? TryOpenDirectory(ReadOnlySpan<string> parts)
	{
		DirectoryEntry? current = root;
		foreach (string part in parts)
		{
			current = current.TryOpenDirectory(part);
			if (current is null)
			{
				return null;
			}
		}
		return current;
	}

	public partial class VirtualFileImplementation
	{
		public override SmartStream Create(string path)
		{
			string[] pathParts = Path.GetPathParts(path);
			if (pathParts.Length == 0)
			{
				throw new ArgumentException("Path cannot be empty.", nameof(path));
			}

			ReadOnlySpan<string> directoryParts = pathParts.AsSpan(0, pathParts.Length - 1);
			string fileName = pathParts[^1];
			DirectoryEntry directory = Parent.OpenDirectory(directoryParts);
			return directory.CreateFile(fileName).Stream.CreateReference();
		}
		public SmartStream Open(string path)
		{
			string[] pathParts = Path.GetPathParts(path);
			if (pathParts.Length == 0)
			{
				throw new ArgumentException("Path cannot be empty.", nameof(path));
			}

			ReadOnlySpan<string> directoryParts = pathParts.AsSpan(0, pathParts.Length - 1);
			string fileName = pathParts[^1];
			DirectoryEntry directory = Parent.OpenDirectory(directoryParts);
			return directory.OpenFile(fileName).Stream.CreateReference();
		}
		public override SmartStream OpenRead(string path) => Open(path);
		public override SmartStream OpenWrite(string path) => Open(path);
		public override void Delete(string path)
		{
			string[] pathParts = Path.GetPathParts(path);
			if (pathParts.Length == 0)
			{
				throw new ArgumentException("Path cannot be empty.", nameof(path));
			}

			ReadOnlySpan<string> directoryParts = pathParts.AsSpan(0, pathParts.Length - 1);
			string fileName = pathParts[^1];
			DirectoryEntry directory = Parent.OpenDirectory(directoryParts);
			if (directory.Files.Remove(fileName, out FileEntry? file))
			{
				file.Stream.Dispose();
			}
		}
		public override bool Exists(string? path)
		{
			string[] pathParts = Path.GetPathParts(path);
			ReadOnlySpan<string> directoryParts = pathParts.AsSpan(0, pathParts.Length - 1);
			string fileName = pathParts[^1];

			return Parent.TryOpenDirectory(directoryParts)?.TryOpenFile(fileName) is not null;
		}
		public override string ReadAllText(string path) => ReadAllText(path, Encoding.UTF8);
		public override string ReadAllText(string path, Encoding encoding) => encoding.GetString(ReadAllBytes(path));
		public override void WriteAllText(string path, ReadOnlySpan<char> contents) => WriteAllText(path, contents, Encoding.UTF8);
		public override void WriteAllText(string path, ReadOnlySpan<char> contents, Encoding encoding)
		{
			int byteCount = encoding.GetByteCount(contents);
			byte[] array = ArrayPool<byte>.Shared.Rent(byteCount);
			Span<byte> span = array.AsSpan(0, byteCount);
			int bytesWritten = encoding.GetBytes(contents, span);
			Debug.Assert(bytesWritten == byteCount);
			WriteAllBytes(path, span);
			ArrayPool<byte>.Shared.Return(array);
		}
		public override byte[] ReadAllBytes(string path)
		{
			using SmartStream stream = Open(path);
			byte[] buffer = new byte[stream.Length];
			stream.Position = 0;
			stream.ReadExactly(buffer);
			return buffer;
		}
		public override void WriteAllBytes(string path, ReadOnlySpan<byte> bytes)
		{
			using SmartStream stream = Create(path);
			stream.SetLength(bytes.Length);
			stream.Position = 0;
			stream.Write(bytes);
		}
	}

	public partial class VirtualDirectoryImplementation
	{
		public override void Create(string? path)
		{
			string[] parts = Path.GetPathParts(path);
			DirectoryEntry current = Parent.root;
			foreach (string part in parts)
			{
				current = current.CreateDirectory(part);
			}
		}

		public override IEnumerable<string> EnumerateDirectories(string? path, string searchPattern, SearchOption searchOption)
		{
			if (searchPattern != "*")
			{
				throw new NotImplementedException("Only '*' search pattern is supported.");
			}

			string[] parts = Path.GetPathParts(path);
			DirectoryEntry directory = Parent.OpenDirectory(parts);
			if (searchOption == SearchOption.TopDirectoryOnly)
			{
				foreach (DirectoryEntry child in directory.Children.Values)
				{
					yield return child.FullName;
				}
			}
			else
			{
				Stack<DirectoryEntry> stack = new();
				stack.Push(directory);
				while (stack.Count > 0)
				{
					DirectoryEntry current = stack.Pop();
					foreach (DirectoryEntry child in current.Children.Values)
					{
						yield return child.FullName;
						stack.Push(child);
					}
				}
			}
		}

		public override IEnumerable<string> EnumerateFiles(string? path, string searchPattern, SearchOption searchOption)
		{
			string[] pathParts = Path.GetPathParts(path);

			string extension;
			if (searchPattern == "*")
			{
				extension = string.Empty;
			}
			else if (searchPattern.StartsWith("*.") && searchPattern.Length > 2)
			{
				extension = searchPattern[1..];
				if (extension.Contains('*') || extension.Contains('?'))
				{
					throw new NotImplementedException("Only '*' and '*.<extension>' search patterns are supported.");
				}
			}
			else
			{
				throw new NotImplementedException("Only '*' and '*.<extension>' search patterns are supported.");
			}

			DirectoryEntry directory = Parent.OpenDirectory(pathParts);
			if (searchOption == SearchOption.TopDirectoryOnly)
			{
				foreach (FileEntry file in directory.Files.Values)
				{
					if (file.Name.EndsWith(extension, StringComparison.Ordinal))
					{
						yield return file.FullName;
					}
				}
			}
			else
			{
				Stack<DirectoryEntry> stack = new();
				stack.Push(directory);
				while (stack.Count > 0)
				{
					DirectoryEntry current = stack.Pop();
					foreach (FileEntry file in current.Files.Values)
					{
						if (file.Name.EndsWith(extension, StringComparison.Ordinal))
						{
							yield return file.FullName;
						}
					}
					foreach (DirectoryEntry child in current.Children.Values)
					{
						stack.Push(child);
					}
				}
			}
		}

		public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
		{
			return EnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
		}

		public override IEnumerable<string> EnumerateDirectories(string path)
		{
			return EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly);
		}

		public override IEnumerable<string> EnumerateFiles(string path, string searchPattern)
		{
			return EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
		}

		public override IEnumerable<string> EnumerateFiles(string path)
		{
			return EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly);
		}

		public override string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
		{
			return EnumerateDirectories(path, searchPattern, searchOption).ToArray();
		}

		public override string[] GetDirectories(string path, string searchPattern)
		{
			return EnumerateDirectories(path, searchPattern).ToArray();
		}

		public override string[] GetDirectories(string path)
		{
			return EnumerateDirectories(path).ToArray();
		}

		public override string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
		{
			return EnumerateFiles(path, searchPattern, searchOption).ToArray();
		}

		public override string[] GetFiles(string path, string searchPattern)
		{
			return EnumerateFiles(path, searchPattern).ToArray();
		}

		public override string[] GetFiles(string path)
		{
			return EnumerateFiles(path).ToArray();
		}

		public override bool Exists(string? path)
		{
			string[] parts = Path.GetPathParts(path);
			DirectoryEntry current = Parent.root;
			foreach (string part in parts)
			{
				if (!current.Children.TryGetValue(part, out DirectoryEntry? next))
				{
					return false;
				}
				current = next;
			}
			return true;
		}
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

		internal string[] GetPathParts(string? path)
		{
			string fullPath = GetFullPath(path);
			return fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
