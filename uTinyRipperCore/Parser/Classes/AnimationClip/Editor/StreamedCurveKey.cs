namespace uTinyRipper.Classes.AnimationClips.Editor
{
	public struct StreamedCurveKey : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Index = reader.ReadInt32();
			TCB.Read(reader);
			Value = reader.ReadSingle();
		}

		public float CalculateOutTangent(float prevValue, float nextValue)
		{
			float sum1 = ((1 - TCB.X) * (1 + TCB.Y) * (1 + TCB.Z) / 2.0f) * (Value - prevValue);
			float sum2 = ((1 - TCB.X) * (1 - TCB.Y) * (1 - TCB.Z) / 2.0f) * (nextValue - Value);
			return (sum1 + sum2);
		}

		public float CalculateNextInTangent(float nextValue, float afterNextValue)
		{
			float sum1 = ((1 - TCB.X) * (1 - TCB.Y) * (1 + TCB.Z) / 2.0f) * (nextValue - Value);
			float sum2 = ((1 - TCB.X) * (1 + TCB.Y) * (1 - TCB.Z) / 2.0f) * (afterNextValue - nextValue);
			return (sum1 + sum2);
		}

		public int Index { get; private set; }
		public float Value { get; private set; }

		public Vector3f TCB;
	}
}
