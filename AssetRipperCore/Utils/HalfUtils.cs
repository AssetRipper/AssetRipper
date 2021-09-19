using System;
using System.Runtime.CompilerServices;

namespace AssetRipper.Core.Utils
{
	public static class HalfUtils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte[] GetBytes(Half half) => BitConverter.GetBytes(half);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Half ToHalf(ushort bytes) => BitConverter.ToHalf(BitConverter.GetBytes(bytes));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Half ToHalf(byte[] value) => BitConverter.ToHalf(value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Half ToHalf(byte[] value, int startIndex) => BitConverter.ToHalf(value, startIndex);
	}
}
