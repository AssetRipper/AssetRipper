#nullable disable

namespace AssetRipper.AssemblyDumper.InjectedTypes;

internal static class HashHelper
{
	public static string ToString(byte bytes__0, byte bytes__1, byte bytes__2, byte bytes__3, byte bytes__4, byte bytes__5, byte bytes__6, byte bytes__7, byte bytes__8, byte bytes__9, byte bytes_10, byte bytes_11, byte bytes_12, byte bytes_13, byte bytes_14, byte bytes_15)
	{
		//Not sure if this depends on Endianess
		//If it does, it might be best to split Hash at Unity 5
		uint Data0 = bytes__0 | (uint)bytes__1 << 8 | (uint)bytes__2 << 16 | (uint)bytes__3 << 24;
		uint Data1 = bytes__4 | (uint)bytes__5 << 8 | (uint)bytes__6 << 16 | (uint)bytes__7 << 24;
		uint Data2 = bytes__8 | (uint)bytes__9 << 8 | (uint)bytes_10 << 16 | (uint)bytes_11 << 24;
		uint Data3 = bytes_12 | (uint)bytes_13 << 8 | (uint)bytes_14 << 16 | (uint)bytes_15 << 24;
		string str = $"{Data0:x8}{Data1:x8}{Data2:x8}{Data3:x8}";
		return str;
	}
}
