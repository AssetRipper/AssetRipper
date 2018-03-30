using UtinyRipper.Classes.AnimationClips;

namespace UtinyRipper.Classes.NewAnimationTracks
{
	public struct Channel : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			ByteOffset = stream.ReadInt32();
			Curve.Read(stream);
			AttributeName = stream.ReadStringAligned();
		}

		public int ByteOffset { get; private set; }
		public string AttributeName { get; private set; }

		public AnimationCurveTpl<Float> Curve;
	}
}
