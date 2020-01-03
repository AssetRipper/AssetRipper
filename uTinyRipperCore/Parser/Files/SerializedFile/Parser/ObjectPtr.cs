namespace uTinyRipper.SerializedFiles
{
	public struct ObjectPtr : ISerializedReadable, ISerializedWritable
	{
		public void Read(SerializedReader reader)
		{
			FileID = reader.ReadInt32();
			if (AssetEntry.IsLongID(reader.Generation))
			{
				reader.AlignStream();
				PathID = reader.ReadInt64();
			}
			else
			{
				PathID = reader.ReadInt32();
			}
		}

		public void Write(SerializedWriter writer)
		{
			writer.Write(FileID);
			if (AssetEntry.IsLongID(writer.Generation))
			{
				writer.AlignStream();
				writer.Write(PathID);
			}
			else
			{
				writer.Write((int)PathID);
			}
		}

		public override string ToString()
		{
			return $"[{FileID}, {PathID}]";
		}

		public int FileID { get; private set; }
		public long PathID { get; private set; }
	}
}
