using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
using ICSharpCode.Decompiler.Metadata;

namespace AssetRipper.Export.Scripts;

public sealed class NullProjectFileWriter : IProjectFileWriter
{
	public static NullProjectFileWriter Instance { get; } = new();

	void IProjectFileWriter.Write(TextWriter target, IProjectInfoProvider project, IEnumerable<ProjectItemInfo> files, MetadataFile module)
	{
	}
}
