using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.Classes.AnimationClips.Editor;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct StreamedClip : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			m_data = reader.ReadUInt32Array();
			CurveCount = (int)reader.ReadUInt32();
		}

		public IReadOnlyList<StreamedFrame> GenerateFrames(Version version, Platform platform, TransferInstructionFlags flags)
		{
			List<StreamedFrame> frames = new List<StreamedFrame>();
			byte[] memStreamBuffer = new byte[m_data.Length * sizeof(uint)];
			Buffer.BlockCopy(m_data, 0, memStreamBuffer, 0, memStreamBuffer.Length);
			using (MemoryStream stream = new MemoryStream(memStreamBuffer))
			{
				using (AssetReader reader = new AssetReader(stream, version, platform, flags))
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

		public bool IsValid => Data.Count > 0;
		
		public IReadOnlyList<uint> Data => m_data;
		public int CurveCount { get; private set; }

		private uint[] m_data;
	}
}
