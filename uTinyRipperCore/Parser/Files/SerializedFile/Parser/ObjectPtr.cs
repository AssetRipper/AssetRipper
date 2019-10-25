namespace uTinyRipper.SerializedFiles
{
	public class ObjectPtr : ISerializedFileReadable
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadLongPathID(FileGeneration generation)
		{
			return generation >= FileGeneration.FG_500;
		}

		public void Read(SerializedFileReader reader)
		{
			FileID = reader.ReadInt32();
			if (IsReadLongPathID(reader.Generation))
			{
				reader.AlignStream(AlignType.Align4);
				PathID = reader.ReadInt64();
			}
			else
			{
				PathID = reader.ReadInt32();
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
