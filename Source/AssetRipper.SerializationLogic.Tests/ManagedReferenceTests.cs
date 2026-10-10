namespace AssetRipper.SerializationLogic.Tests;

public class ManagedReferenceTests
{
	private class Host : UnityEngine.MonoBehaviour
	{
		[UnityEngine.SerializeReference] public object? scalar;
		[UnityEngine.SerializeReference] public List<object>? list;
		[UnityEngine.SerializeReference] public object[]? array;
	}

	private class EngineValueHost : UnityEngine.MonoBehaviour
	{
		public UnityEngine.RenderingLayerMask mask;
		public UnityEngine.LazyLoadReference<UnityEngine.GameObject> asset;
	}

	[Test]
	public void NativeEngineValueLayoutsArePreserved()
	{
		SerializableType type = SerializableTypes.Create<EngineValueHost>();
		Assert.That(type.Fields, Has.Count.EqualTo(2));
		Assert.That(type.Fields[0].Type.Fields.Single().Name, Is.EqualTo("m_Bits"));
		Assert.That(type.Fields[0].Type.Fields.Single().Type.Type, Is.EqualTo(PrimitiveType.UInt));
		Assert.That(type.Fields[1].Type, Is.SameAs(SerializablePointerType.Shared));
	}

	[Test]
	public void ReferenceFieldsStoreIdsWithoutRecursingIntoDeclaredTypes()
	{
		SerializableType type = SerializableTypes.Create<Host>();
		Assert.That(type.HasManagedReferences, Is.True);
		Assert.That(type.Fields.Select(f => f.ArrayDepth), Is.EqualTo(new[] { 0, 1, 1 }));
		foreach (var field in type.Fields)
		{
			Assert.That(field.Type, Is.SameAs(SerializableManagedReferenceType.Instance));
			Assert.That(field.Type.Fields.Single().Name, Is.EqualTo("rid"));
			Assert.That(field.Type.Fields.Single().Type.Type, Is.EqualTo(PrimitiveType.Long));
		}
	}
}
