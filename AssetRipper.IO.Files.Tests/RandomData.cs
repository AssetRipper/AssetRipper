namespace AssetRipper.IO.Files.Tests;

internal static class RandomData
{
	public static byte[] MakeRandomData(int size)
	{
		byte[] data = new byte[size];
		new Random(57).NextBytes(data);
		return data;
	}
}
