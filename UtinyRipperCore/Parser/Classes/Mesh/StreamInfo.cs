using System;
using System.Collections;

namespace UtinyRipper.Classes.Meshes
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

		public void Read(AssetStream stream)
		{
			ChannelMask = stream.ReadUInt32();
			Offset = stream.ReadUInt32();

			if (IsReadAlign(stream.Version))
			{
				Stride = stream.ReadUInt32();
				Align = stream.ReadUInt32();
			}
			else
			{
				Stride = stream.ReadByte();
				DividerOp = stream.ReadByte();
				Frequency = stream.ReadUInt16();
			}
		}

		public bool IsMatch(ChannelType3 channel)
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
