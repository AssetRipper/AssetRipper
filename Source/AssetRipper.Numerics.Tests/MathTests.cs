namespace AssetRipper.Numerics.Tests;

public class MathTests
{
	[Test]
	public void TestAsVector3ExtensionMethod()
	{
		Vector4 v = new Vector4(1, 2, 3, 4);
		Assert.That(v.AsVector3(), Is.EqualTo(new Vector3(1, 2, 3)));
	}
}
