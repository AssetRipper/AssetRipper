using System;
using System.Runtime.InteropServices;

namespace AssetRipper.Core.Utils
{
	public static class HalfUtils
	{
		public static byte[] GetBytes(Half half) => GetBytes<Half>(half);

		public static Half ToHalf(ushort bytes) => ToHalf(BitConverter.GetBytes(bytes));
		public static Half ToHalf(byte[] value) => ToHalf(value, 0);
		public static Half ToHalf(byte[] value, int startIndex)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			if (startIndex < 0 || startIndex >= value.Length)
				throw new ArgumentOutOfRangeException(nameof(startIndex));

			return FromBytes<Half>(new byte[] { value[startIndex], value[startIndex + 1] });
		}

		private static byte[] GetBytes<T>(T str) where T : struct
		{
			int size = Marshal.SizeOf(str);
			byte[] arr = new byte[size];
			IntPtr ptr = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(str, ptr, true);
			Marshal.Copy(ptr, arr, 0, size);
			Marshal.FreeHGlobal(ptr);
			return arr;
		}

		private static T FromBytes<T>(byte[] arr) where T : struct
		{
			T str = new T();
			int size = Marshal.SizeOf(str);
			IntPtr ptr = Marshal.AllocHGlobal(size);
			Marshal.Copy(arr, 0, ptr, size);
			str = (T)Marshal.PtrToStructure(ptr, str.GetType());
			Marshal.FreeHGlobal(ptr);
			return str;
		}
	}
}
