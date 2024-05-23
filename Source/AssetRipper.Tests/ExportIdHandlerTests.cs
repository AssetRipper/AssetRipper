using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects;
using Moq;

namespace AssetRipper.Tests;

public class ExportIdHandlerTests
{
	private Mock<IUnityObjectBase> _assetMock;

	[SetUp]
	public void SetUp()
	{
		_assetMock = new Mock<IUnityObjectBase>(MockBehavior.Strict);
	}

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
		_assetMock.SetupGet(a => a.ClassID).Returns(classID);

		// Act & Assert
		Assert.That(() => ExportIdHandler.GetMainExportID(_assetMock.Object, value), Throws.Exception);
	}

	[Test]
	public void GetMainExportID_ValueLessThan100000_ReturnValue()
	{
		// Arrange
		const int classID = 0;
		const uint value = 1u;
		const uint expectedResult = classID * 100000 + value;
		_assetMock.SetupGet(a => a.ClassID).Returns(classID);

		// Act
		long actualResult = ExportIdHandler.GetMainExportID(_assetMock.Object, value);

		// Assert
		Assert.That(actualResult, Is.EqualTo(expectedResult));
	}

	[Test]
	public void GetMainExportID_ClassIDGreaterThan100100_ReturnClassID()
	{
		// Arrange
		const int classID = 100101;
		const uint expectedValue = classID;
		_assetMock.SetupGet(a => a.ClassID).Returns(classID);

		// Act
		long actualResult = ExportIdHandler.GetMainExportID(_assetMock.Object);

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
		_assetMock.SetupGet(a => a.ClassID).Returns(classID);

		// Act
		long actualResult = ExportIdHandler.GetMainExportID(_assetMock.Object, value);

		// Assert
		Assert.That(actualResult, Is.EqualTo(expectedResult));
	}
}
