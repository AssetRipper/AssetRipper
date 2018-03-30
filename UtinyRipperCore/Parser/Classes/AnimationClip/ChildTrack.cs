namespace UtinyRipper.Classes.AnimationClips
{
	public struct ChildTrack : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			Path = stream.ReadStringAligned();
			ClassID = (ClassIDType)stream.ReadInt32();
			Track.Read(stream);
		}

		public string Path { get; private set; }
		public ClassIDType ClassID { get; private set; }

		public PPtr<BaseAnimationTrack> Track;
	}
}
