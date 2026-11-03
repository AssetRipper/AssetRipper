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
	public async Task PrivateFieldsAreCorrectlyDiscriminated()
	{
		SerializableType serializableType = SerializableTypes.Create<CustomMonoBehaviourWithPrivateFields>();
		await Assert.That(serializableType.Fields.Count).IsEqualTo(1);
		SerializableType.Field field = serializableType.Fields[0];
		await Assert.That(field.Name).IsEqualTo("field1");
	}

	private class CustomMonoBehaviourWithPublicFields : UnityEngine.MonoBehaviour
	{
		[NonSerialized]
		public int field1;

		public int field2;
	}

	[Test]
	public async Task PublicFieldsAreCorrectlyDiscriminated()
	{
		SerializableType serializableType = SerializableTypes.Create<CustomMonoBehaviourWithPublicFields>();
		await Assert.That(serializableType.Fields.Count).IsEqualTo(1);
		SerializableType.Field field = serializableType.Fields[0];
		await Assert.That(field.Name).IsEqualTo(nameof(CustomMonoBehaviourWithPublicFields.field2));
	}

	private class CustomMonoBehaviourWithListField : UnityEngine.MonoBehaviour
	{
		public List<string>? listOfStrings;
	}

	[Test]
	public async Task ResolutionForUnityTypesWorksAsExpected()
	{
		SerializableType serializableType = SerializableTypes.Create<CustomMonoBehaviourWithListField>();
		await Assert.That(serializableType.Fields.Count).IsEqualTo(1);
		SerializableType.Field field = serializableType.Fields[0];
		{
			await Assert.That(field.Name).IsEqualTo(nameof(CustomMonoBehaviourWithListField.listOfStrings));
			await Assert.That(field.Type.Type).IsEqualTo(PrimitiveType.String);
			await Assert.That(field.ArrayDepth).IsEqualTo(1);
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
	public async Task DeserializationSupportsGenericTypes()
	{
		SerializableType serializableType = SerializableTypes.Create<CustomMonoBehaviourWithGenericField>();

		await Assert.That(serializableType.Fields.Count).IsEqualTo(1);
		SerializableType.Field field = serializableType.Fields[0];
		{
			await Assert.That(field.Name).IsEqualTo(nameof(CustomMonoBehaviourWithGenericField.testGenericClass));
			await Assert.That(field.Type.Type).IsEqualTo(PrimitiveType.Complex);
			await Assert.That(field.ArrayDepth).IsEqualTo(0);
			await Assert.That(field.Type.Fields.Count).IsEqualTo(1);
		}
		SerializableType.Field subField = field.Type.Fields[0];
		{
			await Assert.That(subField.Name).IsEqualTo(nameof(TestGenericClass<>.listOfT));
			await Assert.That(subField.Type.Type).IsEqualTo(PrimitiveType.String);
			await Assert.That(subField.ArrayDepth).IsEqualTo(1);
		}
	}

	[Test]
	[Arguments("2019.4", false)]
	[Arguments("2020.1.0a3", true)]
	public async Task GenericInstanceSerializationStartedWithUnity2020(string version, bool serialize)
	{
		// https://github.com/AssetRipper/AssetRipper/issues/1792
		SerializableType serializableType = SerializableTypes.Create<CustomMonoBehaviourWithGenericField>(UnityVersion.Parse(version));
		if (serialize)
		{
			await Assert.That(serializableType.Fields.Count).IsEqualTo(1);
			SerializableType.Field field = serializableType.Fields[0];
			{
				await Assert.That(field.Name).IsEqualTo(nameof(CustomMonoBehaviourWithGenericField.testGenericClass));
				await Assert.That(field.Type.Type).IsEqualTo(PrimitiveType.Complex);
				await Assert.That(field.ArrayDepth).IsEqualTo(0);
			}
		}
		else
		{
			await Assert.That(serializableType.Fields.Count).IsEqualTo(0);
		}
	}

	internal class DerivedObject : UnityEngine.Object
	{
	}

	internal class CustomMonoBehaviourWithObjectField : UnityEngine.MonoBehaviour
	{
		public UnityEngine.Object? pptr;
	}

	internal class CustomMonoBehaviourWithDerivedObjectField : UnityEngine.MonoBehaviour
	{
		public DerivedObject? pptr;
	}

	internal class CustomMonoBehaviourWithMonoBehaviourField : UnityEngine.MonoBehaviour
	{
		public UnityEngine.Object? pptr;
	}

	internal abstract class GenericAbstractMonoBehaviour<T> : UnityEngine.MonoBehaviour
	{
	}

	internal class NonGenericDerivedMonoBehaviour : GenericAbstractMonoBehaviour<int>
	{
	}

	internal class CustomMonoBehaviourWithNonGenericDerivedMonoBehaviourField : UnityEngine.MonoBehaviour
	{
		public NonGenericDerivedMonoBehaviour? pptr;
	}

	internal class CustomMonoBehaviourWithGenericAbstractMonoBehaviourField : UnityEngine.MonoBehaviour
	{
		public GenericAbstractMonoBehaviour<int>? pptr;
	}

	[Test]
	[Arguments(typeof(CustomMonoBehaviourWithObjectField))]
	[Arguments(typeof(CustomMonoBehaviourWithDerivedObjectField))]
	[Arguments(typeof(CustomMonoBehaviourWithMonoBehaviourField))]
	[Arguments(typeof(CustomMonoBehaviourWithNonGenericDerivedMonoBehaviourField))]
	[Arguments(typeof(CustomMonoBehaviourWithGenericAbstractMonoBehaviourField))]
	public async Task DeserializationSupportsPPtrFields(Type type)
	{
		SerializableType serializableType = SerializableTypes.Create(type);
		await Assert.That(serializableType.Fields.Count).IsEqualTo(1);
		SerializableType.Field field = serializableType.Fields[0];
		{
			await Assert.That(field.Type).IsEqualTo(SerializablePointerType.Shared);
			await Assert.That(field.ArrayDepth).IsEqualTo(0);
		}
	}

	[Serializable]
	private class ClassWithLongField
	{
		public long value;
	}

	[Test]
	[Arguments("4.4", false)]
	[Arguments("5.3", false)]
	[Arguments("5.9", false)]
	[Arguments("2017.1", true)]
	public async Task LongIntegerSerializationStartedWithUnity2017(string version, bool serialize)
	{
		// https://github.com/AssetRipper/AssetRipper/issues/647
		SerializableType serializableType = SerializableTypes.Create<ClassWithLongField>(UnityVersion.Parse(version));
		if (serialize)
		{
			await Assert.That(serializableType.Fields.Count).IsEqualTo(1);
			SerializableType.Field field = serializableType.Fields[0];
			{
				await Assert.That(field.Name).IsEqualTo(nameof(ClassWithLongField.value));
				await Assert.That(field.Type.Type).IsEqualTo(PrimitiveType.Long);
				await Assert.That(field.ArrayDepth).IsEqualTo(0);
			}
		}
		else
		{
			await Assert.That(serializableType.Fields.Count).IsEqualTo(0);
		}
	}

	[Serializable]
	private struct StructWithIntField
	{
		public int value;
	}

	[Test]
	public async Task DeserializationSupportsStructsWithIntField()
	{
		SerializableType serializableType = SerializableTypes.Create<StructWithIntField>();
		await Assert.That(serializableType.Fields.Count).IsEqualTo(1);
		SerializableType.Field field = serializableType.Fields[0];
		{
			await Assert.That(field.Name).IsEqualTo(nameof(StructWithIntField.value));
			await Assert.That(field.Type.Type).IsEqualTo(PrimitiveType.Int);
			await Assert.That(field.ArrayDepth).IsEqualTo(0);
		}
	}

	[Serializable]
	private unsafe struct StructWithFixedSizeBuffer
	{
		public fixed int values[4];
	}

	[Test]
	public async Task DeserializationSupportsStructsWithFixedSizeBuffer()
	{
		SerializableType serializableType = SerializableTypes.Create<StructWithFixedSizeBuffer>();
		await Assert.That(serializableType.Fields.Count).IsEqualTo(1);
		SerializableType.Field field = serializableType.Fields[0];
		{
			await Assert.That(field.Name).IsEqualTo(nameof(StructWithFixedSizeBuffer.values));
			await Assert.That(field.Type.Type).IsEqualTo(PrimitiveType.Int);
			await Assert.That(field.ArrayDepth).IsEqualTo(1);
		}
	}

	[Serializable]
	private class ClassWithStructField
	{
		public StructWithIntField value;
	}

	[Test]
	[Arguments("4.4", false)]
	[Arguments("4.6", true)]
	public async Task StructSerializationStartedWithUnity4_5(string version, bool serialize)
	{
		// https://github.com/AssetRipper/AssetRipper/issues/1534
		SerializableType serializableType = SerializableTypes.Create<ClassWithStructField>(UnityVersion.Parse(version));
		if (serialize)
		{
			await Assert.That(serializableType.Fields.Count).IsEqualTo(1);
			SerializableType.Field field = serializableType.Fields[0];
			{
				await Assert.That(field.Name).IsEqualTo(nameof(ClassWithStructField.value));
				await Assert.That(field.Type.Type).IsEqualTo(PrimitiveType.Complex);
				await Assert.That(field.ArrayDepth).IsEqualTo(0);
			}
		}
		else
		{
			await Assert.That(serializableType.Fields.Count).IsEqualTo(0);
		}
	}

	[Serializable]
	private class DerivedClassWithNewField : ClassWithLongField
	{
		public new float value;
	}

	[Test]
	public async Task DeserializationSupportsFieldsWithSameName()
	{
		SerializableType serializableType = SerializableTypes.Create<DerivedClassWithNewField>();
		await Assert.That(serializableType.Fields.Count).IsEqualTo(2);
		SerializableType.Field field1 = serializableType.Fields[0];
		SerializableType.Field field2 = serializableType.Fields[1];
		{
			await Assert.That(field1.Name).IsEqualTo(field2.Name);

			await Assert.That(field1.Name).IsEqualTo(nameof(ClassWithLongField.value));
			await Assert.That(field1.Type.Type).IsEqualTo(PrimitiveType.Long);
			await Assert.That(field1.ArrayDepth).IsEqualTo(0);

			await Assert.That(field2.Name).IsEqualTo(nameof(DerivedClassWithNewField.value));
			await Assert.That(field2.Type.Type).IsEqualTo(PrimitiveType.Single);
			await Assert.That(field2.ArrayDepth).IsEqualTo(0);
		}
	}

	[Test]
	[Arguments(typeof(UnityEngine.Vector2))]
	[Arguments(typeof(UnityEngine.Vector3))]
	[Arguments(typeof(UnityEngine.Vector4))]
	[Arguments(typeof(UnityEngine.Rect))]
	[Arguments(typeof(UnityEngine.RectInt))]
	[Arguments(typeof(UnityEngine.Quaternion))]
	[Arguments(typeof(UnityEngine.Matrix4x4))]
	[Arguments(typeof(UnityEngine.Color))]
	[Arguments(typeof(UnityEngine.Color32))]
	[Arguments(typeof(UnityEngine.LayerMask))]
	[Arguments(typeof(UnityEngine.Bounds))]
	[Arguments(typeof(UnityEngine.BoundsInt))]
	[Arguments(typeof(UnityEngine.Vector3Int))]
	[Arguments(typeof(UnityEngine.Vector2Int))]
	public async Task DeserializationSupportsEngineStructsWithoutSerializableAttributeOnUnity4(Type type)
	{
		TypeDefinition typeDefinition = SerializableClassGenerator.CreateEmptySerializableClass();
		FieldDefinition fieldDefinition = new("engineStructField", FieldAttributes.Public, new FieldSignature(ReferenceAssemblies.GetType(type).ToTypeSignature()));
		typeDefinition.Fields.Add(fieldDefinition);

		SerializableType serializableType = SerializableTypes.Create(typeDefinition, new(4));
		await Assert.That(serializableType.Fields.Count).IsEqualTo(1);
	}

	[Serializable]
	private class ClassWithVolatileField
	{
		public volatile bool value;
	}

	[Test]
	public async Task DeserializationSupportsVolatileFields()
	{
		SerializableType serializableType = SerializableTypes.Create<ClassWithVolatileField>();
		await Assert.That(serializableType.Fields.Count).IsEqualTo(1);
		SerializableType.Field field = serializableType.Fields[0];
		{
			await Assert.That(field.Name).IsEqualTo(nameof(ClassWithVolatileField.value));
			await Assert.That(field.Type.Type).IsEqualTo(PrimitiveType.Bool);
			await Assert.That(field.ArrayDepth).IsEqualTo(0);
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
	public async Task FieldsWithAbstractTypesShouldNotBeSerialized()
	{
		SerializableType serializableType = SerializableTypes.Create<ClassWithAbstractField>();
		await Assert.That(serializableType.Fields.Count).IsEqualTo(0);
	}

	[Serializable]
	private class ClassWithAbstractListField
	{
		public AbstractSerializableClass? value;
	}

	[Test]
	public async Task ListFieldsWithAbstractTypesShouldNotBeSerialized()
	{
		SerializableType serializableType = SerializableTypes.Create<ClassWithAbstractListField>();
		await Assert.That(serializableType.Fields.Count).IsEqualTo(0);
	}

	[Serializable]
	private class BaseClassWithSelfField
	{
		public int baseField;
		public BaseClassWithSelfField? nonSerializedField;
	}

	[Serializable]
	private class DerivedClassWithBaseClassField : BaseClassWithSelfField
	{
		public BaseClassWithSelfField? serializedField;
	}

	[Test]
	public async Task FieldWhoseTypeIsBaseShouldBeSerialized()
	{
		SerializableType serializableType = SerializableTypes.Create<DerivedClassWithBaseClassField>();
		await Assert.That(serializableType.Fields.Count).IsEqualTo(2);
	}

	private class BaseClassWithoutSerializableAttribute
	{
		public int baseField;
	}

	[Serializable]
	private class DerivedClassWithSerializableAttribute : BaseClassWithoutSerializableAttribute
	{
		public int derivedField;
	}

	[Test]
	public async Task FieldsFromBaseTypesAreStillSerializedEvenWithoutSerializableAttributeOnBaseType()
	{
		SerializableType serializableType = SerializableTypes.Create<DerivedClassWithSerializableAttribute>();
		await Assert.That(serializableType.Fields.Count).IsEqualTo(2);
	}

	private interface IReferencedInterface
	{
	}

	[Serializable]
	private class ClassWithReferenceField
	{
		[UnityEngine.SerializeReference]
		public IReferencedInterface? reference;
	}

	private class CustomMonoBehaviourWithReferenceFields : UnityEngine.MonoBehaviour
	{
		[UnityEngine.SerializeReference]
		public IReferencedInterface? single;

		[UnityEngine.SerializeReference]
		public List<IReferencedInterface>? list;

		[UnityEngine.SerializeReference]
		public IReferencedInterface[]? array;
	}

	private class DerivedMonoBehaviourWithReferenceFields : CustomMonoBehaviourWithReferenceFields
	{
		public int derivedField;
	}

	private class CustomMonoBehaviourWithNestedReferenceField : UnityEngine.MonoBehaviour
	{
		public ClassWithReferenceField? nested;
	}

	[Test]
	public async Task ReferenceFieldsHoldAnIdentifierInsteadOfTheObject()
	{
		SerializableType serializableType = SerializableTypes.Create<CustomMonoBehaviourWithReferenceFields>();
		{
			await Assert.That(serializableType.Fields.Count).IsEqualTo(4);
			await Assert.That(serializableType.Fields[0].Type).IsSameReferenceAs(ManagedReferenceTypes.ManagedReference);
			await Assert.That(serializableType.Fields[0].ArrayDepth).IsEqualTo(0);
			await Assert.That(serializableType.Fields[1].Type).IsSameReferenceAs(ManagedReferenceTypes.ManagedReference);
			await Assert.That(serializableType.Fields[1].ArrayDepth).IsEqualTo(1);
			await Assert.That(serializableType.Fields[2].Type).IsSameReferenceAs(ManagedReferenceTypes.ManagedReference);
			await Assert.That(serializableType.Fields[2].ArrayDepth).IsEqualTo(1);
			await Assert.That(serializableType.Fields[3].Type).IsSameReferenceAs(ManagedReferenceTypes.Registry);
		}
	}

	[Test]
	public async Task ReferenceFieldsHoldAnIndexBeforeStableIdentifiersWereIntroduced()
	{
		SerializableType serializableType = SerializableTypes.Create<CustomMonoBehaviourWithReferenceFields>(new UnityVersion(2019, 4));
		{
			await Assert.That(serializableType.Fields.Count).IsEqualTo(4);
			await Assert.That(serializableType.Fields[0].Type).IsSameReferenceAs(ManagedReferenceTypes.IndexedManagedReference);
			await Assert.That(serializableType.Fields[1].Type).IsSameReferenceAs(ManagedReferenceTypes.IndexedManagedReference);
			await Assert.That(serializableType.Fields[2].Type).IsSameReferenceAs(ManagedReferenceTypes.IndexedManagedReference);
			await Assert.That(serializableType.Fields[3].Type).IsSameReferenceAs(ManagedReferenceTypes.Registry);
		}
	}

	[Test]
	public async Task OnlyTheMostDerivedTypeHasAManagedReferenceRegistry()
	{
		SerializableType serializableType = SerializableTypes.Create<DerivedMonoBehaviourWithReferenceFields>();
		{
			await Assert.That(serializableType.Fields.Count(field => field.Type == ManagedReferenceTypes.Registry)).IsEqualTo(1);
			await Assert.That(serializableType.Fields[^1].Type).IsSameReferenceAs(ManagedReferenceTypes.Registry);
			await Assert.That(serializableType.Fields[^2].Name).IsEqualTo(nameof(DerivedMonoBehaviourWithReferenceFields.derivedField));
		}
	}

	[Test]
	public async Task TypesWithoutReferenceFieldsHaveNoManagedReferenceRegistry()
	{
		SerializableType serializableType = SerializableTypes.Create<CustomMonoBehaviourWithPublicFields>();
		await Assert.That(serializableType.Fields.Any(field => field.Type == ManagedReferenceTypes.Registry)).IsFalse();
	}

	[Test]
	public async Task NestedTypesWithReferenceFieldsAreNotRootTypes()
	{
		SerializableType serializableType = SerializableTypes.Create<ClassWithReferenceField>();
		await Assert.That(serializableType.Fields.Any(field => field.Type == ManagedReferenceTypes.Registry)).IsFalse();
	}

	[Test]
	public async Task ManagedReferenceRegistryIsAddedWhenTheNestedTypeWasAlreadyCached()
	{
		TypeDefinition nestedType = ReferenceAssemblies.GetType<ClassWithReferenceField>();
		TypeDefinition behaviourType = ReferenceAssemblies.GetType<CustomMonoBehaviourWithNestedReferenceField>();
		RuntimeContext? runtimeContext = nestedType.DeclaringModule?.RuntimeContext;
		FieldSerializer serializer = new(new UnityVersion(6000), runtimeContext);
		Dictionary<ITypeDefOrRef, SerializableType> typeCache = new(runtimeContext?.SignatureComparer);

		if (!serializer.TryCreateSerializableType(nestedType, typeCache, out _, out string? nestedFailure))
			throw new InvalidOperationException(nestedFailure);
		if (!serializer.TryCreateSerializableType(behaviourType, typeCache, out SerializableType? serializableType, out string? failure))
			throw new InvalidOperationException(failure);
		await Assert.That(serializableType!.Fields[^1].Type).IsSameReferenceAs(ManagedReferenceTypes.Registry);
	}
}
