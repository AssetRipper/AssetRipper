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
	}
}
