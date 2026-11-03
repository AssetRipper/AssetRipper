namespace AssetRipper.IO.Files.Tests;

internal static class RandomData
{
	public static byte[] NextBytes(this Random random, int size)
	{
		byte[] data = new byte[size];
		random.NextBytes(data);
		return data;
	}
}
