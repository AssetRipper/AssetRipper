using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.AnimationClips.Editor;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct StreamedClip : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			m_data = stream.ReadUInt32Array();
			CurveCount = stream.ReadUInt32();
		}

		public IReadOnlyList<StreamedFrame> GenerateFrames(Version version, Platform platform)
		{
			List<StreamedFrame> frames = new List<StreamedFrame>();
			byte[] memStreamBuffer = new byte[m_data.Length * sizeof(uint)];
			Buffer.BlockCopy(m_data, 0, memStreamBuffer, 0, memStreamBuffer.Length);
			using (MemoryStream memStream = new MemoryStream(memStreamBuffer))
			{
				using (AssetStream stream = new AssetStream(memStream, version, platform))
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
		public uint CurveCount { get; private set; }

		private uint[] m_data;
	}
}
