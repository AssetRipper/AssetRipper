using AssetRipper.Assets.Metadata;

namespace AssetRipper.Assets.Tests;

public class PPtrTests
{
	[Test]
	public async Task PPtr_ReturnsCorrectValues()
	{
		// Arrange
		int fileID = 1;
		long pathID = 2;

		// Act
		PPtr pptr = new PPtr(fileID, pathID);

		// Assert

		{
			await Assert.That(pptr.FileID).IsEqualTo(fileID);
			await Assert.That(pptr.PathID).IsEqualTo(pathID);
		}
	}

	[Test]
	public async Task PPtr_ImplicitOperator_ReturnsCorrectType()
	{
		// Arrange
		int fileID = 1;
		long pathID = 2;
		PPtr<IUnityObjectBase> pptr = new PPtr<IUnityObjectBase>(fileID, pathID);

		// Act
		PPtr convertedPptr = pptr;

		// Assert

		{
			await Assert.That(convertedPptr.FileID).IsEqualTo(fileID);
			await Assert.That(convertedPptr.PathID).IsEqualTo(pathID);
		}
	}

	[Test]
	public async Task PPtrT_ReturnsCorrectValues()
	{
		// Arrange
		int fileID = 1;
		long pathID = 2;

		// Act
		PPtr<IDerivedUnityObjectInterface> pptr = new PPtr<IDerivedUnityObjectInterface>(fileID, pathID);

		// Assert

		{
			await Assert.That(pptr.FileID).IsEqualTo(fileID);
			await Assert.That(pptr.PathID).IsEqualTo(pathID);
		}
	}

	[Test]
	public async Task PPtrT_ExplicitOperator_ReturnsCorrectType()
	{
		// Arrange
		int fileID = 1;
		long pathID = 2;
		PPtr<IUnityObjectBase> pptr = new PPtr<IUnityObjectBase>(fileID, pathID);

		// Act
		PPtr<IDerivedUnityObjectInterface> convertedPptr = (PPtr<IDerivedUnityObjectInterface>)pptr;

		// Assert

		{
			await Assert.That(convertedPptr.FileID).IsEqualTo(fileID);
			await Assert.That(convertedPptr.PathID).IsEqualTo(pathID);
		}
	}

	[Test]
	public async Task PPtrT_ImplicitOperator_ReturnsCorrectType()
	{
		// Arrange
		int fileID = 1;
		long pathID = 2;
		PPtr<IDerivedUnityObjectInterface> pptr = new PPtr<IDerivedUnityObjectInterface>(fileID, pathID);

		// Act
		PPtr<IUnityObjectBase> convertedPptr = pptr;

		// Assert

		{
			await Assert.That(convertedPptr.FileID).IsEqualTo(fileID);
			await Assert.That(convertedPptr.PathID).IsEqualTo(pathID);
		}
	}

	[Test]
	public async Task PPtrT_ExplicitOperator_FromIUnityObjectBase_ReturnsCorrectType()
	{
		// Arrange
		int fileID = 1;
		long pathID = 2;
		PPtr<IUnityObjectBase> pptr = new PPtr<IUnityObjectBase>(fileID, pathID);

		// Act
		PPtr<IDerivedUnityObjectInterface> convertedPptr = (PPtr<IDerivedUnityObjectInterface>)pptr;

		// Assert

		{
			await Assert.That(convertedPptr.FileID).IsEqualTo(fileID);
			await Assert.That(convertedPptr.PathID).IsEqualTo(pathID);
		}
	}

	private interface IDerivedUnityObjectInterface : IUnityObjectBase
	{
	}
}