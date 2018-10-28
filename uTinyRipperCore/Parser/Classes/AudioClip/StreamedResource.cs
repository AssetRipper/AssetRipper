namespace uTinyRipper.Classes.AudioClips
{
	public struct StreamedResource : IAssetReadable
	{
		/// <summary>
		/// Greater than 5.0.0b1
		/// </summary>
		public static bool IsReadSize(Version version)
		{
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}

		public void Read(AssetReader reader)
		{
			Source = reader.ReadString();
			Offset = (long)reader.ReadUInt64();
			if (IsReadSize(reader.Version))
			{
				Size = (long)reader.ReadUInt64();
			}
			else
			{
				reader.AlignStream(AlignType.Align4);
			}
		}

		public string Source { get; private set; }
		public long Offset { get; private set; }
		public long Size { get; private set; }
	}
}
