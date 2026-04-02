using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects;
using AssetRipper.Primitives;

namespace AssetRipper.Tests;

public class ExportIdHandlerTests
{
	[Test]
	public void GetMainExportID_ValueGreaterThan100100AndNonZero_ExceptionThrown()
	{
		// Arrange
		const int classID = 100500;
		const uint value = 1u;

		// Act & Assert
		Assert.Throws<ArgumentException>(() => ExportIdHandler.GetMainExportID(classID, value));
	}

	[Test]
	public void GetMainExportID_ValueGreaterThan100000_DebugAssertFails()
	{
		// Arrange
		const int classID = 0;
		const uint value = 100001u;

		// Act & Assert
		Assert.That(() => ExportIdHandler.GetMainExportID(MockObject.Create(classID), value), Throws.Exception);
	}

	[Test]
	public void GetMainExportID_ValueLessThan100000_ReturnValue()
	{
		// Arrange
		const int classID = 0;
		const uint value = 1u;
		const uint expectedResult = classID * 100000 + value;

		// Act
		long actualResult = ExportIdHandler.GetMainExportID(MockObject.Create(classID), value);

		// Assert
		Assert.That(actualResult, Is.EqualTo(expectedResult));
	}

	[Test]
	public void GetMainExportID_ClassIDGreaterThan100100_ReturnClassID()
	{
		// Arrange
		const int classID = 100101;
		const uint expectedValue = classID;

		// Act
		long actualResult = ExportIdHandler.GetMainExportID(MockObject.Create(classID));

		// Assert
		Assert.That(actualResult, Is.EqualTo(expectedValue));
	}

	[Test]
	public void GetMainExportID_ClassIDLessThan100101_ReturnCompositeID()
	{
		// Arrange
		const int classID = 42;
		const uint value = 12345u;
		const uint expectedResult = classID * 100000 + value;

		// Act
		long actualResult = ExportIdHandler.GetMainExportID(MockObject.Create(classID), value);

		// Assert
		Assert.That(actualResult, Is.EqualTo(expectedResult));
	}

	[Test]
	public void GetPseudoRandomValue_NoConflictsInFirstMillionValues()
	{
		const int Count = 1_000_000;
		HashSet<long> generatedIds = new(Count);
		for (int i = 0; i < Count; i++)
		{
			long exportId = ExportIdHandler.GetPseudoRandomValue64(i);
			Assert.That(generatedIds.Add(exportId), Is.True, $"Duplicate export ID generated: {exportId}");
		}
	}

	[Test]
	public void GetPseudoRandomExportID_NoConflictsInFirstMillionValues()
	{
		const int classID = 42;
		MockObject asset = MockObject.Create(classID);

		const int Count = 1_000_000;
		HashSet<long> generatedIds = new(Count);
		for (int i = 0; i < Count; i++)
		{
			long exportId = ExportIdHandler.GetPseudoRandomExportId(asset, i);
			Assert.That(generatedIds.Add(exportId), Is.True, $"Duplicate export ID generated: {exportId}");
		}
	}

	private sealed class MockObject : UnityObjectBase
	{
		public MockObject(AssetInfo assetInfo) : base(assetInfo)
		{
		}

		public static MockObject Create(int classID, UnityVersion version)
		{
			GameBundle gameBundle = new();
			ProcessedAssetCollection collection = gameBundle.AddNewProcessedCollection("TestCollection", version);
			return collection.CreateAsset(classID, assetInfo => new MockObject(assetInfo));
		}

		public static MockObject Create(int classID) => Create(classID, new UnityVersion(6000));
	}
}
