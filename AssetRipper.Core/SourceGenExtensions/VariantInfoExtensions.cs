using AssetRipper.Core.Classes.ShaderVariantCollection;
using AssetRipper.SourceGenerated.Subclasses.VariantInfo;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class VariantInfoExtensions
	{
		public static PassType GetPassType(this IVariantInfo info)
		{
			return (PassType)info.PassType;
		}
	}
}
