namespace AssetRipper.SerializationLogic.Tests;

public class CyclicalReferenceTests
{
	[Serializable]
	private class SelfReferencingClass
	{
		public SelfReferencingClass? selfReference;
	}

	[Test]
	public async Task CyclicalReferenceClassIsHandled_D1()
	{
		SerializableType serializableType = SerializableTypes.Create<SelfReferencingClass>();
		await Assert.That(serializableType.Fields).Count().IsEqualTo(0); // Infinite recursion disqualifies a field from serialization
	}

	[Serializable]
	private class CyclicalReferenceClass_C1_D2
	{
		public CyclicalReferenceClass_C2_D2? reference;
	}

	[Serializable]
	private class CyclicalReferenceClass_C2_D2
	{
		public CyclicalReferenceClass_C1_D2? reference;
	}

	[Test]
	public async Task CyclicalReferenceClassIsHandled_D2()
	{
		List<SerializableType> serializableType = SerializableTypes.CreateMultiple<CyclicalReferenceClass_C1_D2>();
		foreach (SerializableType type in serializableType)
		{
			await Assert.That(type.Fields).Count().IsEqualTo(0);
		}
	}

	[Serializable]
	private class CyclicalReferenceClass_C1_D3
	{
		public CyclicalReferenceClass_C2_D3? reference;
	}

	[Serializable]
	private class CyclicalReferenceClass_C2_D3
	{
		public CyclicalReferenceClass_C3_D3? reference;
	}

	[Serializable]
	private class CyclicalReferenceClass_C3_D3
	{
		public CyclicalReferenceClass_C1_D3? reference;
	}

	[Test]
	public async Task CyclicalReferenceClassIsHandled_D3()
	{
		List<SerializableType> serializableType = SerializableTypes.CreateMultiple<CyclicalReferenceClass_C1_D3>();
		foreach (SerializableType type in serializableType)
		{
			await Assert.That(type.Fields).Count().IsEqualTo(0);
		}
	}

	[Serializable]
	private class CyclicalReferenceClass_C1_D4
	{
		public CyclicalReferenceClass_C2_D4? reference;
	}

	[Serializable]
	private class CyclicalReferenceClass_C2_D4
	{
		public CyclicalReferenceClass_C3_D4? reference;
	}

	[Serializable]
	private class CyclicalReferenceClass_C3_D4
	{
		public CyclicalReferenceClass_C4_D4? reference;
	}

	[Serializable]
	private class CyclicalReferenceClass_C4_D4
	{
		public CyclicalReferenceClass_C1_D4? reference;
	}

	[Test]
	public async Task CyclicalReferenceClassIsHandled_D4()
	{
		List<SerializableType> serializableType = SerializableTypes.CreateMultiple<CyclicalReferenceClass_C1_D4>();
		foreach (SerializableType type in serializableType)
		{
			await Assert.That(type.Fields).Count().IsEqualTo(0);
		}
	}

	[Serializable]
	private class CyclicalReferenceClass_C1_D3_V1
	{
		public CyclicalReferenceClass_C2_D3_V1? reference;
	}

	[Serializable]
	private class CyclicalReferenceClass_C2_D3_V1
	{
		public CyclicalReferenceClass_C3_D3_V1? reference1;
		public CyclicalReferenceClass_C3_D3_V1? reference2;
	}

	[Serializable]
	private class CyclicalReferenceClass_C3_D3_V1
	{
		public CyclicalReferenceClass_C1_D3_V1? reference;
	}

	[Test]
	public async Task CyclicalReferenceClassIsHandled_D3_V1()
	{
		// Variant 1: Two references to the same class
		List<SerializableType> serializableType = SerializableTypes.CreateMultiple<CyclicalReferenceClass_C1_D3_V1>();
		foreach (SerializableType type in serializableType)
		{
			await Assert.That(type.Fields).Count().IsEqualTo(0);
		}
	}

	[Serializable]
	private class CyclicalReferenceClass_C1_D3_V2
	{
		public CyclicalReferenceClass_C2_D3_V2? reference;
	}

	[Serializable]
	private class CyclicalReferenceClass_C2_D3_V2
	{
		public CyclicalReferenceClass_C3_D3_V2? reference1;
		public CyclicalReferenceClass_C1_D3_V2? reference2;
	}

	[Serializable]
	private class CyclicalReferenceClass_C3_D3_V2
	{
		public CyclicalReferenceClass_C1_D3_V2? reference;
	}

	[Test]
	public async Task CyclicalReferenceClassIsHandled_D3_V2()
	{
		// Variant 2: Reference to child class, then reference to parent class
		List<SerializableType> serializableType = SerializableTypes.CreateMultiple<CyclicalReferenceClass_C1_D3_V2>();
		foreach (SerializableType type in serializableType)
		{
			await Assert.That(type.Fields).Count().IsEqualTo(0);
		}
	}

	[Serializable]
	private class CyclicalReferenceClass_C1_D3_V3
	{
		public CyclicalReferenceClass_C2_D3_V3? reference;
	}

	[Serializable]
	private class CyclicalReferenceClass_C2_D3_V3
	{
		public CyclicalReferenceClass_C1_D3_V3? reference1;
		public CyclicalReferenceClass_C3_D3_V3? reference2;
	}

	[Serializable]
	private class CyclicalReferenceClass_C3_D3_V3
	{
		public CyclicalReferenceClass_C1_D3_V3? reference;
	}

	[Test]
	public async Task CyclicalReferenceClassIsHandled_D3_V3()
	{
		// Variant 3: Reference to parent class, then reference to child class
		List<SerializableType> serializableType = SerializableTypes.CreateMultiple<CyclicalReferenceClass_C1_D3_V3>();
		foreach (SerializableType type in serializableType)
		{
			await Assert.That(type.Fields).Count().IsEqualTo(0);
		}
	}

	[Serializable]
	public class BaseClass<T>
	{
		public string? field1;
		public T? field2;
	}
	[Serializable]
	public class DerivedClass1 : BaseClass<string>
	{
	}
	[Serializable]
	public class DerivedClass2 : BaseClass<DerivedClass2>
	{
	}

	[Test]
	public async Task CyclicalReferenceGenericClassIsHandled()
	{
		SerializableType serializableType1 = SerializableTypes.Create<DerivedClass1>();
		await Assert.That(serializableType1.Fields).Count().IsEqualTo(2); // No cycles, both fields are serialized
		SerializableType serializableType2 = SerializableTypes.Create<DerivedClass2>();
		await Assert.That(serializableType2.Fields).Count().IsEqualTo(1); // One cycle, only field1 is serialized
	}
}
