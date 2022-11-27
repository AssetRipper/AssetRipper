using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
