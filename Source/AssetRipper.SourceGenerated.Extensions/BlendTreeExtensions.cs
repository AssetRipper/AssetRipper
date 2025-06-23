using AssetRipper.SourceGenerated.Classes.ClassID_206;
using BlendTreeType = AssetRipper.SourceGenerated.Enums.BlendTreeType_1;

namespace AssetRipper.SourceGenerated.Extensions;

public static class BlendTreeExtensions
{
	public static BlendTreeType GetBlendType(this IBlendTree tree)
	{
		return tree.Has_BlendType_Int32() ? tree.BlendType_Int32E : tree.BlendType_UInt32E;
	}
}
