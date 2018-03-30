using System;

namespace UtinyRipper.Classes.Meshes
{
	public struct StreamInfo : IAssetReadable
	{
		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool IsReadAlign(Version version)
		{
			return version.IsLess(4);
		}

		public void Read(AssetStream stream)
		{
			ChannelMask = stream.ReadUInt32();
			Offset = stream.ReadUInt32();

			if(IsReadAlign(stream.Version))
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

		public uint ChannelMask { get; private set; }
		public uint Offset { get; private set; }
		public uint Stride { get; private set; }
		public uint Align { get; private set; }
		public byte DividerOp { get; private set; }
		public ushort Frequency { get; private set; }
	}
}
