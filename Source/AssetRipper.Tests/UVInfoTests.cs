using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Tests;

internal class UVInfoTests
{
	[Test]
	public void GetReturnsTheSetValue()
	{
		const int index = 0;
		const bool exists = true;
		const int dimension = 2;
		UVInfo uvInfo = new UVInfo().AddChannelInfo(index, exists, dimension);
		uvInfo.GetChannelInfo(index, out bool actualExists, out int actualDimension);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(actualExists, Is.EqualTo(exists));
			Assert.That(actualDimension, Is.EqualTo(dimension));
		}
	}

	[Test]
	public void GetReturnsTheSetValues()
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
			using (Assert.EnterMultipleScope())
			{
				Assert.That(actualExists, Is.EqualTo(i % 2 == 0));
				Assert.That(actualDimension, Is.EqualTo(dimension));
			}
		}
	}
}
