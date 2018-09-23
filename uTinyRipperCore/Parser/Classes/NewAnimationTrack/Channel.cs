using uTinyRipper.Classes.AnimationClips;

namespace uTinyRipper.Classes.NewAnimationTracks
{
	public struct Channel : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			ByteOffset = reader.ReadInt32();
			Curve.Read(reader);
			AttributeName = reader.ReadString();
		}

		public int ByteOffset { get; private set; }
		public string AttributeName { get; private set; }

		public AnimationCurveTpl<Float> Curve;
	}
}
