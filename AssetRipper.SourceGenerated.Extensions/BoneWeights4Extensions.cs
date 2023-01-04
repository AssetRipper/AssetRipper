using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Subclasses.BoneWeights4;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class BoneWeights4Extensions
	{
		public static BoneWeight4 ToCommonClass(this IBoneWeights4 weights)
		{
			return new BoneWeight4(weights.Weight_0_, weights.Weight_1_, weights.Weight_2_, weights.Weight_3_,
				weights.BoneIndex_0_, weights.BoneIndex_1_, weights.BoneIndex_2_, weights.BoneIndex_3_);
		}
	}
}
