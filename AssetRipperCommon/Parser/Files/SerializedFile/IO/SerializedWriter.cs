using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using System.IO;

namespace AssetRipper.Core.Parser.Files.SerializedFiles.IO
{
	public sealed class SerializedWriter : EndianWriter
	{
		public SerializedWriter(Stream stream, EndianType endianess, FormatVersion generation) : base(stream, endianess)
		{
			Generation = generation;
		}

		public void WriteSerialized<T>(T value) where T : ISerializedWritable
		{
			value.Write(this);
		}

		public void WriteSerializedArray<T>(T[] buffer) where T : ISerializedWritable
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i].Write(this);
			}
		}

		public void WriteSerializedTypeArray<T>(T[] buffer, bool hasTypeTree) where T : SerializedTypeBase
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i].Write(this, hasTypeTree);
			}
		}

		public FormatVersion Generation { get; }
	}
}
