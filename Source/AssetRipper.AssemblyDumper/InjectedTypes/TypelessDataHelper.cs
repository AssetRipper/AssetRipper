using AssetRipper.IO.Endian;

#nullable disable

namespace AssetRipper.AssemblyDumper.InjectedTypes;

internal static class TypelessDataHelper
{
	public static byte[] ReadByteArray(ref EndianSpanReader reader, int count)
	{
		return reader.ReadBytesExact(count).ToArray();
	}
}
