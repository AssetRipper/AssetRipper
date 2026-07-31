using AssetRipper.Import.Structure;
using AssetRipper.IO.Files;

namespace AssetRipper.Tests;

public class ZipExtractorTests
{
	[Test]
	public void ExtractionPathsMustRemainWithinDestination()
	{
		string destination = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "AssetRipper", "extract"));
		string child = Path.Combine(destination, "Data", "file.assets");
		string sibling = destination + "-other";

		using (Assert.EnterMultipleScope())
		{
			Assert.That(ZipExtractor.IsPathWithinDirectory(child, destination, LocalFileSystem.Instance), Is.True);
			Assert.That(ZipExtractor.IsPathWithinDirectory(destination, destination, LocalFileSystem.Instance), Is.True);
			Assert.That(ZipExtractor.IsPathWithinDirectory(sibling, destination, LocalFileSystem.Instance), Is.False);
			Assert.That(ZipExtractor.IsPathWithinDirectory(Path.Combine(destination, "..", "file.assets"), destination, LocalFileSystem.Instance), Is.False);
		}
	}
}
