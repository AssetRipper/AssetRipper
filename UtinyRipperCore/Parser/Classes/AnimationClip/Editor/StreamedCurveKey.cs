namespace UtinyRipper.Classes.AnimationClips.Editor
{
	public struct StreamedCurveKey : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			Index = stream.ReadInt32();
			TCB.Read(stream);
			Value = stream.ReadSingle();
		}

		public int Index { get; private set; }
		public float Value { get; private set; }

		public Vector3f TCB;
	}
}
