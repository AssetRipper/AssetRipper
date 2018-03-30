namespace UtinyRipper.SerializedFiles
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

		public void Read(SerializedFileStream stream)
		{
			FileID = stream.ReadInt32();
			if (IsReadLongPathID(stream.Generation))
			{
				stream.AlignStream(AlignType.Align4);
				PathID = stream.ReadInt64();
			}
			else
			{
				PathID = stream.ReadInt32();
			}
		}

		public int FileID { get; private set; }
		public long PathID { get; private set; }
	}
}
