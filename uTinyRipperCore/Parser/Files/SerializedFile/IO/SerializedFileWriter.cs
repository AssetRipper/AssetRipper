using System;
using System.IO;

namespace uTinyRipper.SerializedFiles
{
	public sealed class SerializedFileWriter : EndianWriter
	{
		public SerializedFileWriter(Stream stream, EndianType endianess, string name, FileGeneration generation) :
			base(stream, endianess)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
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

		public string Name { get; }
		public FileGeneration Generation { get; }
	}
}
