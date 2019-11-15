using System;
using System.IO;
using System.Text;
using uTinyRipper.Layout;

namespace uTinyRipper
{
	public sealed class AssetWriter : EndianWriter
	{
		public AssetWriter(Stream stream, EndianType endian, AssetLayout layout) :
			base(stream, endian, layout.IsAlignArrays)
		{
			Layout = layout;
			IsAlignString = layout.IsAlign;
		}

		public override void Write(char value)
		{
			FillInnerBuffer(value);
			Write(m_buffer, 0, sizeof(char));
		}

		public override void Write(string value)
		{
			char[] valueArray = value.ToCharArray();
			int count = Encoding.UTF8.GetByteCount(valueArray, 0, valueArray.Length);
			FillInnerBuffer(count);
			Write(m_buffer, 0, sizeof(int));

			byte[] buffer = count <= m_buffer.Length ? m_buffer : new byte[count];
			int written = Encoding.UTF8.GetBytes(valueArray, 0, valueArray.Length, buffer, 0);
			if(written != count)
			{
				throw new Exception($"Written {written} but expected {count}");
			}
			Write(buffer, 0, written);
			if (IsAlignString)
			{
				AlignStream();
			}
		}

		public override void Write(char[] buffer, int index, int count)
		{
			int byteIndex = 0;
			int byteCount = buffer.Length * sizeof(char);
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toWrite = left < BufferSize ? left : BufferSize;
				for (int i = 0; i < toWrite; i += sizeof(char), index++)
				{
					FillInnerBuffer((ushort)buffer[index], i);
				}
				Write(m_buffer, 0, toWrite);
				byteIndex += toWrite;
			}
		}

		public void WriteAsset<T>(T value)
			where T : IAssetWritable
		{
			value.Write(this);
		}

		public void WriteAssetArray<T>(T[] buffer)
			where T : IAssetWritable
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i].Write(this);
			}
			if (IsAlignArray)
			{
				AlignStream();
			}
		}

		public void WriteAssetArray<T>(T[][] buffer)
			where T : IAssetWritable
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			for (int i = 0; i < buffer.GetLength(0); i++)
			{
				WriteAssetArray(buffer[i]);
			}
			if (IsAlignArray)
			{
				AlignStream();
			}
		}

		public AssetLayout Layout { get; }
		public Version Version => Layout.Info.Version;
		public Platform Platform => Layout.Info.Platform;
		public TransferInstructionFlags Flags => Layout.Info.Flags;

		private bool IsAlignString { get; }
	}
}
