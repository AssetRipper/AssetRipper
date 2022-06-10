using AssetRipper.Core.Classes.BlendTree;
using AssetRipper.SourceGenerated.Classes.ClassID_206;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class BlendTreeExtensions
	{
		public static BlendTreeType GetBlendType(this IBlendTree tree)
		{
			return tree.Has_BlendType_C206_Int32() ? (BlendTreeType)tree.BlendType_C206_Int32 : (BlendTreeType)tree.BlendType_C206_UInt32;
		}
	}
}
