using AssetRipper.IO.Files.Streams.Smart;
using System.Buffers;
using System.Diagnostics;
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

	public partial class VirtualFileImplementation
	{
		public override SmartStream Create(string path)
		{
			string directory = fileSystem.GetFullDirectoryName(path);
			string fullPath = Path.GetFullPath(path);
			if (!fileSystem.directories.Contains(directory))
			{
				throw new DirectoryNotFoundException($"Directory '{directory}' not found.");
			}
			if (!fileSystem.files.TryGetValue(fullPath, out SmartStream? stream))
			{
				stream = SmartStream.CreateMemory();
				fileSystem.files.Add(fullPath, stream);
			}
			else
			{
				stream.SetLength(0);
			}
			return stream.CreateReference();
		}
		public SmartStream Open(string path)
		{
			string directory = fileSystem.GetFullDirectoryName(path);
			string fullPath = Path.GetFullPath(path);
			if (!fileSystem.directories.Contains(directory))
			{
				throw new DirectoryNotFoundException($"Directory '{directory}' not found.");
			}
			if (!fileSystem.files.TryGetValue(fullPath, out SmartStream? stream))
			{
				throw new FileNotFoundException($"File '{path}' not found.");
			}
			return stream.CreateReference();
		}
		public override SmartStream OpenRead(string path) => Open(path);
		public override SmartStream OpenWrite(string path) => Open(path);
		public override void Delete(string path)
		{
			string directory = fileSystem.GetFullDirectoryName(path);
			string fullPath = Path.GetFullPath(path);
			if (!fileSystem.directories.Contains(directory))
			{
				throw new DirectoryNotFoundException($"Directory '{directory}' not found.");
			}
			if (fileSystem.files.Remove(fullPath, out SmartStream? stream))
			{
				stream.Dispose();
			}
		}
		public override bool Exists(string path) => fileSystem.files.ContainsKey(path);
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

		public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
		{
			throw new NotImplementedException();
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
