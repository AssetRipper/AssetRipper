using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Tests;

internal class MeshDataTests
{
	[Test]
	public void AccessingNullUV2DoesNotThrow()
	{
		MeshData meshData = MeshData.CreateTriangleMesh() with
		{
			UV2 = null
		};
		Assert.DoesNotThrow(() =>
		{
			meshData.TryGetUV2AtIndex(0);
		});
	}

	[Test]
	public void AccessingEmptyUV2DoesNotThrow()
	{
		MeshData meshData = MeshData.CreateTriangleMesh() with
		{
			UV2 = []
		};
		Assert.DoesNotThrow(() =>
		{
			meshData.TryGetUV2AtIndex(0);
		});
	}
}
