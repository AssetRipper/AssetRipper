namespace AssetRipper.IO.Files.CompressedFiles
{
	public abstract class CompressedFile : File
	{
		public File? UncompressedFile { get; set; }
	}
}
