namespace uTinyRipper.Classes.AnimationClips
{
	public struct ValueDelta : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Start = reader.ReadSingle();
			Stop = reader.ReadSingle();
		}

		public override string ToString()
		{
			return $"Start:{Start} Stop:{Stop}";
		}

		public float Start { get; private set; }
		public float Stop { get; private set; }
	}
}
