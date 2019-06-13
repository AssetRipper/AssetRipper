namespace uTinyRipper.Classes.Meshes
{
	public struct StreamInfo : IAssetReadable
	{
		public StreamInfo(uint mask, uint offset, uint stride)
		{
			ChannelMask = mask;
			Offset = offset;
			Stride = stride;
			Align = 0;
			DividerOp = 0;
			Frequency = 0;
		}

		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		private static bool IsReadAlign(Version version)
		{
			return version.IsLess(4);
		}

		public void Read(AssetReader reader)
		{
			ChannelMask = reader.ReadUInt32();
			Offset = reader.ReadUInt32();

			if (IsReadAlign(reader.Version))
			{
				Stride = reader.ReadUInt32();
				Align = reader.ReadUInt32();
			}
			else
			{
				Stride = reader.ReadByte();
				DividerOp = reader.ReadByte();
				Frequency = reader.ReadUInt16();
			}
		}

		public bool IsMatch(ChannelTypeV4 channel)
		{
			return (ChannelMask & (1 << (int)channel)) != 0;
		}

		public uint ChannelMask { get; private set; }
		public uint Offset { get; private set; }
		public uint Stride { get; private set; }
		public uint Align { get; private set; }
		public byte DividerOp { get; private set; }
		public ushort Frequency { get; private set; }
	}
}
