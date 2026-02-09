namespace AssetRipper.IO.Files.Tests;

internal static class RandomData
{
	public static byte[] MakeRandomData(int size)
	{
		byte[] data = new byte[size];
		TestContext.CurrentContext.Random.NextBytes(data);
		return data;
	}
}
