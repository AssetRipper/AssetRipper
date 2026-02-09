using AssetRipper.Assets.Metadata;

namespace AssetRipper.Assets.Tests;

public class PPtrTests
{
	[Test]
	public void PPtr_ReturnsCorrectValues()
	{
		// Arrange
		int fileID = 1;
		long pathID = 2;

		// Act
		PPtr pptr = new PPtr(fileID, pathID);

		// Assert
		using (Assert.EnterMultipleScope())
		{
			Assert.That(pptr.FileID, Is.EqualTo(fileID));
			Assert.That(pptr.PathID, Is.EqualTo(pathID));
		}
	}

	[Test]
	public void PPtr_ImplicitOperator_ReturnsCorrectType()
	{
		// Arrange
		int fileID = 1;
		long pathID = 2;
		PPtr<IUnityObjectBase> pptr = new PPtr<IUnityObjectBase>(fileID, pathID);

		// Act
		PPtr convertedPptr = pptr;

		// Assert
		using (Assert.EnterMultipleScope())
		{
			Assert.That(convertedPptr.FileID, Is.EqualTo(fileID));
			Assert.That(convertedPptr.PathID, Is.EqualTo(pathID));
		}
	}

	[Test]
	public void PPtrT_ReturnsCorrectValues()
	{
		// Arrange
		int fileID = 1;
		long pathID = 2;

		// Act
		PPtr<IDerivedUnityObjectInterface> pptr = new PPtr<IDerivedUnityObjectInterface>(fileID, pathID);

		// Assert
		using (Assert.EnterMultipleScope())
		{
			Assert.That(pptr.FileID, Is.EqualTo(fileID));
			Assert.That(pptr.PathID, Is.EqualTo(pathID));
		}
	}

	[Test]
	public void PPtrT_ExplicitOperator_ReturnsCorrectType()
	{
		// Arrange
		int fileID = 1;
		long pathID = 2;
		PPtr<IUnityObjectBase> pptr = new PPtr<IUnityObjectBase>(fileID, pathID);

		// Act
		PPtr<IDerivedUnityObjectInterface> convertedPptr = (PPtr<IDerivedUnityObjectInterface>)pptr;

		// Assert
		using (Assert.EnterMultipleScope())
		{
			Assert.That(convertedPptr.FileID, Is.EqualTo(fileID));
			Assert.That(convertedPptr.PathID, Is.EqualTo(pathID));
		}
	}

	[Test]
	public void PPtrT_ImplicitOperator_ReturnsCorrectType()
	{
		// Arrange
		int fileID = 1;
		long pathID = 2;
		PPtr<IDerivedUnityObjectInterface> pptr = new PPtr<IDerivedUnityObjectInterface>(fileID, pathID);

		// Act
		PPtr<IUnityObjectBase> convertedPptr = pptr;

		// Assert
		using (Assert.EnterMultipleScope())
		{
			Assert.That(convertedPptr.FileID, Is.EqualTo(fileID));
			Assert.That(convertedPptr.PathID, Is.EqualTo(pathID));
		}
	}

	[Test]
	public void PPtrT_ExplicitOperator_FromIUnityObjectBase_ReturnsCorrectType()
	{
		// Arrange
		int fileID = 1;
		long pathID = 2;
		PPtr<IUnityObjectBase> pptr = new PPtr<IUnityObjectBase>(fileID, pathID);

		// Act
		PPtr<IDerivedUnityObjectInterface> convertedPptr = (PPtr<IDerivedUnityObjectInterface>)pptr;

		// Assert
		using (Assert.EnterMultipleScope())
		{
			Assert.That(convertedPptr.FileID, Is.EqualTo(fileID));
			Assert.That(convertedPptr.PathID, Is.EqualTo(pathID));
		}
	}

	private interface IDerivedUnityObjectInterface : IUnityObjectBase
	{
	}
}
