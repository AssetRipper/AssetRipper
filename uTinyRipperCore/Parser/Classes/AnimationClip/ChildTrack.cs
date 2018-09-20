namespace uTinyRipper.Classes.AnimationClips
{
	public struct ChildTrack : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Path = reader.ReadStringAligned();
			ClassID = (ClassIDType)reader.ReadInt32();
			Track.Read(reader);
		}

		public string Path { get; private set; }
		public ClassIDType ClassID { get; private set; }

		public PPtr<BaseAnimationTrack> Track;
	}
}
