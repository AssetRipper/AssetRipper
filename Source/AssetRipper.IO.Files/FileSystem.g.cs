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
		/// <inheritdoc cref="global::System.IO.File.Create(global::System.String)"/>
		public virtual global::System.IO.Stream Create(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.File.Delete(global::System.String)"/>
		public virtual void Delete(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.File.Exists(global::System.String)"/>
		public virtual global::System.Boolean Exists(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.File.OpenRead(global::System.String)"/>
		public virtual global::System.IO.Stream OpenRead(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.File.OpenWrite(global::System.String)"/>
		public virtual global::System.IO.Stream OpenWrite(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.File.ReadAllBytes(global::System.String)"/>
		public virtual global::System.Byte[] ReadAllBytes(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.File.ReadAllText(global::System.String)"/>
		public virtual global::System.String ReadAllText(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.File.ReadAllText(global::System.String, global::System.Text.Encoding)"/>
		public virtual global::System.String ReadAllText(global::System.String path, global::System.Text.Encoding encoding)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.File.WriteAllBytes(global::System.String, global::System.ReadOnlySpan<global::System.Byte>)"/>
		public virtual void WriteAllBytes(global::System.String path, global::System.ReadOnlySpan<global::System.Byte> bytes)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.File.WriteAllText(global::System.String, global::System.ReadOnlySpan<global::System.Char>)"/>
		public virtual void WriteAllText(global::System.String path, global::System.ReadOnlySpan<global::System.Char> contents)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.File.WriteAllText(global::System.String, global::System.ReadOnlySpan<global::System.Char>, global::System.Text.Encoding)"/>
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
		/// <inheritdoc cref="global::System.IO.Directory.EnumerateDirectories(global::System.String, global::System.String, global::System.IO.SearchOption)"/>
		public virtual global::System.Collections.Generic.IEnumerable<global::System.String> EnumerateDirectories(global::System.String path, global::System.String searchPattern, global::System.IO.SearchOption searchOption)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.Directory.EnumerateFiles(global::System.String, global::System.String, global::System.IO.SearchOption)"/>
		public virtual global::System.Collections.Generic.IEnumerable<global::System.String> EnumerateFiles(global::System.String path, global::System.String searchPattern, global::System.IO.SearchOption searchOption)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.Directory.GetDirectories(global::System.String, global::System.String, global::System.IO.SearchOption)"/>
		public virtual global::System.String[] GetDirectories(global::System.String path, global::System.String searchPattern, global::System.IO.SearchOption searchOption)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.Directory.GetFiles(global::System.String, global::System.String, global::System.IO.SearchOption)"/>
		public virtual global::System.String[] GetFiles(global::System.String path, global::System.String searchPattern, global::System.IO.SearchOption searchOption)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.Directory.EnumerateDirectories(global::System.String, global::System.String)"/>
		public virtual global::System.Collections.Generic.IEnumerable<global::System.String> EnumerateDirectories(global::System.String path, global::System.String searchPattern)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.Directory.EnumerateFiles(global::System.String, global::System.String)"/>
		public virtual global::System.Collections.Generic.IEnumerable<global::System.String> EnumerateFiles(global::System.String path, global::System.String searchPattern)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.Directory.GetDirectories(global::System.String, global::System.String)"/>
		public virtual global::System.String[] GetDirectories(global::System.String path, global::System.String searchPattern)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.Directory.GetFiles(global::System.String, global::System.String)"/>
		public virtual global::System.String[] GetFiles(global::System.String path, global::System.String searchPattern)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.Directory.EnumerateDirectories(global::System.String)"/>
		public virtual global::System.Collections.Generic.IEnumerable<global::System.String> EnumerateDirectories(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.Directory.EnumerateFiles(global::System.String)"/>
		public virtual global::System.Collections.Generic.IEnumerable<global::System.String> EnumerateFiles(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.Directory.GetDirectories(global::System.String)"/>
		public virtual global::System.String[] GetDirectories(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.Directory.GetFiles(global::System.String)"/>
		public virtual global::System.String[] GetFiles(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.Directory.Exists(global::System.String)"/>
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
		/// <inheritdoc cref="global::System.IO.Path.Join(global::System.String, global::System.String)"/>
		public virtual global::System.String Join(global::System.String path1, global::System.String path2)
		{
			return global::System.IO.Path.Join(path1, path2);
		}

		/// <inheritdoc cref="global::System.IO.Path.Join(global::System.String, global::System.String, global::System.String)"/>
		public virtual global::System.String Join(global::System.String path1, global::System.String path2, global::System.String path3)
		{
			return global::System.IO.Path.Join(path1, path2, path3);
		}

		/// <inheritdoc cref="global::System.IO.Path.Join(global::System.String, global::System.String, global::System.String, global::System.String)"/>
		public virtual global::System.String Join(global::System.String path1, global::System.String path2, global::System.String path3, global::System.String path4)
		{
			return global::System.IO.Path.Join(path1, path2, path3, path4);
		}

		/// <inheritdoc cref="global::System.IO.Path.Join(params global::System.ReadOnlySpan<global::System.String>)"/>
		public virtual global::System.String Join(params global::System.ReadOnlySpan<global::System.String> paths)
		{
			return global::System.IO.Path.Join(paths);
		}

		/// <inheritdoc cref="global::System.IO.Path.GetDirectoryName(global::System.ReadOnlySpan<global::System.Char>)"/>
		public global::System.ReadOnlySpan<global::System.Char> GetDirectoryName(global::System.ReadOnlySpan<global::System.Char> path)
		{
			return global::System.IO.Path.GetDirectoryName(path);
		}

		/// <inheritdoc cref="global::System.IO.Path.GetDirectoryName(global::System.String)"/>
		public global::System.String GetDirectoryName(global::System.String path)
		{
			return global::System.IO.Path.GetDirectoryName(path);
		}

		/// <inheritdoc cref="global::System.IO.Path.GetExtension(global::System.ReadOnlySpan<global::System.Char>)"/>
		public global::System.ReadOnlySpan<global::System.Char> GetExtension(global::System.ReadOnlySpan<global::System.Char> path)
		{
			return global::System.IO.Path.GetExtension(path);
		}

		/// <inheritdoc cref="global::System.IO.Path.GetExtension(global::System.String)"/>
		public global::System.String GetExtension(global::System.String path)
		{
			return global::System.IO.Path.GetExtension(path);
		}

		/// <inheritdoc cref="global::System.IO.Path.GetFileName(global::System.ReadOnlySpan<global::System.Char>)"/>
		public global::System.ReadOnlySpan<global::System.Char> GetFileName(global::System.ReadOnlySpan<global::System.Char> path)
		{
			return global::System.IO.Path.GetFileName(path);
		}

		/// <inheritdoc cref="global::System.IO.Path.GetFileName(global::System.String)"/>
		public global::System.String GetFileName(global::System.String path)
		{
			return global::System.IO.Path.GetFileName(path);
		}

		/// <inheritdoc cref="global::System.IO.Path.GetFileNameWithoutExtension(global::System.ReadOnlySpan<global::System.Char>)"/>
		public global::System.ReadOnlySpan<global::System.Char> GetFileNameWithoutExtension(global::System.ReadOnlySpan<global::System.Char> path)
		{
			return global::System.IO.Path.GetFileNameWithoutExtension(path);
		}

		/// <inheritdoc cref="global::System.IO.Path.GetFileNameWithoutExtension(global::System.String)"/>
		public global::System.String GetFileNameWithoutExtension(global::System.String path)
		{
			return global::System.IO.Path.GetFileNameWithoutExtension(path);
		}

		/// <inheritdoc cref="global::System.IO.Path.GetFullPath(global::System.String)"/>
		public virtual global::System.String GetFullPath(global::System.String path)
		{
			throw new global::System.NotSupportedException();
		}

		/// <inheritdoc cref="global::System.IO.Path.GetRelativePath(global::System.String, global::System.String)"/>
		public global::System.String GetRelativePath(global::System.String relativeTo, global::System.String path)
		{
			return global::System.IO.Path.GetRelativePath(relativeTo, path);
		}

		/// <inheritdoc cref="global::System.IO.Path.IsPathRooted(global::System.ReadOnlySpan<global::System.Char>)"/>
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
