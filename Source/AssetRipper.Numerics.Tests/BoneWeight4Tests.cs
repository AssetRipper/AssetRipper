namespace AssetRipper.Numerics.Tests;

public class BoneWeight4Tests
{
	[Test]
	public void DefaultCanBeNormalized()
	{
		BoneWeight4 boneWeight = new BoneWeight4().NormalizeWeights();
		using (Assert.EnterMultipleScope())
		{
			Assert.That(boneWeight.Weight0, Is.EqualTo(0.25f));
			Assert.That(boneWeight.Weight1, Is.EqualTo(0.25f));
			Assert.That(boneWeight.Weight2, Is.EqualTo(0.25f));
			Assert.That(boneWeight.Weight3, Is.EqualTo(0.25f));
			Assert.That(boneWeight.Normalized);
		}
	}
}
