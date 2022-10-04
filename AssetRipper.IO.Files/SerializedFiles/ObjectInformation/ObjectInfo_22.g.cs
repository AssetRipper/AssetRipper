// Auto-generated code. Do not modify manually.
using AssetRipper.IO.Endian;
namespace AssetRipper.IO.Files.SerializedFiles.ObjectInformation;
/// <summary>
/// Contains information for a block of raw serialized object data.
/// </summary>
public partial record class ObjectInfo_22 : IObjectInfo
{
	/// <summary>
	/// ObjectID
	/// </summary>
	/// <remarks>
	/// Unique ID that identifies the object. Can be used as a key for a map.
	/// </remarks>
	private long m_FileID = new();
	
	/// <summary>
	/// ObjectID
	/// </summary>
	/// <remarks>
	/// Unique ID that identifies the object. Can be used as a key for a map.
	/// </remarks>
	public long FileID
	{
		get => m_FileID;
		set
		{
			m_FileID = value;
			OnFileIDAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="FileID"/> is set.
	/// </summary>
	partial void OnFileIDAssignment(long value);
	
	/// <summary>
	/// Offset to the object data.
	/// </summary>
	/// <remarks>
	/// Add to <see cref="SerializedFileHeader.DataOffset"/> to get the absolute offset within the serialized file.
	/// </remarks>
	private long m_ByteStart = new();
	
	/// <summary>
	/// Offset to the object data.
	/// </summary>
	/// <remarks>
	/// Add to <see cref="SerializedFileHeader.DataOffset"/> to get the absolute offset within the serialized file.
	/// </remarks>
	public long ByteStart
	{
		get => m_ByteStart;
		set
		{
			m_ByteStart = value;
			OnByteStartAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="ByteStart"/> is set.
	/// </summary>
	partial void OnByteStartAssignment(long value);
	
	/// <summary>
	/// Size of the object data.
	/// </summary>
	private int m_ByteSize = new();
	
	/// <summary>
	/// Size of the object data.
	/// </summary>
	public int ByteSize
	{
		get => m_ByteSize;
		set
		{
			m_ByteSize = value;
			OnByteSizeAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="ByteSize"/> is set.
	/// </summary>
	partial void OnByteSizeAssignment(int value);
	
	/// <summary>
	/// Type index in <see cref="SerializedFileMetadata.Types"/> array.
	/// </summary>
	private int m_SerializedTypeIndex = new();
	
	/// <summary>
	/// Type index in <see cref="SerializedFileMetadata.Types"/> array.
	/// </summary>
	public int SerializedTypeIndex
	{
		get => m_SerializedTypeIndex;
		set
		{
			m_SerializedTypeIndex = value;
			OnSerializedTypeIndexAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="SerializedTypeIndex"/> is set.
	/// </summary>
	partial void OnSerializedTypeIndexAssignment(int value);
	
	/// <summary>
	/// Type ID of the object, which is mapped to <see cref="SerializedType.TypeID"/>. Equals to classID if the object is not <see cref="ClassIDType.MonoBehaviour"/>
	/// </summary>
	public int TypeID
	{
		get => -1;
		set { }
	}
	
	/// <summary>
	/// Class ID of the object.
	/// </summary>
	public short ClassID
	{
		get => (short)TypeID;
		set => TypeID = value;
	}
	
	public ushort IsDestroyed
	{
		get => default;
		set { }
	}
	
	public short ScriptTypeIndex
	{
		get => default;
		set { }
	}
	
	public bool Stripped
	{
		get => false;
		set { }
	}
	
	public void Read(EndianReader reader)
	{
		reader.AlignStream();
		m_FileID = reader.ReadInt64();
		m_ByteStart = reader.ReadInt64();
		m_ByteSize = reader.ReadInt32();
		m_SerializedTypeIndex = reader.ReadInt32();
		OnReadFinished(reader);
	}
	
	/// <summary>
	/// Called when <see cref="Read"/> is finished.
	/// </summary>
	partial void OnReadFinished(EndianReader reader);
	
	public void Write(EndianWriter writer)
	{
		writer.AlignStream();
		writer.Write(m_FileID);
		writer.Write(m_ByteStart);
		writer.Write(m_ByteSize);
		writer.Write(m_SerializedTypeIndex);
		OnWriteFinished(writer);
	}
	
	/// <summary>
	/// Called when <see cref="Write"/> is finished.
	/// </summary>
	partial void OnWriteFinished(EndianWriter writer);
	
}
