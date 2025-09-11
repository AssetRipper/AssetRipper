using AssetRipper.IO.Files.Streams.Smart;
using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace AssetRipper.IO.Files;

public partial class VirtualFileSystem : FileSystem
{
	private readonly DirectoryEntity root = new("", null);

	private sealed class DirectoryEntity
	{
		public string Name { get; }
		public DirectoryEntity? Parent { get; }
		public Dictionary<string, DirectoryEntity> Children { get; } = [];
		public Dictionary<string, FileEntity> Files { get; } = [];

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
				foreach (DirectoryEntity child in Children.Values)
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
				foreach (DirectoryEntity child in Children.Values)
				{
					count += child.TotalDirectoryCount;
				}
				return count;
			}
		}

		public DirectoryEntity(string name, DirectoryEntity? parent)
		{
			Name = name;
			Parent = parent;
		}

		public DirectoryEntity CreateDirectory(string name)
		{
			if (!Children.TryGetValue(name, out DirectoryEntity? directory))
			{
				directory = new(name, this);
				Children[name] = directory;
			}
			return directory;
		}

		public DirectoryEntity OpenDirectory(string name)
		{
			return TryOpenDirectory(name) ?? throw new DirectoryNotFoundException($"Directory '{name}' not found.");
		}

		public DirectoryEntity? TryOpenDirectory(string name)
		{
			if (!Children.TryGetValue(name, out DirectoryEntity? directory))
			{
				return null;
			}
			return directory;
		}

		public FileEntity CreateFile(string name)
		{
			if (Files.TryGetValue(name, out FileEntity? file))
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

		public FileEntity OpenFile(string name)
		{
			return TryOpenFile(name) ?? throw new FileNotFoundException($"File '{name}' not found.");
		}

		public FileEntity? TryOpenFile(string name)
		{
			if (!Files.TryGetValue(name, out FileEntity? file))
			{
				return null;
			}
			return file;
		}
	}

	private sealed class FileEntity
	{
		public string Name { get; }
		public DirectoryEntity Parent { get; }
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
		public FileEntity(string name, DirectoryEntity parent, SmartStream stream)
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

	private DirectoryEntity OpenDirectory(ReadOnlySpan<string> parts)
	{
		DirectoryEntity current = root;
		foreach (string part in parts)
		{
			current = current.OpenDirectory(part);
		}
		return current;
	}

	private DirectoryEntity? TryOpenDirectory(ReadOnlySpan<string> parts)
	{
		DirectoryEntity? current = root;
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
			DirectoryEntity directoryEntity = Parent.OpenDirectory(directoryParts);
			return directoryEntity.CreateFile(fileName).Stream.CreateReference();
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
			DirectoryEntity directoryEntity = Parent.OpenDirectory(directoryParts);
			return directoryEntity.OpenFile(fileName).Stream.CreateReference();
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
			DirectoryEntity directory = Parent.OpenDirectory(directoryParts);
			if (directory.Files.Remove(fileName, out FileEntity? file))
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
		public override void Create(string path)
		{
			string[] parts = Path.GetPathParts(path);
			DirectoryEntity current = Parent.root;
			foreach (string part in parts)
			{
				current = current.CreateDirectory(part);
			}
		}

		public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
		{
			throw new NotImplementedException();
		}

		public override bool Exists(string? path)
		{
			string[] parts = Path.GetPathParts(path);
			DirectoryEntity current = Parent.root;
			foreach (string part in parts)
			{
				if (!current.Children.TryGetValue(part, out DirectoryEntity? next))
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
