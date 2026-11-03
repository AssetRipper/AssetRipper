namespace AssetRipper.SerializationLogic.Tests;

public class ReferenceAssembliesTests
{
	[Test]
	public async Task ResolutionForUnityTypesWorksAsExpected()
	{
		TypeDefinition customMonoBehaviour = ReferenceAssemblies.GetType<NestedMonoBehaviour>();
		await Assert.That(customMonoBehaviour).IsNotNull();
		await Assert.That(customMonoBehaviour.BaseType).IsNotNull();
		customMonoBehaviour.BaseType.TryResolve(customMonoBehaviour.DeclaringModule?.RuntimeContext, out TypeDefinition? monoBehaviour);
		await Assert.That(monoBehaviour).IsNotNull();
		await Assert.That(monoBehaviour?.Name?.ToString()).IsEqualTo(nameof(UnityEngine.MonoBehaviour));
	}

	private class NestedMonoBehaviour : UnityEngine.MonoBehaviour
	{
	}
}
