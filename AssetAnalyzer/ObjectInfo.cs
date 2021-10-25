namespace AssetAnalyzer
{
	public class ObjectInfo
	{
		public long byteStart;
		public uint byteSize;
		public int typeID;
		public int classID;
		public ushort isDestroyed;
		public byte stripped;

		public long m_PathID;
		public SerializedType serializedType;
	}
}
