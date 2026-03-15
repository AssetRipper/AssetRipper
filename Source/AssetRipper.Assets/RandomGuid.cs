using System.Runtime.InteropServices;

namespace AssetRipper.Assets;

public static class RandomGuid
{
	private static readonly Random rng = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH"))
		&& long.TryParse(Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH"), out long l)
			? new(Seed: unchecked((int)l))
			: Random.Shared;

	[ThreadStatic]
	private static byte[]? buf;

	public static Guid Next()
	{
		buf ??= new byte[/*sizeof(Guid)*/16];
		rng.NextBytes(buf);
		return MemoryMarshal.Read<Guid>(buf);
	}

	public static long NextLong() => rng.NextInt64();
}
