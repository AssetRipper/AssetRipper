using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.VariantInfo;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class VariantInfoExtensions
	{
		public static PassType GetPassType(this IVariantInfo info)
		{
			return (PassType)info.PassType;
		}
	}
}
