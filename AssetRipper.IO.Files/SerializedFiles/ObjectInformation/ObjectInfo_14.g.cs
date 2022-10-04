// Auto-generated code. Do not modify manually.
using AssetRipper.IO.Endian;
namespace AssetRipper.IO.Files.SerializedFiles.ObjectInformation;
/// <summary>
/// Contains information for a block of raw serialized object data.
/// </summary>
public partial record class ObjectInfo_14 : IObjectInfo
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
	private int m_ByteStart = new();
	
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
			m_ByteStart = (int)value;
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
	/// Type ID of the object, which is mapped to <see cref="SerializedType.TypeID"/>. Equals to classID if the object is not <see cref="ClassIDType.MonoBehaviour"/>
	/// </summary>
	private int m_TypeID = new();
	
	/// <summary>
	/// Type ID of the object, which is mapped to <see cref="SerializedType.TypeID"/>. Equals to classID if the object is not <see cref="ClassIDType.MonoBehaviour"/>
	/// </summary>
	public int TypeID
	{
		get => m_TypeID;
		set
		{
			m_TypeID = value;
			OnTypeIDAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="TypeID"/> is set.
	/// </summary>
	partial void OnTypeIDAssignment(int value);
	
	/// <summary>
	/// Class ID of the object.
	/// </summary>
	private short m_ClassID = new();
	
	/// <summary>
	/// Class ID of the object.
	/// </summary>
	public short ClassID
	{
		get => m_ClassID;
		set
		{
			m_ClassID = value;
			OnClassIDAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="ClassID"/> is set.
	/// </summary>
	partial void OnClassIDAssignment(short value);
	
	private short m_ScriptTypeIndex = new();
	
	public short ScriptTypeIndex
	{
		get => m_ScriptTypeIndex;
		set
		{
			m_ScriptTypeIndex = value;
			OnScriptTypeIndexAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="ScriptTypeIndex"/> is set.
	/// </summary>
	partial void OnScriptTypeIndexAssignment(short value);
	
	public ushort IsDestroyed
	{
		get => default;
		set { }
	}
	
	public bool Stripped
	{
		get => false;
		set { }
	}
	
	/// <summary>
	/// Type index in <see cref="SerializedFileMetadata.Types"/> array.
	/// </summary>
	public int SerializedTypeIndex
	{
		get => -1;
		set { }
	}
	
	public void Read(EndianReader reader)
	{
		reader.AlignStream();
		m_FileID = reader.ReadInt64();
		m_ByteStart = reader.ReadInt32();
		m_ByteSize = reader.ReadInt32();
		m_TypeID = reader.ReadInt32();
		m_ClassID = reader.ReadInt16();
		m_ScriptTypeIndex = reader.ReadInt16();
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
		writer.Write(m_TypeID);
		writer.Write(m_ClassID);
		writer.Write(m_ScriptTypeIndex);
		OnWriteFinished(writer);
	}
	
	/// <summary>
	/// Called when <see cref="Write"/> is finished.
	/// </summary>
	partial void OnWriteFinished(EndianWriter writer);
	
}
