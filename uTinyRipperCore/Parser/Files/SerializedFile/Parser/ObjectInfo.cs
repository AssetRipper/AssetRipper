namespace uTinyRipper.SerializedFiles
{
	/// <summary>
	/// Contains information for a block of raw serialized object data.
	/// </summary>
	public struct ObjectInfo : ISerializedReadable, ISerializedWritable
	{
		/// <summary>
		/// 5.0.0unk and greater
		/// </summary>
		public static bool IsLongID(FormatVersion generation) => generation >= FormatVersion.Unknown_14;
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		public static bool HasClassID(FormatVersion generation) => generation < FormatVersion.RefactoredClassId;
		/// <summary>
		/// Less than 5.0.0unk
		/// </summary>
		public static bool HasIsDestroyed(FormatVersion generation) => generation < FormatVersion.HasScriptTypeIndex;
		/// <summary>
		/// 5.0.0unk to 5.5.0unk exclusive
		/// </summary>
		public static bool HasScriptID(FormatVersion generation) => generation >= FormatVersion.HasScriptTypeIndex && generation < FormatVersion.RefactorTypeData;
		/// <summary>
		/// 5.0.1 to 5.5.0unk exclusive
		/// </summary>
		public static bool HasStripped(FormatVersion generation) => generation >= FormatVersion.SupportsStrippedObject && generation < FormatVersion.RefactorTypeData;

		public void Read(SerializedReader reader)
		{
			if (IsLongID(reader.Generation))
			{
				reader.AlignStream();
				FileID = reader.ReadInt64();
			}
			else
			{
				FileID = reader.ReadInt32();
			}

			ByteStart = reader.ReadUInt32();
			ByteSize = reader.ReadInt32();
			TypeID = reader.ReadInt32();
			if (HasClassID(reader.Generation))
			{
				ClassID = (ClassIDType)reader.ReadInt16();
			}
			if (HasScriptID(reader.Generation))
			{
				ScriptTypeIndex = reader.ReadInt16();
			}
			else if (HasIsDestroyed(reader.Generation))
			{
				IsDestroyed = reader.ReadUInt16();
			}
			if (HasStripped(reader.Generation))
			{
				Stripped = reader.ReadBoolean();
			}
		}

		public void Write(SerializedWriter writer)
		{
			if (IsLongID(writer.Generation))
			{
				writer.AlignStream();
				writer.Write(FileID);
			}
			else
			{
				writer.Write((int)FileID);
			}

			writer.Write(ByteStart);
			writer.Write(ByteSize);
			writer.Write(TypeID);
			if (HasClassID(writer.Generation))
			{
				writer.Write((short)ClassID);
			}
			if (HasScriptID(writer.Generation))
			{
				writer.Write(ScriptTypeIndex);
			}
			else if (HasIsDestroyed(writer.Generation))
			{
				writer.Write(IsDestroyed);
			}
			if (HasStripped(writer.Generation))
			{
				writer.Write(Stripped);
			}
		}

		public override string ToString()
		{
			return $"{ClassID}[{FileID}]";
		}

		/// <summary>
		/// ObjectID
		/// Unique ID that identifies the object. Can be used as a key for a map.
		/// </summary>
		public long FileID { get; set; }
		/// <summary>
		/// Offset to the object data.
		/// Add to <see cref="SerializedFileHeader.DataOffset"> to get the absolute offset within the serialized file.
		/// </summary>
		public uint ByteStart { get; set; }
		/// <summary>
		/// Size of the object data.
		/// </summary>
		public int ByteSize { get; set; }
		/// <summary>
		/// New versions:
		///		Type index in <see cref="SerializedFileMetadata.Types"/> array
		/// Old versions:
		///		Type ID of the object, which is mapped to <see cref="SerializedType.TypeID">
		///		Equals to classID if the object is not <see cref="ClassIDType.MonoBehaviour">
		/// </summary>
		public int TypeID { get; set; }
		/// <summary>
		/// Class ID of the object.
		/// </summary>
		public ClassIDType ClassID { get; set; }
		public ushort IsDestroyed { get; set; }
		public short ScriptTypeIndex { get; set; }
		public bool Stripped { get; set; }
	}
}
