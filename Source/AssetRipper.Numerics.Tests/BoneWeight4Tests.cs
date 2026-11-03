namespace AssetRipper.Numerics.Tests;

public class BoneWeight4Tests
{
	[Test]
	public async Task DefaultCanBeNormalized()
	{
		BoneWeight4 boneWeight = new BoneWeight4().NormalizeWeights();

		{
			await Assert.That(boneWeight.Weight0).IsEqualTo(0.25f);
			await Assert.That(boneWeight.Weight1).IsEqualTo(0.25f);
			await Assert.That(boneWeight.Weight2).IsEqualTo(0.25f);
			await Assert.That(boneWeight.Weight3).IsEqualTo(0.25f);
			await Assert.That(boneWeight.Normalized).IsTrue();
		}
	}
}