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

		public void Read(SerializedFileStream stream)
		{
			if (IsReadLongID(stream.Generation))
			{
				stream.AlignStream(AlignType.Align4);
				PathID = stream.ReadInt64();
			}
			else
			{
				PathID = stream.ReadInt32();
			}
			DataOffset = stream.ReadInt32();
			DataSize = stream.ReadInt32();
			if (IsReadTypeIndex(stream.Generation))
			{
				TypeIndex = stream.ReadInt32();
			}
			else
			{
				TypeID = stream.ReadInt32();
				ClassID = (ClassIDType)stream.ReadInt16();
				ScriptID = stream.ReadInt16();
			}
			if (IsReadUnknown(stream.Generation))
			{
				Unknown = stream.ReadBoolean();
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
		/// Equal to classID if the object is not a MonoBehaviour.
		/// </summary>
		public int TypeID { get; private set; }
		/// <summary>
		/// Class ID of the object.
		/// </summary>
		public ClassIDType ClassID { get; private set; }
		public int TypeIndex { get; private set; }
		public short ScriptID { get; private set; }
		public bool Unknown { get; private set; }
	}
}
