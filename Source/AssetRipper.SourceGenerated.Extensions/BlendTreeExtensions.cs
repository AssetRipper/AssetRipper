using AssetRipper.SourceGenerated.Classes.ClassID_206;
using BlendTreeType = AssetRipper.SourceGenerated.Enums.BlendTreeType_1;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class BlendTreeExtensions
	{
		public static BlendTreeType GetBlendType(this IBlendTree tree)
		{
			return tree.Has_BlendType_C206_Int32() ? (BlendTreeType)tree.BlendType_C206_Int32 : (BlendTreeType)tree.BlendType_C206_UInt32;
		}
	}
}
