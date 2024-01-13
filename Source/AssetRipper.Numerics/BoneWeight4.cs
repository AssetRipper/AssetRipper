using System.Diagnostics.Contracts;

namespace AssetRipper.Numerics
{
	public record struct BoneWeight4(
		float Weight0,
		float Weight1,
		float Weight2,
		float Weight3,
		int Index0,
		int Index1,
		int Index2,
		int Index3)
	{
		public readonly bool AnyWeightsNegative => Weight0 < 0f || Weight1 < 0f || Weight2 < 0f || Weight3 < 0f;

		public readonly float Sum => Weight0 + Weight1 + Weight2 + Weight3;

		public void SetWeight(int index, float value)
		{
			switch (index)
			{
				case 0:
					Weight0 = value; break;
				case 1:
					Weight1 = value; break;
				case 2:
					Weight2 = value; break;
				case 3:
					Weight3 = value; break;
				default:
					throw new ArgumentOutOfRangeException(nameof(index), value, null);
			}
		}

		public void SetIndex(int index, int value)
		{
			switch (index)
			{
				case 0:
					Index0 = value; break;
				case 1:
					Index1 = value; break;
				case 2:
					Index2 = value; break;
				case 3:
					Index3 = value; break;
				default:
					throw new ArgumentOutOfRangeException(nameof(index), value, null);
			}
		}

		public readonly BoneWeight4 NormalizeWeights()
		{
			float sum = Sum;
			if (sum == 0f)
			{
				return new BoneWeight4(.25f, .25f, .25f, .25f, Index0, Index1, Index2, Index3);
			}
			else
			{
				float invSum = 1f / sum;
				return new BoneWeight4(Weight0 * invSum, Weight1 * invSum, Weight2 * invSum, Weight3 * invSum, Index0, Index1, Index2, Index3);
			}
		}
	}
}
