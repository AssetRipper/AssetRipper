using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class TextureUsageModeExtensions
{
	public static bool IsNormalmap(this TextureUsageMode _this)
	{
		return _this is TextureUsageMode.NormalmapDXT5nm or TextureUsageMode.NormalmapPlain or TextureUsageMode.NormalmapASTCnm;
	}
}
