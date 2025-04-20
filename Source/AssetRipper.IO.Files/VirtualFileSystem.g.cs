// Auto-generated code. Do not modify manually.

namespace AssetRipper.IO.Files;

public sealed partial class VirtualFileSystem : FileSystem
{
	public override VirtualFileImplementation File { get; }

	public sealed partial class VirtualFileImplementation(VirtualFileSystem fileSystem) : FileImplementation(fileSystem)
	{
	}

	public override VirtualDirectoryImplementation Directory { get; }

	public sealed partial class VirtualDirectoryImplementation(VirtualFileSystem fileSystem) : DirectoryImplementation(fileSystem)
	{
	}

	public override VirtualPathImplementation Path { get; }

	public sealed partial class VirtualPathImplementation(VirtualFileSystem fileSystem) : PathImplementation(fileSystem)
	{
	}

	public VirtualFileSystem()
	{
		File = new(this);
		Directory = new(this);
		Path = new(this);
	}
}
