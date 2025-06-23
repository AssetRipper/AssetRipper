using AssetRipper.IO.Files.ResourceFiles;

namespace AssetRipper.IO.Files.CompressedFiles;

public abstract class CompressedFile : FileBase
{
	public FileBase? UncompressedFile { get; set; }

	public override void ReadContents()
	{
		if (UncompressedFile is ResourceFile resourceFile)
		{
			UncompressedFile = SchemeReader.ReadFile(resourceFile);
		}
	}

	public override void ReadContentsRecursively()
	{
		ReadContents();
		UncompressedFile?.ReadContentsRecursively();
	}
}
