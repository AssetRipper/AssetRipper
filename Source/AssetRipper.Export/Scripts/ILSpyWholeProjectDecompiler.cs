using AssetRipper.IO.Files;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
using System.Runtime.CompilerServices;

namespace AssetRipper.Export.Scripts;

public class ILSpyWholeProjectDecompiler : WholeProjectDecompiler
{
	protected FileSystem FileSystem { get; }

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public ILSpyWholeProjectDecompiler(DecompilerSettings settings, ILSpyAssemblyResolver assemblyResolver, IProjectFileWriter? projectFileWriter, FileSystem fileSystem)
		: base(settings, assemblyResolver, NullProjectFileWriter.Instance, NullAssemblyReferenceClassifier.Instance, null)
	{
		FileSystem = fileSystem;
	}

	protected override void CreateDirectory(string path)
	{
		try
		{
			FileSystem.Directory.Create(path);
		}
		catch (IOException)
		{
			FileSystem.File.Delete(path);
			FileSystem.Directory.Create(path);
		}
	}

	protected override TextWriter CreateFile(string path)
	{
		Stream stream = FileSystem.File.Create(path);
		return new StreamWriter(stream);
	}
}
