namespace UtinyRipper.SerializedFiles
{
	/// <summary>
	/// Contains information for a block of raw serialized object data.
	/// </summary>
	public class AssetEntry : ISerializedFileReadable
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadLongID(FileGeneration generation)
		{
			return generation >= FileGeneration.FG_500;
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadTypeIndex(FileGeneration generation)
		{
			return generation >= FileGeneration.FG_550_x;
		}
		/// <summary>
		/// 5.0.1 to 5.4.x
		/// </summary>
		public static bool IsReadUnknown(FileGeneration generation)
		{
			return generation >= FileGeneration.FG_501_54 &&  generation <= FileGeneration.FG_5unknown;
		}

		public void Read(SerializedFileReader reader)
		{
			if (IsReadLongID(reader.Generation))
			{
				reader.AlignStream(AlignType.Align4);
				PathID = reader.ReadInt64();
			}
			else
			{
				PathID = reader.ReadInt32();
			}
			DataOffset = reader.ReadInt32();
			DataSize = reader.ReadInt32();
			if (IsReadTypeIndex(reader.Generation))
			{
				TypeIndex = reader.ReadInt32();
			}
			else
			{
				TypeID = reader.ReadInt32();
				ClassID = (ClassIDType)reader.ReadInt16();
				ScriptID = reader.ReadInt16();
			}
			if (IsReadUnknown(reader.Generation))
			{
				Unknown = reader.ReadBoolean();
			}
		}

		/// <summary>
		/// ObjectID
		/// Unique ID that identifies the object. Can be used as a key for a map.
		/// </summary>
		public long PathID { get; private set; }
		/// <summary>
		/// Offset to the object data.
		/// Add to SerializedFileHeader.dataOffset to get the absolute offset within the serialized file.
		/// </summary>
		public int DataOffset { get; private set; }
		/// <summary>
		/// Size of the object data.
		/// </summary>
		public int DataSize { get; private set; }
		/// <summary>
		/// Type ID of the object, which is mapped to RTTIBaseClassDescriptor.classID.
		/// Equals to classID if the object is not a MonoBehaviour.
		/// </summary>
		public int TypeID { get; private set; }
		/// <summary>
		/// Class ID of the object.
		/// </summary>
		public ClassIDType ClassID { get; private set; }
		/// <summary>
		/// Index of RTTIBaseClassDescriptor
		/// </summary>
		public int TypeIndex { get; private set; }
		public short ScriptID { get; private set; }
		public bool Unknown { get; private set; }
	}
}
