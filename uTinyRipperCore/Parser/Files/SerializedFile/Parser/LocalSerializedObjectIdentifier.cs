namespace uTinyRipper.SerializedFiles
{
	public struct LocalSerializedObjectIdentifier : ISerializedReadable, ISerializedWritable
	{
		public void Read(SerializedReader reader)
		{
			LocalSerializedFileIndex = reader.ReadInt32();
			if (ObjectInfo.IsLongID(reader.Generation))
			{
				reader.AlignStream();
				LocalIdentifierInFile = reader.ReadInt64();
			}
			else
			{
				LocalIdentifierInFile = reader.ReadInt32();
			}
		}

		public void Write(SerializedWriter writer)
		{
			writer.Write(LocalSerializedFileIndex);
			if (ObjectInfo.IsLongID(writer.Generation))
			{
				writer.AlignStream();
				writer.Write(LocalIdentifierInFile);
			}
			else
			{
				writer.Write((int)LocalIdentifierInFile);
			}
		}

		public override string ToString()
		{
			return $"[{LocalSerializedFileIndex}, {LocalIdentifierInFile}]";
		}

		public int LocalSerializedFileIndex { get; set; }
		public long LocalIdentifierInFile { get; set; }
	}
}
