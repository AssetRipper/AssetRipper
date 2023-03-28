using System.Runtime.CompilerServices;

namespace AssetRipper.IO.Endian.Tests;

public static class RandomData
{
	private static Random random = new Random(57);

	public static sbyte NextSByte() => unchecked((sbyte)NextByte());

	public static byte NextByte() => unchecked((byte)random.Next());

	public static short NextInt16() => unchecked((short)NextUInt16());

	public static ushort NextUInt16() => unchecked((ushort)random.Next());

	public static int NextInt32() => random.Next() - random.Next();

	public static uint NextUInt32() => unchecked((uint)NextInt32());

	public static long NextInt64() => random.NextInt64() - random.NextInt64();

	public static ulong NextUInt64() => unchecked((ulong)NextInt64());

	public static Half NextHalf() => (Half)NextSingle();

	public static float NextSingle() => random.NextSingle();

	public static double NextDouble() => random.NextDouble();

	public static bool NextBoolean() => (random.Next() & 1) != 0;

	public static char NextChar() => (char)NextUInt16();

	public static T NextPrimitive<T>() where T : unmanaged
	{
		if (typeof(T) == typeof(bool))
		{
			bool value = NextBoolean();
			return Unsafe.As<bool, T>(ref value);
		}
		else
		{
			Assert.That(Unsafe.SizeOf<T>(), Is.LessThanOrEqualTo(sizeof(ulong)));
			ulong value = NextUInt64();
			return Unsafe.As<ulong, T>(ref value);
		}
	}

	public static byte[] NextBytes(int count)
	{
		byte[] bytes = new byte[count];
		random.NextBytes(bytes);
		return bytes;
	}
}
