using AssetRipper.Import.Platforms;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.Tests;

public class MixedGameStructureTests
{
	[Test]
	public void SplitFilesAreCollectedOnce()
	{
		VirtualFileSystem fileSystem = new();
		const string root = "/game";
		fileSystem.Directory.Create(root);

		SerializedFile serializedFile = new SerializedFileBuilder
		{
			Generation = FormatVersion.LargeFilesSupport,
			Version = new(5, 3, 1),
			Platform = BuildTarget.Android,
		}.Build();
		using MemoryStream serializedStream = new();
		serializedFile.Write(serializedStream);
		WriteSplit(fileSystem, $"{root}/globalgamemanagers", serializedStream.ToArray(), 8);

		byte[] bundleData = new byte[64];
		"UnityFS\0"u8.CopyTo(bundleData);
		WriteSplit(fileSystem, $"{root}/archive.bundle", bundleData, 4);

		MixedGameStructure structure = new([root], fileSystem);

		Assert.That(structure.Files, Is.EquivalentTo(new KeyValuePair<string, string>[]
		{
			new("globalgamemanagers", $"{root}/globalgamemanagers"),
			new("archive", $"{root}/archive.bundle"),
		}));
	}

	private static void WriteSplit(VirtualFileSystem fileSystem, string path, byte[] data, int splitPosition)
	{
		fileSystem.File.WriteAllBytes($"{path}.split0", data[..splitPosition]);
		fileSystem.File.WriteAllBytes($"{path}.split1", data[splitPosition..]);
	}
}
