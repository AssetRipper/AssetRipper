using AssetRipper.IO.Files.SerializedFiles.IO;

namespace AssetRipper.IO.Files.SerializedFiles.Parser;

/// <summary>
/// Contains information for a block of raw serialized object data.
/// </summary>
public struct ObjectInfo
{
	/// <summary>
	/// 5.0.0unk and greater / Format Version at least 14
	/// </summary>
	public static bool IsLongID(FormatVersion generation) => generation >= FormatVersion.Unknown_14;
	/// <summary>
	/// Less than 5.5.0 / Format Version less than 16
	/// </summary>
	public static bool HasClassID(FormatVersion generation) => generation < FormatVersion.RefactoredClassId;
	/// <summary>
	/// Less than 5.0.0unk / Format Version less than 11
	/// </summary>
	public static bool HasIsDestroyed(FormatVersion generation) => generation < FormatVersion.HasScriptTypeIndex;
	/// <summary>
	/// 5.0.0unk to 5.5.0unk exclusive / Format Version at least 11 but less than 17
	/// </summary>
	public static bool HasScriptTypeIndex(FormatVersion generation) => generation >= FormatVersion.HasScriptTypeIndex && generation < FormatVersion.RefactorTypeData;
	/// <summary>
	/// 5.0.1 to 5.5.0unk exclusive / Format Version at least 15 but less than 17
	/// </summary>
	public static bool HasStripped(FormatVersion generation) => generation >= FormatVersion.SupportsStrippedObject && generation < FormatVersion.RefactorTypeData;
	/// <summary>
	/// 5.5.0unk and greater / Format Version at least 17
	/// </summary>
	public static bool HasSerializedTypeIndex(FormatVersion generation) => generation >= FormatVersion.RefactorTypeData;
	/// <summary>
	/// 2020.1.0 and greater / Format Version at least 22
	/// </summary>
	public static bool HasLargeFilesSupport(FormatVersion generation) => generation >= FormatVersion.LargeFilesSupport;

	public void Read(SerializedReader reader, bool longFileID, ReadOnlySpan<SerializedType> types, long dataOffset)
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

		/// <summary>
		/// Offset to the object data.<br/>
		/// Add to <see cref="SerializedFileHeader.DataOffset"/> to get the absolute offset within the serialized file.
		/// </summary>
		long byteStart;
		if (HasLargeFilesSupport(reader.Generation))
		{
			byteStart = reader.ReadInt64();
		}
		else
		{
			byteStart = reader.ReadUInt32();
		}

		// Size of the object data.
		int byteSize = reader.ReadInt32();

		// Read object data
		{
			long currentPosition = reader.BaseStream.Position;
			long dataPosition = dataOffset + byteStart;
			reader.BaseStream.Position = dataPosition;
			ObjectData = reader.ReadBytes(byteSize);
			reader.BaseStream.Position = currentPosition;
		}

		if (HasSerializedTypeIndex(reader.Generation))
		{
			SerializedTypeIndex = reader.ReadInt32();
		}
		else
		{
			SerializedTypeIndex = -1;
			TypeID = reader.ReadInt32();
		}
		if (HasClassID(reader.Generation))
		{
			ClassID = reader.ReadInt16();
		}
		if (HasScriptTypeIndex(reader.Generation))
		{
			ScriptTypeIndex = reader.ReadInt16();
		}
		else if (HasIsDestroyed(reader.Generation))
		{
			IsDestroyed = reader.ReadUInt16();
		}
		bool? stripped;
		if (HasStripped(reader.Generation))
		{
			Stripped = reader.ReadBoolean();
			stripped = Stripped;
		}
		else
		{
			Stripped = false;
			stripped = null;
		}
		Type = GetSerializedType(types, stripped);
		if (Type is not null)
		{
			TypeID = Type.TypeID;
			if (!HasClassID(reader.Generation) && Type.TypeID >= short.MinValue && Type.TypeID <= short.MaxValue)
			{
				ClassID = (short)Type.TypeID;
			}
			if (!HasScriptTypeIndex(reader.Generation))
			{
				ScriptTypeIndex = Type.ScriptTypeIndex;
			}
			if (!HasStripped(reader.Generation))
			{
				Stripped = Type.IsStrippedType;
			}
		}
	}

	public readonly void Write(SerializedWriter writer, long byteStart)
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

		if (HasLargeFilesSupport(writer.Generation))
		{
			writer.Write(byteStart);
		}
		else
		{
			writer.Write((uint)byteStart);
		}

		writer.Write(ObjectData.Length);
		if (HasSerializedTypeIndex(writer.Generation))
		{
			writer.Write(SerializedTypeIndex);
		}
		else
		{
			writer.Write(TypeID);
		}
		if (HasClassID(writer.Generation))
		{
			writer.Write(ClassID);
		}
		if (HasScriptTypeIndex(writer.Generation))
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

	public override readonly string ToString()
	{
		return $"{ClassID}[{FileID}]";
	}

	private readonly SerializedType? GetSerializedType(ReadOnlySpan<SerializedType> types, bool? stripped)
	{
		if (SerializedTypeIndex >= 0)
		{
			return types[SerializedTypeIndex];
		}
		else if (types.Length == 0)
		{
			return default; //It's common on Unity 4 and lower for the array to be empty.
		}
		else
		{
			SerializedType? result = null;
			foreach (SerializedType type in types)
			{
				if (type.TypeID == TypeID)
				{
					if (stripped.HasValue && type.IsStrippedType != stripped.Value)
					{
						// If the caller specified a stripped value, skip types that don't match it.
					}
					else if (result is null)
					{
						result = type;
					}
					else
					{
						throw new Exception($"Multiple types with the same ID {TypeID} and stripped {Stripped} found");
					}
				}
			}
			return result ?? throw new Exception($"Type with ID {TypeID} and stripped {Stripped} not found");
		}
	}

	/// <summary>
	/// ObjectID<br/>
	/// Unique ID that identifies the object. Can be used as a key for a map.
	/// </summary>
	public long FileID { get; set; }
	/// <summary>
	/// Type ID of the object, which is mapped to <see cref="SerializedType.TypeID"/><br/>
	/// Equals to classID if the object is not MonoBehaviour"/>
	/// </summary>
	public int TypeID { get; set; }
	/// <summary>
	/// Type index in <see cref="SerializedFileMetadata.Types"/> array<br/>
	/// </summary>
	public int SerializedTypeIndex { get; set; }
	/// <summary>
	/// Class ID of the object.
	/// </summary>
	public short ClassID { get; set; }
	public ushort IsDestroyed { get; set; }
	public short ScriptTypeIndex { get; set; }
	public bool Stripped { get; set; }
	public SerializedType? Type { get; set; }
	/// <summary>
	/// The data for the object.
	/// </summary>
	[AllowNull]
	public byte[] ObjectData { readonly get => field ?? []; set; }
}
