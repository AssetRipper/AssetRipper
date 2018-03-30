namespace UtinyRipper.Classes.AudioClips
{
	public struct StreamedResource : IAssetReadable
	{
		/// <summary>
		/// Greater than 5.0.0b1
		/// </summary>
		public static bool IsReadSize(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}

		public void Read(AssetStream stream)
		{
			Source = stream.ReadStringAligned();
			Offset = (long)stream.ReadUInt64();
			if (IsReadSize(stream.Version))
			{
				Size = (long)stream.ReadUInt64();
			}
			else
			{
				stream.AlignStream(AlignType.Align4);
			}
		}

		public string Source { get; private set; }
		public long Offset { get; private set; }
		public long Size { get; private set; }
	}
}
