using AssetRipper.IO.Endian;
using AssetRipper.Layout;
using AssetRipper.Classes.AnimationClip.Editor;
using AssetRipper.IO.Asset;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Classes.AnimationClip.Clip
{
	public class StreamedClip : IAssetReadable
	{
		public StreamedClip() { }

		public void Read(AssetReader reader)
		{
			Data = reader.ReadUInt32Array();
			CurveCount = (int)reader.ReadUInt32();
		}

		public IReadOnlyList<StreamedFrame> GenerateFrames(AssetLayout layout)
		{
			List<StreamedFrame> frames = new List<StreamedFrame>();
			byte[] memStreamBuffer = new byte[Data.Length * sizeof(uint)];
			Buffer.BlockCopy(Data, 0, memStreamBuffer, 0, memStreamBuffer.Length);
			using (MemoryStream stream = new MemoryStream(memStreamBuffer))
			{
				using (AssetReader reader = new AssetReader(stream, EndianType.LittleEndian, layout))
				{
					while (reader.BaseStream.Position < reader.BaseStream.Length)
					{
						StreamedFrame frame = new StreamedFrame();
						frame.Read(reader);
						frames.Add(frame);
					}
				}
			}
			return frames;
		}

		public bool IsSet => Data.Length > 0;

		public uint[] Data { get; set; }
		public int CurveCount { get; set; }
	}
}
