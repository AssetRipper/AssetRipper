namespace uTinyRipper.Classes.AnimationClips
{
	public struct ConstantClip : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Constants = reader.ReadSingleArray();
		}
		
		public bool IsSet => Constants.Length > 0;

		public float[] Constants { get; set; }
	}
}
