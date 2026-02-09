// Auto-generated code. Do not modify manually.

namespace AssetRipper.IO.Files;

public sealed partial class VirtualFileSystem : FileSystem
{
	public override VirtualFileImplementation File { get; }

	public sealed partial class VirtualFileImplementation(VirtualFileSystem fileSystem) : FileImplementation(fileSystem)
	{
		private new VirtualFileSystem Parent => (VirtualFileSystem)base.Parent;
		private new VirtualFileImplementation File => this;
		private new VirtualDirectoryImplementation Directory => Parent.Directory;
		private new VirtualPathImplementation Path => Parent.Path;
	}

	public override VirtualDirectoryImplementation Directory { get; }

	public sealed partial class VirtualDirectoryImplementation(VirtualFileSystem fileSystem) : DirectoryImplementation(fileSystem)
	{
		private new VirtualFileSystem Parent => (VirtualFileSystem)base.Parent;
		private new VirtualFileImplementation File => Parent.File;
		private new VirtualDirectoryImplementation Directory => this;
		private new VirtualPathImplementation Path => Parent.Path;
	}

	public override VirtualPathImplementation Path { get; }

	public sealed partial class VirtualPathImplementation(VirtualFileSystem fileSystem) : PathImplementation(fileSystem)
	{
		private new VirtualFileSystem Parent => (VirtualFileSystem)base.Parent;
		private new VirtualFileImplementation File => Parent.File;
		private new VirtualDirectoryImplementation Directory => Parent.Directory;
		private new VirtualPathImplementation Path => this;
	}

	public VirtualFileSystem()
	{
		File = new(this);
		Directory = new(this);
		Path = new(this);
	}
}
