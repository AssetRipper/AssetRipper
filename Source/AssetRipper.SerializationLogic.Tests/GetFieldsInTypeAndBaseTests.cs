using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AssetRipper.SerializationLogic.Tests;

public class GetFieldsInTypeAndBaseTests
{
	[Test]
	public void InheritedGenericFieldsGetInstantiatedCorrectly()
	{
		TypeDefinition type = ReferenceAssemblies.GetType<Gamma>();
		IEnumerator<(FieldDefinition, TypeSignature)> enumerator = FieldQuery.GetFieldsInTypeAndBase(type).GetEnumerator();
		{
			Assert.That(enumerator.MoveNext());
			(FieldDefinition field, TypeSignature fieldType) = enumerator.Current;
			Assert.That(field.Name?.ToString(), Is.EqualTo(nameof(Alpha<int>.alphaField)));
			Assert.That(fieldType is CorLibTypeSignature { ElementType: ElementType.String });
		}
		{
			Assert.That(enumerator.MoveNext());
			(FieldDefinition field, TypeSignature fieldType) = enumerator.Current;
			Assert.That(field.Name?.ToString(), Is.EqualTo(nameof(Beta<long, long>.betaField)));
			Assert.That(fieldType is CorLibTypeSignature { ElementType: ElementType.I4 });
		}
		{
			Assert.That(!enumerator.MoveNext());
		}
	}

	[Test]
	public void GenericFieldTypesGetResolvedCorrectly()
	{
		TypeDefinition type = ReferenceAssemblies.GetType<Delta>();

		IEnumerable<(FieldDefinition, TypeSignature)> fields = FieldQuery.GetFieldsInTypeAndBase(type);

		Assert.That(fields.Count(), Is.EqualTo(1));
		(FieldDefinition field, TypeSignature fieldType) = fields.First();
		Assert.That(field.Name?.ToString(), Is.EqualTo(nameof(Delta.alphaField)));
		Assert.That(fieldType, Is.InstanceOf<GenericInstanceTypeSignature>());

		GenericInstanceTypeSignature genericFieldType = (GenericInstanceTypeSignature)fieldType;
		Assert.That(genericFieldType.GenericType.Name!.Value, Is.EqualTo("Alpha`1"));
		Assert.That(genericFieldType.TypeArguments, Has.Count.EqualTo(1));
		Assert.That(genericFieldType.TypeArguments[0].Name, Is.EqualTo("Int32"));
	}

	private class Alpha<T>
	{
		public T? alphaField;
	}
	private class Beta<T, S> : Alpha<T>
	{
		public S? betaField;
	}
	private class Gamma : Beta<string, int>
	{
	}
	private class Delta
	{
		public Alpha<int>? alphaField;
	}
}
