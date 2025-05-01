using AssetRipper.Primitives;

namespace AssetRipper.SerializationLogic.Tests;

public class FieldSerializationTests
{
	private class CustomMonoBehaviourWithPrivateFields : UnityEngine.MonoBehaviour
	{
		[UnityEngine.SerializeField]
		private int field1;

		private int field2; // Not serialized
	}

	[Test]
	public void PrivateFieldsAreCorrectlyDiscriminated()
	{
		SerializableType serializableType = CreateSerializableType<CustomMonoBehaviourWithPrivateFields>();
		Assert.That(serializableType.Fields, Has.Count.EqualTo(1));
		SerializableType.Field field = serializableType.Fields[0];
		Assert.That(field.Name, Is.EqualTo("field1"));
	}

	private class CustomMonoBehaviourWithPublicFields : UnityEngine.MonoBehaviour
	{
		[NonSerialized]
		public int field1;

		public int field2;
	}

	[Test]
	public void PublicFieldsAreCorrectlyDiscriminated()
	{
		SerializableType serializableType = CreateSerializableType<CustomMonoBehaviourWithPublicFields>();
		Assert.That(serializableType.Fields, Has.Count.EqualTo(1));
		SerializableType.Field field = serializableType.Fields[0];
		Assert.That(field.Name, Is.EqualTo(nameof(CustomMonoBehaviourWithPublicFields.field2)));
	}

	private class CustomMonoBehaviourWithListField : UnityEngine.MonoBehaviour
	{
		public List<string>? listOfStrings;
	}

	[Test]
	public void ResolutionForUnityTypesWorksAsExpected()
	{
		SerializableType serializableType = CreateSerializableType<CustomMonoBehaviourWithListField>();
		Assert.That(serializableType.Fields, Has.Count.EqualTo(1));
		SerializableType.Field field = serializableType.Fields[0];
		using (Assert.EnterMultipleScope())
		{
			Assert.That(field.Name, Is.EqualTo(nameof(CustomMonoBehaviourWithListField.listOfStrings)));
			Assert.That(field.Type.Type, Is.EqualTo(PrimitiveType.String));
			Assert.That(field.ArrayDepth, Is.EqualTo(1));
		}
	}

	[Serializable]
	private class TestGenericClass<T>
	{
		public List<T>? listOfT;
	}

	private class CustomMonoBehaviourWithGenericField : UnityEngine.MonoBehaviour
	{
		public TestGenericClass<string>? testGenericClass;
	}

	[Test]
	public void DeserializationSupportsGenericTypes()
	{
		SerializableType serializableType = CreateSerializableType<CustomMonoBehaviourWithGenericField>();

		Assert.That(serializableType.Fields, Has.Count.EqualTo(1));
		SerializableType.Field field = serializableType.Fields[0];
		using (Assert.EnterMultipleScope())
		{
			Assert.That(field.Name, Is.EqualTo(nameof(CustomMonoBehaviourWithGenericField.testGenericClass)));
			Assert.That(field.Type.Type, Is.EqualTo(PrimitiveType.Complex));
			Assert.That(field.ArrayDepth, Is.EqualTo(0));
			Assert.That(field.Type.Fields, Has.Count.EqualTo(1));
		}
		SerializableType.Field subField = field.Type.Fields[0];
		using (Assert.EnterMultipleScope())
		{
			Assert.That(subField.Name, Is.EqualTo(nameof(TestGenericClass<>.listOfT)));
			Assert.That(subField.Type.Type, Is.EqualTo(PrimitiveType.String));
			Assert.That(subField.ArrayDepth, Is.EqualTo(1));
		}
	}

	[Serializable]
	private class SelfReferencingClass
	{
		public SelfReferencingClass? selfReference;
	}

	[Test]
	public void CyclicalReferenceClassIsHandled_D1()
	{
		SerializableType serializableType = CreateSerializableType<SelfReferencingClass>();
		Assert.That(serializableType.Fields, Has.Count.EqualTo(0)); // Infinite recursion disqualifies a field from serialization
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
	public void CyclicalReferenceClassIsHandled_D2()
	{
		List<SerializableType> serializableType = CreateSerializableTypes<CyclicalReferenceClass_C1_D2>();
		using (Assert.EnterMultipleScope())
		{
			foreach (SerializableType type in serializableType)
			{
				Assert.That(type.Fields, Has.Count.EqualTo(0), $"{type.Name} should have no fields.");
			}
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
	public void CyclicalReferenceClassIsHandled_D3()
	{
		List<SerializableType> serializableType = CreateSerializableTypes<CyclicalReferenceClass_C1_D3>();
		using (Assert.EnterMultipleScope())
		{
			foreach (SerializableType type in serializableType)
			{
				Assert.That(type.Fields, Has.Count.EqualTo(0), $"{type.Name} should have no fields.");
			}
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
	public void CyclicalReferenceClassIsHandled_D4()
	{
		List<SerializableType> serializableType = CreateSerializableTypes<CyclicalReferenceClass_C1_D4>();
		using (Assert.EnterMultipleScope())
		{
			foreach (SerializableType type in serializableType)
			{
				Assert.That(type.Fields, Has.Count.EqualTo(0), $"{type.Name} should have no fields.");
			}
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
	public void CyclicalReferenceClassIsHandled_D3_V1()
	{
		// Variant 1: Two references to the same class
		List<SerializableType> serializableType = CreateSerializableTypes<CyclicalReferenceClass_C1_D3_V1>();
		using (Assert.EnterMultipleScope())
		{
			foreach (SerializableType type in serializableType)
			{
				Assert.That(type.Fields, Has.Count.EqualTo(0), $"{type.Name} should have no fields.");
			}
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
	public void CyclicalReferenceClassIsHandled_D3_V2()
	{
		// Variant 2: Reference to child class, then reference to parent class
		List<SerializableType> serializableType = CreateSerializableTypes<CyclicalReferenceClass_C1_D3_V2>();
		using (Assert.EnterMultipleScope())
		{
			foreach (SerializableType type in serializableType)
			{
				Assert.That(type.Fields, Has.Count.EqualTo(0), $"{type.Name} should have no fields.");
			}
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
	public void CyclicalReferenceClassIsHandled_D3_V3()
	{
		// Variant 3: Reference to parent class, then reference to child class
		List<SerializableType> serializableType = CreateSerializableTypes<CyclicalReferenceClass_C1_D3_V3>();
		using (Assert.EnterMultipleScope())
		{
			foreach (SerializableType type in serializableType)
			{
				Assert.That(type.Fields, Has.Count.EqualTo(0), $"{type.Name} should have no fields.");
			}
		}
	}

	private static SerializableType CreateSerializableType<T>() => CreateSerializableType<T>(DefaultUnityVersion);

	private static SerializableType CreateSerializableType<T>(UnityVersion version)
	{
		TypeDefinition typeDefinition = ReferenceAssemblies.GetType<T>();
		FieldSerializer serializer = new(version);
		if (serializer.TryCreateSerializableType(typeDefinition, out SerializableType? serializableType, out string? failureReason))
		{
			return serializableType;
		}
		else
		{
			Assert.Fail($"Failed to create serializable type: {failureReason}");
			return default!;
		}
	}

	private static List<SerializableType> CreateSerializableTypes<T>() => CreateSerializableTypes<T>(DefaultUnityVersion);

	private static List<SerializableType> CreateSerializableTypes<T>(UnityVersion version)
	{
		TypeDefinition typeDefinition = ReferenceAssemblies.GetType<T>();
		FieldSerializer serializer = new(version);
		Dictionary<ITypeDefOrRef, SerializableType> typeCache = new();
		if (serializer.TryCreateSerializableType(typeDefinition, typeCache, out SerializableType? serializableType, out string? failureReason))
		{
			return typeCache.Values.ToList();
		}
		else
		{
			Assert.Fail($"Failed to create serializable type: {failureReason}");
			return default!;
		}
	}

	/// <summary>
	/// Assume a recent Unity version if not specified.
	/// </summary>
	private static UnityVersion DefaultUnityVersion => new(6000);
}
