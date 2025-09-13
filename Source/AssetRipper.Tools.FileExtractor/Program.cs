using AssetRipper.IO.Files;
using AssetRipper.IO.Files.CompressedFiles;
using AssetRipper.IO.Files.ResourceFiles;
using FileBase = AssetRipper.IO.Files.FileBase;

namespace AssetRipper.Tools.FileExtractor;

internal class Program
{
	private static readonly string outputDirectory = Path.Join(AppContext.BaseDirectory, "Output");

	static void Main(string[] args)
	{
		Directory.CreateDirectory(outputDirectory);
		if (args.Length == 0)
		{
			Console.WriteLine("No arguments");
		}
		else
		{
			LoadFiles(args);
		}
		Console.WriteLine("Finished!");
		Console.ReadKey();
		return;
	}

	private static void LoadFiles(string[] files)
	{
		foreach (string file in files)
		{
			LoadFile(file);
		}
	}

	private static void LoadFile(string fullName)
	{
		Console.WriteLine(fullName);
		try
		{
			FileBase file = SchemeReader.LoadFile(fullName, LocalFileSystem.Instance);
			SaveContents(file);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}

	private static void SaveContents(FileBase file)
	{
		if (file is FileContainer container && container.ResourceFiles.Count > 0)
		{
			foreach (ResourceFile resourceFile in container.ResourceFiles)
			{
				string path = Path.Join(outputDirectory, resourceFile.NameFixed);
				using FileStream fileStream = File.OpenWrite(path);
				resourceFile.Write(fileStream);
				Console.WriteLine($"\t{path}");
			}
		}
		else if (file is CompressedFile compressedFile && compressedFile.UncompressedFile is ResourceFile uncompressedFile)
		{
			string path = Path.Join(outputDirectory, uncompressedFile.NameFixed);
			using FileStream fileStream = File.OpenWrite(path);
			uncompressedFile.Write(fileStream);
			Console.WriteLine($"\t{path}");
		}
	}
}
