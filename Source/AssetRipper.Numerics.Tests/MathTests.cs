namespace AssetRipper.Numerics.Tests;

public class MathTests
{
	[Test]
	public async Task TestAsVector3ExtensionMethod()
	{
		Vector4 v = new Vector4(1, 2, 3, 4);
		await Assert.That(v.AsVector3()).IsEqualTo(new Vector3(1, 2, 3));
	}
}