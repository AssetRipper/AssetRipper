using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.SerializationLogic.Tests;

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0044 // Add readonly modifier
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

	private class CustomMonoBehaviourWithListField : UnityEngine.MonoBehaviour
	{
		[UnityEngine.SerializeField]
		private List<string>? listOfStrings;
	}
}
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
