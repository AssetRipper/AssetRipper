using System.Collections.Generic;
using System.Linq;

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

	private class CustomMonoBehaviourWithListField : UnityEngine.MonoBehaviour
	{
		[UnityEngine.SerializeField]
		[SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Required for unit tests")]
		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for unit tests")]
		private List<string>? listOfStrings;
	}
}
