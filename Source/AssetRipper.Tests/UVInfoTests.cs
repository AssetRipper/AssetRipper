using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Tests;

internal class UVInfoTests
{
	[Test]
	public async Task GetReturnsTheSetValue()
	{
		const int index = 0;
		const int dimension = 2;
		UVInfo uvInfo = new UVInfo().AddChannelInfo(index, true, dimension);
		uvInfo.GetChannelInfo(index, out bool actualExists, out int actualDimension);

		{
			await Assert.That(actualExists).IsTrue();
			await Assert.That(actualDimension).IsEqualTo(dimension);
		}
	}

	[Test]
	public async Task GetReturnsTheSetValues()
	{
		const int dimension = 2;
		UVInfo uvInfo = default;
		for (int i = 0; i < 8; i++)
		{
			uvInfo = uvInfo.AddChannelInfo(i, i % 2 == 0, dimension);
		}
		for (int i = 0; i < 8; i++)
		{
			uvInfo.GetChannelInfo(i, out bool actualExists, out int actualDimension);

			{
				await Assert.That(actualExists).IsEqualTo(i % 2 == 0);
				await Assert.That(actualDimension).IsEqualTo(dimension);
			}
		}
	}
}