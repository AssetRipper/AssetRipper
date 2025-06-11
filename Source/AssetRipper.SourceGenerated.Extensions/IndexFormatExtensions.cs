using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class IndexFormatExtensions
{
	extension(IndexFormat format)
	{
		public int Size => format is IndexFormat.UInt16 ? sizeof(ushort) : sizeof(uint);
	}
}
