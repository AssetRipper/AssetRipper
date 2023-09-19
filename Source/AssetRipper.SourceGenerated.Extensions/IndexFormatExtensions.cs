using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class IndexFormatExtensions
{
	public static int ToSize(this IndexFormat instance)
	{
		return instance is IndexFormat.UInt16 ? sizeof(ushort) : sizeof(uint);
	}
}
