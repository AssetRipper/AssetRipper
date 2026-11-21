namespace AssetRipper.IO.Files.Tests;

public class FileSystemTests
{
	[Test]
	[Arguments("Assets/Aux", "Assets/Aux_")]
	[Arguments("Assets/Aux/CON.txt/file", "Assets/Aux_/CON_.txt/file")]
	[Arguments("Assets/CON.txt/file", "Assets/CON_.txt/file")]
	[Arguments("Assets/ Aux .txt/file", "Assets/Aux_.txt/file")]
	[Arguments(@"Assets\NUL\file", @"Assets\NUL_\file")]
	[Arguments("Assets/normal", "Assets/normal")]
	public async Task FixInvalidPathCharactersRenamesReservedPathSegments(string path, string expected)
	{
		await Assert.That(FileSystem.FixInvalidPathCharacters(path)).IsEqualTo(expected);
	}

	[Test]
	[Arguments("Aux", "Aux_")]
	[Arguments("COM1.cs", "COM1_.cs")]
	[Arguments("CON.g.txt", "CON_.g.txt")]
	[Arguments(" Aux .txt", "Aux_.txt")]
	[Arguments("normal", "normal")]
	public async Task FixInvalidFileNameCharactersRenamesReservedNames(string name, string expected)
	{
		await Assert.That(FileSystem.FixInvalidFileNameCharacters(name)).IsEqualTo(expected);
	}
}
