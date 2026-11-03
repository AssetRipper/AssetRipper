using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Tests;

internal class MeshDataTests
{
	[Test]
	public async Task AccessingNullUV2DoesNotThrow()
	{
		MeshData meshData = MeshData.CreateTriangleMesh() with
		{
			UV2 = null
		};
		await Assert.That(() =>
		{
			meshData.TryGetUV2AtIndex(0);
		}).ThrowsNothing();
	}

	[Test]
	public async Task AccessingEmptyUV2DoesNotThrow()
	{
		MeshData meshData = MeshData.CreateTriangleMesh() with
		{
			UV2 = []
		};
		await Assert.That(() =>
		{
			meshData.TryGetUV2AtIndex(0);
		}).ThrowsNothing();
	}
}