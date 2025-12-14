using AsmResolver.PE.DotNet.Metadata.Tables;
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

	[TestCase("2019.4", false)]
	[TestCase("2020.1.0a3", true)]
	public void GenericInstanceSerializationStartedWithUnity2020(string version, bool serialize)
	{
		// https://github.com/AssetRipper/AssetRipper/issues/1792
		SerializableType serializableType = SerializableTypes.Create<CustomMonoBehaviourWithGenericField>(UnityVersion.Parse(version));
		if (serialize)
		{
			Assert.That(serializableType.Fields, Has.Count.EqualTo(1));
			SerializableType.Field field = serializableType.Fields[0];
			using (Assert.EnterMultipleScope())
			{
				Assert.That(field.Name, Is.EqualTo(nameof(CustomMonoBehaviourWithGenericField.testGenericClass)));
				Assert.That(field.Type.Type, Is.EqualTo(PrimitiveType.Complex));
				Assert.That(field.ArrayDepth, Is.Zero);
			}
		}
		else
		{
			Assert.That(serializableType.Fields, Has.Count.EqualTo(0));
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

	[Serializable]
	private class ClassWithLongField
	{
		public long value;
	}

	[TestCase("4.4", false)]
	[TestCase("5.3", false)]
	[TestCase("5.9", false)]
	[TestCase("2017.1", true)]
	public void LongIntegerSerializationStartedWithUnity2017(string version, bool serialize)
	{
		// https://github.com/AssetRipper/AssetRipper/issues/647
		SerializableType serializableType = SerializableTypes.Create<ClassWithLongField>(UnityVersion.Parse(version));
		if (serialize)
		{
			Assert.That(serializableType.Fields, Has.Count.EqualTo(1));
			SerializableType.Field field = serializableType.Fields[0];
			using (Assert.EnterMultipleScope())
			{
				Assert.That(field.Name, Is.EqualTo(nameof(ClassWithLongField.value)));
				Assert.That(field.Type.Type, Is.EqualTo(PrimitiveType.Long));
				Assert.That(field.ArrayDepth, Is.Zero);
			}
		}
		else
		{
			Assert.That(serializableType.Fields, Has.Count.EqualTo(0));
		}
	}

	[Serializable]
	private struct StructWithIntField
	{
		public int value;
	}

	[Test]
	public void DeserializationSupportsStructsWithIntField()
	{
		SerializableType serializableType = SerializableTypes.Create<StructWithIntField>();
		Assert.That(serializableType.Fields, Has.Count.EqualTo(1));
		SerializableType.Field field = serializableType.Fields[0];
		using (Assert.EnterMultipleScope())
		{
			Assert.That(field.Name, Is.EqualTo(nameof(StructWithIntField.value)));
			Assert.That(field.Type.Type, Is.EqualTo(PrimitiveType.Int));
			Assert.That(field.ArrayDepth, Is.Zero);
		}
	}

	[Serializable]
	private unsafe struct StructWithFixedSizeBuffer
	{
		public fixed int values[4];
	}

	[Test]
	public void DeserializationSupportsStructsWithFixedSizeBuffer()
	{
		SerializableType serializableType = SerializableTypes.Create<StructWithFixedSizeBuffer>();
		Assert.That(serializableType.Fields, Has.Count.EqualTo(1));
		SerializableType.Field field = serializableType.Fields[0];
		using (Assert.EnterMultipleScope())
		{
			Assert.That(field.Name, Is.EqualTo(nameof(StructWithFixedSizeBuffer.values)));
			Assert.That(field.Type.Type, Is.EqualTo(PrimitiveType.Int));
			Assert.That(field.ArrayDepth, Is.EqualTo(1));
		}
	}

	[Serializable]
	private class ClassWithStructField
	{
		public StructWithIntField value;
	}

	[TestCase("4.4", false)]
	[TestCase("4.6", true)]
	public void StructSerializationStartedWithUnity4_5(string version, bool serialize)
	{
		// https://github.com/AssetRipper/AssetRipper/issues/1534
		SerializableType serializableType = SerializableTypes.Create<ClassWithStructField>(UnityVersion.Parse(version));
		if (serialize)
		{
			Assert.That(serializableType.Fields, Has.Count.EqualTo(1));
			SerializableType.Field field = serializableType.Fields[0];
			using (Assert.EnterMultipleScope())
			{
				Assert.That(field.Name, Is.EqualTo(nameof(ClassWithStructField.value)));
				Assert.That(field.Type.Type, Is.EqualTo(PrimitiveType.Complex));
				Assert.That(field.ArrayDepth, Is.Zero);
			}
		}
		else
		{
			Assert.That(serializableType.Fields, Has.Count.EqualTo(0));
		}
	}

	[Serializable]
	private class DerivedClassWithNewField : ClassWithLongField
	{
		public new float value;
	}

	[Test]
	public void DeserializationSupportsFieldsWithSameName()
	{
		SerializableType serializableType = SerializableTypes.Create<DerivedClassWithNewField>();
		Assert.That(serializableType.Fields, Has.Count.EqualTo(2));
		SerializableType.Field field1 = serializableType.Fields[0];
		SerializableType.Field field2 = serializableType.Fields[1];
		using (Assert.EnterMultipleScope())
		{
			Assert.That(field1.Name, Is.EqualTo(field2.Name));

			Assert.That(field1.Name, Is.EqualTo(nameof(ClassWithLongField.value)));
			Assert.That(field1.Type.Type, Is.EqualTo(PrimitiveType.Long));
			Assert.That(field1.ArrayDepth, Is.Zero);

			Assert.That(field2.Name, Is.EqualTo(nameof(DerivedClassWithNewField.value)));
			Assert.That(field2.Type.Type, Is.EqualTo(PrimitiveType.Single));
			Assert.That(field2.ArrayDepth, Is.Zero);
		}
	}

	[TestCase(typeof(UnityEngine.Vector2))]
	[TestCase(typeof(UnityEngine.Vector3))]
	[TestCase(typeof(UnityEngine.Vector4))]
	[TestCase(typeof(UnityEngine.Rect))]
	[TestCase(typeof(UnityEngine.RectInt))]
	[TestCase(typeof(UnityEngine.Quaternion))]
	[TestCase(typeof(UnityEngine.Matrix4x4))]
	[TestCase(typeof(UnityEngine.Color))]
	[TestCase(typeof(UnityEngine.Color32))]
	[TestCase(typeof(UnityEngine.LayerMask))]
	[TestCase(typeof(UnityEngine.Bounds))]
	[TestCase(typeof(UnityEngine.BoundsInt))]
	[TestCase(typeof(UnityEngine.Vector3Int))]
	[TestCase(typeof(UnityEngine.Vector2Int))]
	public void DeserializationSupportsEngineStructsWithoutSerializableAttributeOnUnity4(Type type)
	{
		TypeDefinition typeDefinition = SerializableClassGenerator.CreateEmptySerializableClass();
		FieldDefinition fieldDefinition = new("engineStructField", FieldAttributes.Public, new FieldSignature(ReferenceAssemblies.GetType(type).ToTypeSignature()));
		typeDefinition.Fields.Add(fieldDefinition);

		SerializableType serializableType = SerializableTypes.Create(typeDefinition, new(4));
		Assert.That(serializableType.Fields, Has.Count.EqualTo(1));
	}

	[Serializable]
	private class ClassWithVolatileField
	{
		public volatile bool value;
	}

	[Test]
	public void DeserializationSupportsVolatileFields()
	{
		SerializableType serializableType = SerializableTypes.Create<ClassWithVolatileField>();
		Assert.That(serializableType.Fields, Has.Count.EqualTo(1));
		SerializableType.Field field = serializableType.Fields[0];
		using (Assert.EnterMultipleScope())
		{
			Assert.That(field.Name, Is.EqualTo(nameof(ClassWithVolatileField.value)));
			Assert.That(field.Type.Type, Is.EqualTo(PrimitiveType.Bool));
			Assert.That(field.ArrayDepth, Is.Zero);
		}
	}

	[Serializable]
	private abstract class AbstractSerializableClass
	{
		public int value;
	}

	[Serializable]
	private class ClassWithAbstractField
	{
		public AbstractSerializableClass? value;
	}

	[Test]
	public void FieldsWithAbstractTypesShouldNotBeSerialized()
	{
		SerializableType serializableType = SerializableTypes.Create<ClassWithAbstractField>();
		Assert.That(serializableType.Fields, Has.Count.EqualTo(0));
	}

	[Serializable]
	private class ClassWithAbstractListField
	{
		public AbstractSerializableClass? value;
	}

	[Test]
	public void ListFieldsWithAbstractTypesShouldNotBeSerialized()
	{
		SerializableType serializableType = SerializableTypes.Create<ClassWithAbstractListField>();
		Assert.That(serializableType.Fields, Has.Count.EqualTo(0));
	}
}
