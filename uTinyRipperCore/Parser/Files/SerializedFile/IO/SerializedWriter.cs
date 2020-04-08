using System.IO;

namespace uTinyRipper.SerializedFiles
{
	public sealed class SerializedWriter : EndianWriter
	{
		public SerializedWriter(Stream stream, EndianType endianess, FormatVersion generation) :
			base(stream, endianess)
		{
			Generation = generation;
		}

		public void WriteSerialized<T>(T value)
			where T : ISerializedWritable
		{
			value.Write(this);
		}

		public void WriteSerializedArray<T>(T[] buffer)
			where T : ISerializedWritable
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i].Write(this);
			}
		}

		public FormatVersion Generation { get; }
	}
}
