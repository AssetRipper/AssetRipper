using AssetRipper.Core.Classes.Misc;
using AssetRipper.SourceGenerated.Subclasses.BoneWeights4;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class BoneWeights4Extensions
	{
		public static BoneWeights4 ToCommonClass(this IBoneWeights4 weights)
		{
			return new BoneWeights4(weights.Weight_0_, weights.Weight_1_, weights.Weight_2_, weights.Weight_3_,
				weights.BoneIndex_0_, weights.BoneIndex_1_, weights.BoneIndex_2_, weights.BoneIndex_3_);
		}
	}
}
