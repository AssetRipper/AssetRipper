// Auto-generated code. Do not modify manually.

namespace AssetRipper.IO.Files;

public abstract partial class FileSystem
{
	public abstract FileImplementation File { get; }

	public abstract partial class FileImplementation(FileSystem fileSystem)
	{
		protected FileSystem Parent { get; } = fileSystem;
		protected FileImplementation File => this;
		protected DirectoryImplementation Directory => Parent.Directory;
		protected PathImplementation Path => Parent.Path;
		public virtual global::System.IO.Stream Create(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		public virtual void Delete(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		public virtual global::System.Boolean Exists(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		public virtual global::System.IO.Stream OpenRead(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		public virtual global::System.IO.Stream OpenWrite(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		public virtual global::System.Byte[] ReadAllBytes(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		public virtual global::System.String ReadAllText(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		public virtual global::System.String ReadAllText(global::System.String path, global::System.Text.Encoding encoding)
		{
			throw new global::System.NotSupportedException();
		}

		public virtual void WriteAllBytes(global::System.String path, global::System.ReadOnlySpan<global::System.Byte> bytes)
		{
			throw new global::System.NotSupportedException();
		}

		public virtual void WriteAllText(global::System.String path, global::System.ReadOnlySpan<global::System.Char> contents)
		{
			throw new global::System.NotSupportedException();
		}

		public virtual void WriteAllText(global::System.String path, global::System.ReadOnlySpan<global::System.Char> contents, global::System.Text.Encoding encoding)
		{
			throw new global::System.NotSupportedException();
		}

		// Override methods below to provide custom implementation
		public sealed override string ToString() => base.ToString();
		public sealed override bool Equals(object obj) => base.Equals(obj);
		public sealed override int GetHashCode() => base.GetHashCode();
	}

	public abstract DirectoryImplementation Directory { get; }

	public abstract partial class DirectoryImplementation(FileSystem fileSystem)
	{
		protected FileSystem Parent { get; } = fileSystem;
		protected FileImplementation File => Parent.File;
		protected DirectoryImplementation Directory => this;
		protected PathImplementation Path => Parent.Path;
		public virtual global::System.Collections.Generic.IEnumerable<global::System.String> EnumerateDirectories(global::System.String path, global::System.String searchPattern, global::System.IO.SearchOption searchOption)
		{
			throw new global::System.NotSupportedException();
		}

		public virtual global::System.Collections.Generic.IEnumerable<global::System.String> EnumerateFiles(global::System.String path, global::System.String searchPattern, global::System.IO.SearchOption searchOption)
		{
			throw new global::System.NotSupportedException();
		}

		public virtual global::System.Boolean Exists(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		// Override methods below to provide custom implementation
		public sealed override string ToString() => base.ToString();
		public sealed override bool Equals(object obj) => base.Equals(obj);
		public sealed override int GetHashCode() => base.GetHashCode();
	}

	public abstract PathImplementation Path { get; }

	public abstract partial class PathImplementation(FileSystem fileSystem)
	{
		protected FileSystem Parent { get; } = fileSystem;
		protected FileImplementation File => Parent.File;
		protected DirectoryImplementation Directory => Parent.Directory;
		protected PathImplementation Path => this;
		public virtual global::System.String Join(global::System.String path1, global::System.String path2)
		{
			return global::System.IO.Path.Join(path1, path2);
		}

		public virtual global::System.String Join(global::System.String path1, global::System.String path2, global::System.String path3)
		{
			return global::System.IO.Path.Join(path1, path2, path3);
		}

		public virtual global::System.String Join(global::System.String path1, global::System.String path2, global::System.String path3, global::System.String path4)
		{
			return global::System.IO.Path.Join(path1, path2, path3, path4);
		}

		public virtual global::System.String Join(params global::System.ReadOnlySpan<global::System.String> paths)
		{
			return global::System.IO.Path.Join(paths);
		}

		public global::System.ReadOnlySpan<global::System.Char> GetDirectoryName(global::System.ReadOnlySpan<global::System.Char> path)
		{
			return global::System.IO.Path.GetDirectoryName(path);
		}

		public global::System.String GetDirectoryName(global::System.String path)
		{
			return global::System.IO.Path.GetDirectoryName(path);
		}

		public global::System.ReadOnlySpan<global::System.Char> GetExtension(global::System.ReadOnlySpan<global::System.Char> path)
		{
			return global::System.IO.Path.GetExtension(path);
		}

		public global::System.String GetExtension(global::System.String path)
		{
			return global::System.IO.Path.GetExtension(path);
		}

		public global::System.ReadOnlySpan<global::System.Char> GetFileName(global::System.ReadOnlySpan<global::System.Char> path)
		{
			return global::System.IO.Path.GetFileName(path);
		}

		public global::System.String GetFileName(global::System.String path)
		{
			return global::System.IO.Path.GetFileName(path);
		}

		public global::System.ReadOnlySpan<global::System.Char> GetFileNameWithoutExtension(global::System.ReadOnlySpan<global::System.Char> path)
		{
			return global::System.IO.Path.GetFileNameWithoutExtension(path);
		}

		public global::System.String GetFileNameWithoutExtension(global::System.String path)
		{
			return global::System.IO.Path.GetFileNameWithoutExtension(path);
		}

		public virtual global::System.String GetFullPath(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		public global::System.String GetRelativePath(global::System.String relativeTo, global::System.String path)
		{
			return global::System.IO.Path.GetRelativePath(relativeTo, path);
		}

		public virtual global::System.Boolean IsPathRooted(global::System.ReadOnlySpan<global::System.Char> path)
		{
			throw new global::System.NotSupportedException();
		}

		// Override methods below to provide custom implementation
		public sealed override string ToString() => base.ToString();
		public sealed override bool Equals(object obj) => base.Equals(obj);
		public sealed override int GetHashCode() => base.GetHashCode();
	}

}
