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
	public async Task GetMainExportID_ValueGreaterThan100100AndNonZero_ExceptionThrown()
	{
		// Arrange
		const int classID = 100500;
		const uint value = 1u;

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentException>(() => Task.Run(() => ExportIdHandler.GetMainExportID(classID, value)));
	}

	[Test]
	public async Task GetMainExportID_ValueGreaterThan100000_DebugAssertFails()
	{
		// Arrange
		const int classID = 0;
		const uint value = 100001u;

		// Act & Assert
		await Assert.ThrowsAsync<Exception>(() => Task.Run(() => ExportIdHandler.GetMainExportID(MockObject.Create(classID), value)));
	}

	[Test]
	public async Task GetMainExportID_ValueLessThan100000_ReturnValue()
	{
		// Arrange
		const int classID = 0;
		const uint value = 1u;
		const uint expectedResult = classID * 100000 + value;

		// Act
		long actualResult = ExportIdHandler.GetMainExportID(MockObject.Create(classID), value);

		// Assert
		await Assert.That(actualResult).IsEqualTo(expectedResult);
	}

	[Test]
	public async Task GetMainExportID_ClassIDGreaterThan100100_ReturnClassID()
	{
		// Arrange
		const int classID = 100101;
		const uint expectedValue = classID;

		// Act
		long actualResult = ExportIdHandler.GetMainExportID(MockObject.Create(classID));

		// Assert
		await Assert.That(actualResult).IsEqualTo(expectedValue);
	}

	[Test]
	public async Task GetMainExportID_ClassIDLessThan100101_ReturnCompositeID()
	{
		// Arrange
		const int classID = 42;
		const uint value = 12345u;
		const uint expectedResult = classID * 100000 + value;

		// Act
		long actualResult = ExportIdHandler.GetMainExportID(MockObject.Create(classID), value);

		// Assert
		await Assert.That(actualResult).IsEqualTo(expectedResult);
	}

	[Test]
	public async Task GetPseudoRandomValue_NoConflictsInFirstMillionValues()
	{
		const int Count = 1_000_000;
		HashSet<long> generatedIds = new(Count);
		for (int i = 0; i < Count; i++)
		{
			long exportId = ExportIdHandler.GetPseudoRandomValue64(i);
			await Assert.That(generatedIds.Add(exportId)).IsTrue().Because($"Duplicate export ID generated: {exportId}");
		}
	}

	[Test]
	public async Task GetPseudoRandomExportID_NoConflictsInFirstMillionValues()
	{
		const int classID = 42;
		MockObject asset = MockObject.Create(classID);

		const int Count = 1_000_000;
		HashSet<long> generatedIds = new(Count);
		for (int i = 0; i < Count; i++)
		{
			long exportId = ExportIdHandler.GetPseudoRandomExportId(asset, i);
			await Assert.That(generatedIds.Add(exportId)).IsTrue().Because($"Duplicate export ID generated: {exportId}");
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