namespace AssetRipper.SerializationLogic.Tests;

public class ReferenceAssembliesTests
{
	[Test]
	public void ResolutionForUnityTypesWorksAsExpected()
	{
		TypeDefinition customMonoBehaviour = ReferenceAssemblies.GetType<NestedMonoBehaviour>();
		Assert.NotNull(customMonoBehaviour);
		Assert.NotNull(customMonoBehaviour.BaseType);
		TypeDefinition? monoBehaviour = customMonoBehaviour.BaseType?.Resolve();
		Assert.NotNull(monoBehaviour);
		Assert.That(monoBehaviour?.Name?.ToString(), Is.EqualTo(nameof(UnityEngine.MonoBehaviour)));
	}

	private class NestedMonoBehaviour : UnityEngine.MonoBehaviour
	{
	}
}
