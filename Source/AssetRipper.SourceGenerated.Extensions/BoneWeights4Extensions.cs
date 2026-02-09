using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Subclasses.BoneWeights4;

namespace AssetRipper.SourceGenerated.Extensions;

public static class BoneWeights4Extensions
{
	public static BoneWeight4 ToCommonClass(this IBoneWeights4 weights)
	{
		return new BoneWeight4(weights.Weight_0_, weights.Weight_1_, weights.Weight_2_, weights.Weight_3_,
			weights.BoneIndex_0_, weights.BoneIndex_1_, weights.BoneIndex_2_, weights.BoneIndex_3_);
	}

	public static void CopyValues(this IBoneWeights4 vector, BoneWeight4 source)
	{
		vector.Weight_0_ = source.Weight0;
		vector.Weight_1_ = source.Weight1;
		vector.Weight_2_ = source.Weight2;
		vector.Weight_3_ = source.Weight3;
		vector.BoneIndex_0_ = source.Index0;
		vector.BoneIndex_1_ = source.Index1;
		vector.BoneIndex_2_ = source.Index2;
		vector.BoneIndex_3_ = source.Index3;
	}
}
