using AssetRipper.IO.Files.SerializedFiles.IO;

namespace AssetRipper.IO.Files.SerializedFiles.Parser;

/// <summary>
/// Contains information for a block of raw serialized object data.
/// </summary>
public struct ObjectInfo : ISerializedReadable, ISerializedWritable
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
	public static bool HasScriptID(FormatVersion generation) => generation >= FormatVersion.HasScriptTypeIndex && generation < FormatVersion.RefactorTypeData;
	/// <summary>
	/// 5.0.1 to 5.5.0unk exclusive / Format Version at least 15 but less than 17
	/// </summary>
	public static bool HasStripped(FormatVersion generation) => generation >= FormatVersion.SupportsStrippedObject && generation < FormatVersion.RefactorTypeData;
	/// <summary>
	/// 2020.1.0 and greater / Format Version at least 22
	/// </summary>
	public static bool HasLargeFilesSupport(FormatVersion generation) => generation >= FormatVersion.LargeFilesSupport;

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

		if (HasLargeFilesSupport(reader.Generation))
		{
			ByteStart = reader.ReadInt64();
		}
		else
		{
			ByteStart = reader.ReadUInt32();
		}

		ByteSize = reader.ReadInt32();
		if (reader.Generation >= FormatVersion.RefactorTypeData)
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

	public readonly void Write(SerializedWriter writer)
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
			writer.Write(ByteStart);
		}
		else
		{
			writer.Write((uint)ByteStart);
		}

		writer.Write(ByteSize);
		if (writer.Generation >= FormatVersion.RefactorTypeData)
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

	public override readonly string ToString()
	{
		return $"{ClassID}[{FileID}]";
	}

	public readonly SerializedType? GetSerializedType(ReadOnlySpan<SerializedType> types)
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
				if (type.TypeID == TypeID && type.IsStrippedType == Stripped)
				{
					if (result is null)
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

	public void Initialize(ReadOnlySpan<SerializedType> types)
	{
		if (SerializedTypeIndex >= 0)
		{
			SerializedType type = types[SerializedTypeIndex];
			TypeID = type.TypeID;
			if (type.TypeID < short.MaxValue)
			{
				ClassID = (short)type.TypeID;
			}
			ScriptTypeIndex = type.ScriptTypeIndex;
			Stripped = type.IsStrippedType;
		}
	}

	/// <summary>
	/// ObjectID<br/>
	/// Unique ID that identifies the object. Can be used as a key for a map.
	/// </summary>
	public long FileID { get; set; }
	/// <summary>
	/// Offset to the object data.<br/>
	/// Add to <see cref="SerializedFileHeader.DataOffset"/> to get the absolute offset within the serialized file.
	/// </summary>
	public long ByteStart { get; set; }
	/// <summary>
	/// Size of the object data.
	/// </summary>
	public int ByteSize { get; set; }
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
	/// <summary>
	/// The data referenced by <see cref="ByteStart"/> and <see cref="ByteSize"/>.
	/// </summary>
	public byte[]? ObjectData { get; set; }
}
