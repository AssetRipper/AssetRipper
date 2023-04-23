namespace AssetRipper.SerializationLogic.Tests;

public class FieldSerializationTests
{
	[Test]
	public void ResolutionForUnityTypesWorksAsExpected()
	{
		TypeDefinition customMonoBehaviour = ReferenceAssemblies.GetType<CustomMonoBehaviourWithListField>();
		FieldDefinition field = customMonoBehaviour.Fields.Single();
		Assert.Multiple(() =>
		{
			Assert.That(FieldSerializationLogic.HasSerializeFieldAttribute(field), "Could not detect the SerializeField attribute.");
			Assert.That(FieldSerializationLogic.WillUnitySerialize(field));
		});
	}

	[Test]
	public void DeserializationSupportsGenericTypes()
	{
		TypeDefinition customMonoBehaviour = ReferenceAssemblies.GetType<CustomMonoBehaviourWithGenericField>();
		FieldDefinition field = customMonoBehaviour.Fields.Single();
		
		//This isn't the best check because it doesn't validate if we actually support deserializing the type, but it's a start.
		Assert.That(FieldSerializationLogic.WillUnitySerialize(field));
	}

	private class CustomMonoBehaviourWithListField : UnityEngine.MonoBehaviour
	{
		[UnityEngine.SerializeField]
		[SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Required for unit tests")]
		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for unit tests")]
		private List<string>? listOfStrings;
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
}
