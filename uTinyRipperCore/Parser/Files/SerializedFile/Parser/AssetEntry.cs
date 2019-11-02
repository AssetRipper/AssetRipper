namespace uTinyRipper.SerializedFiles
{
	/// <summary>
	/// Contains information for a block of raw serialized object data.
	/// </summary>
	public struct AssetEntry : ISerializedReadable, ISerializedWritable
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsLongID(FileGeneration generation) => generation >= FileGeneration.FG_500;
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasTypeIndex(FileGeneration generation) => generation >= FileGeneration.FG_550_2018;
		/// <summary>
		/// 5.0.1 to 5.5.0b exclusive
		/// </summary>
		public static bool HasStripped(FileGeneration generation) => generation >= FileGeneration.FG_501_54 && generation <= FileGeneration.FG_5unknown;

		public void Read(SerializedReader reader)
		{
			if (IsLongID(reader.Generation))
			{
				reader.AlignStream();
				PathID = reader.ReadInt64();
			}
			else
			{
				PathID = reader.ReadInt32();
			}

			Offset = reader.ReadUInt32();
			Size = reader.ReadInt32();
			if (HasTypeIndex(reader.Generation))
			{
				TypeIndex = reader.ReadInt32();
			}
			else
			{
				TypeID = reader.ReadInt32();
				ClassID = (ClassIDType)reader.ReadInt16();
				ScriptID = reader.ReadInt16();
			}

			if (HasStripped(reader.Generation))
			{
				IsStripped = reader.ReadBoolean();
			}
		}

		public void Write(SerializedWriter writer)
		{
			if (IsLongID(writer.Generation))
			{
				writer.AlignStream();
				writer.Write(PathID);
			}
			else
			{
				writer.Write((int)PathID);
			}

			writer.Write(Offset);
			writer.Write(Size);
			if (HasTypeIndex(writer.Generation))
			{
				writer.Write(TypeIndex);
			}
			else
			{
				writer.Write(TypeID);
				writer.Write((short)ClassID);
				writer.Write(ScriptID);
			}

			if (HasStripped(writer.Generation))
			{
				writer.Write(IsStripped);
			}
		}

		public override string ToString()
		{
			return $"{ClassID}[{PathID}]";
		}

		/// <summary>
		/// ObjectID
		/// Unique ID that identifies the object. Can be used as a key for a map.
		/// </summary>
		public long PathID { get; set; }
		/// <summary>
		/// Offset to the object data.
		/// Add to SerializedFileHeader.dataOffset to get the absolute offset within the serialized file.
		/// </summary>
		public uint Offset { get; set; }
		/// <summary>
		/// Size of the object data.
		/// </summary>
		public int Size { get; set; }
		/// <summary>
		/// Type index in <see cref="RTTIClassHierarchyDescriptor.Types"/> array
		/// </summary>
		public int TypeIndex { get; set; }
		/// <summary>
		/// Type ID of the object, which is mapped to RTTIBaseClassDescriptor.classID.
		/// Equals to classID if the object is not a MonoBehaviour.
		/// </summary>
		public int TypeID { get; set; }
		/// <summary>
		/// Class ID of the object.
		/// </summary>
		public ClassIDType ClassID { get; set; }
		public short ScriptID { get; set; }
		public bool IsStripped { get; set; }
	}
}
