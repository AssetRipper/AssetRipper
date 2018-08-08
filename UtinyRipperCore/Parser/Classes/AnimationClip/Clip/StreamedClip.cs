using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.Classes.AnimationClips.Editor;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct StreamedClip : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			m_data = stream.ReadUInt32Array();
			CurveCount = (int)stream.ReadUInt32();
		}

		public IReadOnlyList<StreamedFrame> GenerateFrames(Version version, Platform platform, TransferInstructionFlags flags)
		{
			List<StreamedFrame> frames = new List<StreamedFrame>();
			byte[] memStreamBuffer = new byte[m_data.Length * sizeof(uint)];
			Buffer.BlockCopy(m_data, 0, memStreamBuffer, 0, memStreamBuffer.Length);
			using (MemoryStream memStream = new MemoryStream(memStreamBuffer))
			{
				using (AssetStream stream = new AssetStream(memStream, version, platform, flags))
				{
					while (stream.BaseStream.Position < stream.BaseStream.Length)
					{
						StreamedFrame frame = new StreamedFrame();
						frame.Read(stream);
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
