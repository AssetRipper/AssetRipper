// Auto-generated code. Do not modify manually.

namespace AssetRipper.IO.Files;

public sealed partial class LocalFileSystem : FileSystem
{
	public override LocalFileImplementation File { get; }

	public sealed partial class LocalFileImplementation(LocalFileSystem fileSystem) : FileImplementation(fileSystem)
	{
		public override global::System.IO.FileStream Create(global::System.String path)
		{
			return global::System.IO.File.Create(path);
		}

		public override void Delete(global::System.String path)
		{
			global::System.IO.File.Delete(path);
		}

		public override global::System.Boolean Exists(global::System.String path)
		{
			return global::System.IO.File.Exists(path);
		}

		public override global::System.IO.FileStream OpenRead(global::System.String path)
		{
			return global::System.IO.File.OpenRead(path);
		}

		public override global::System.IO.FileStream OpenWrite(global::System.String path)
		{
			return global::System.IO.File.OpenWrite(path);
		}

		public override global::System.Byte[] ReadAllBytes(global::System.String path)
		{
			return global::System.IO.File.ReadAllBytes(path);
		}

		public override global::System.String ReadAllText(global::System.String path)
		{
			return global::System.IO.File.ReadAllText(path);
		}

		public override global::System.String ReadAllText(global::System.String path, global::System.Text.Encoding encoding)
		{
			return global::System.IO.File.ReadAllText(path, encoding);
		}

		public override void WriteAllBytes(global::System.String path, global::System.ReadOnlySpan<global::System.Byte> bytes)
		{
			global::System.IO.File.WriteAllBytes(path, bytes);
		}

		public override void WriteAllText(global::System.String path, global::System.ReadOnlySpan<global::System.Char> contents)
		{
			global::System.IO.File.WriteAllText(path, contents);
		}

		public override void WriteAllText(global::System.String path, global::System.ReadOnlySpan<global::System.Char> contents, global::System.Text.Encoding encoding)
		{
			global::System.IO.File.WriteAllText(path, contents, encoding);
		}

	}

	public override LocalDirectoryImplementation Directory { get; }

	public sealed partial class LocalDirectoryImplementation(LocalFileSystem fileSystem) : DirectoryImplementation(fileSystem)
	{
		public override global::System.Collections.Generic.IEnumerable<global::System.String> EnumerateDirectories(global::System.String path, global::System.String searchPattern, global::System.IO.SearchOption searchOption)
		{
			return global::System.IO.Directory.EnumerateDirectories(path, searchPattern, searchOption);
		}

		public override global::System.Collections.Generic.IEnumerable<global::System.String> EnumerateFiles(global::System.String path, global::System.String searchPattern, global::System.IO.SearchOption searchOption)
		{
			return global::System.IO.Directory.EnumerateFiles(path, searchPattern, searchOption);
		}

		public override global::System.String[] GetDirectories(global::System.String path, global::System.String searchPattern, global::System.IO.SearchOption searchOption)
		{
			return global::System.IO.Directory.GetDirectories(path, searchPattern, searchOption);
		}

		public override global::System.String[] GetFiles(global::System.String path, global::System.String searchPattern, global::System.IO.SearchOption searchOption)
		{
			return global::System.IO.Directory.GetFiles(path, searchPattern, searchOption);
		}

		public override global::System.Collections.Generic.IEnumerable<global::System.String> EnumerateDirectories(global::System.String path, global::System.String searchPattern)
		{
			return global::System.IO.Directory.EnumerateDirectories(path, searchPattern);
		}

		public override global::System.Collections.Generic.IEnumerable<global::System.String> EnumerateFiles(global::System.String path, global::System.String searchPattern)
		{
			return global::System.IO.Directory.EnumerateFiles(path, searchPattern);
		}

		public override global::System.String[] GetDirectories(global::System.String path, global::System.String searchPattern)
		{
			return global::System.IO.Directory.GetDirectories(path, searchPattern);
		}

		public override global::System.String[] GetFiles(global::System.String path, global::System.String searchPattern)
		{
			return global::System.IO.Directory.GetFiles(path, searchPattern);
		}

		public override global::System.Collections.Generic.IEnumerable<global::System.String> EnumerateDirectories(global::System.String path)
		{
			return global::System.IO.Directory.EnumerateDirectories(path);
		}

		public override global::System.Collections.Generic.IEnumerable<global::System.String> EnumerateFiles(global::System.String path)
		{
			return global::System.IO.Directory.EnumerateFiles(path);
		}

		public override global::System.String[] GetDirectories(global::System.String path)
		{
			return global::System.IO.Directory.GetDirectories(path);
		}

		public override global::System.String[] GetFiles(global::System.String path)
		{
			return global::System.IO.Directory.GetFiles(path);
		}

		public override global::System.Boolean Exists(global::System.String path)
		{
			return global::System.IO.Directory.Exists(path);
		}

	}

	public override LocalPathImplementation Path { get; }

	public sealed partial class LocalPathImplementation(LocalFileSystem fileSystem) : PathImplementation(fileSystem)
	{
		public override global::System.String GetFullPath(global::System.String path)
		{
			return global::System.IO.Path.GetFullPath(path);
		}

		public override global::System.Boolean IsPathRooted(global::System.ReadOnlySpan<global::System.Char> path)
		{
			return global::System.IO.Path.IsPathRooted(path);
		}

	}

	public LocalFileSystem()
	{
		File = new(this);
		Directory = new(this);
		Path = new(this);
	}
}
