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
		SerializableType serializableType = SerializableTypes.Create<CustomMonoBehaviourWithPrivateFields>();
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
		SerializableType serializableType = SerializableTypes.Create<CustomMonoBehaviourWithPublicFields>();
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
		SerializableType serializableType = SerializableTypes.Create<CustomMonoBehaviourWithListField>();
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
		SerializableType serializableType = SerializableTypes.Create<CustomMonoBehaviourWithGenericField>();

		Assert.That(serializableType.Fields, Has.Count.EqualTo(1));
		SerializableType.Field field = serializableType.Fields[0];
		using (Assert.EnterMultipleScope())
		{
			Assert.That(field.Name, Is.EqualTo(nameof(CustomMonoBehaviourWithGenericField.testGenericClass)));
			Assert.That(field.Type.Type, Is.EqualTo(PrimitiveType.Complex));
			Assert.That(field.ArrayDepth, Is.Zero);
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

	private class CustomMonoBehaviourWithObjectField : UnityEngine.MonoBehaviour
	{
		public UnityEngine.Object? pptr;
	}

	private class CustomMonoBehaviourWithMonoBehaviourField : UnityEngine.MonoBehaviour
	{
		public UnityEngine.Object? pptr;
	}

	private abstract class GenericAbstractMonoBehaviour<T> : UnityEngine.MonoBehaviour
	{
	}

	private class NonGenericDerivedMonoBehaviour : GenericAbstractMonoBehaviour<int>
	{
	}

	private class CustomMonoBehaviourWithNonGenericDerivedMonoBehaviourField : UnityEngine.MonoBehaviour
	{
		public NonGenericDerivedMonoBehaviour? pptr;
	}

	private class CustomMonoBehaviourWithGenericAbstractMonoBehaviourField : UnityEngine.MonoBehaviour
	{
		public GenericAbstractMonoBehaviour<int>? pptr;
	}

	[TestCase(typeof(CustomMonoBehaviourWithObjectField))]
	[TestCase(typeof(CustomMonoBehaviourWithMonoBehaviourField))]
	[TestCase(typeof(CustomMonoBehaviourWithNonGenericDerivedMonoBehaviourField))]
	[TestCase(typeof(CustomMonoBehaviourWithGenericAbstractMonoBehaviourField))]
	public void DeserializationSupportsPPtrFields(Type type)
	{
		SerializableType serializableType = SerializableTypes.Create(type);
		Assert.That(serializableType.Fields, Has.Count.EqualTo(1));
		SerializableType.Field field = serializableType.Fields[0];
		using (Assert.EnterMultipleScope())
		{
			Assert.That(field.Type, Is.EqualTo(SerializablePointerType.Shared));
			Assert.That(field.ArrayDepth, Is.Zero);
		}
	}
}
