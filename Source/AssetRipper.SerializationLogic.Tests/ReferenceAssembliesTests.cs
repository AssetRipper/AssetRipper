namespace AssetRipper.SerializationLogic.Tests;

public class ReferenceAssembliesTests
{
	[Test]
	public void ResolutionForUnityTypesWorksAsExpected()
	{
		TypeDefinition customMonoBehaviour = ReferenceAssemblies.GetType<NestedMonoBehaviour>();
		Assert.That(customMonoBehaviour, Is.Not.Null);
		Assert.That(customMonoBehaviour.BaseType, Is.Not.Null);
		customMonoBehaviour.BaseType.TryResolve(customMonoBehaviour.DeclaringModule?.RuntimeContext, out TypeDefinition? monoBehaviour);
		Assert.That(monoBehaviour, Is.Not.Null);
		Assert.That(monoBehaviour?.Name?.ToString(), Is.EqualTo(nameof(UnityEngine.MonoBehaviour)));
	}

	private class NestedMonoBehaviour : UnityEngine.MonoBehaviour
	{
	}
}
