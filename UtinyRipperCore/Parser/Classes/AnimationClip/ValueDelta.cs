namespace UtinyRipper.Classes.AnimationClips
{
	public struct ValueDelta : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			Start = stream.ReadSingle();
			Stop = stream.ReadSingle();
		}

		public float Start { get; private set; }
		public float Stop { get; private set; }
	}
}
