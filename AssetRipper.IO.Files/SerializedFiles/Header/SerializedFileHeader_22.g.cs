// Auto-generated code. Do not modify manually.
using AssetRipper.IO.Endian;
namespace AssetRipper.IO.Files.SerializedFiles.Header;
/// <summary>
/// The file header is found at the beginning of an asset file. The header is always using big endian byte order.
/// </summary>
public partial record class SerializedFileHeader_22 : ISerializedFileHeader
{
	/// <summary>
	/// File format version. The number is required for backward compatibility and is normally incremented after the file format has been changed in a major update.
	/// </summary>
	private int m_Version = new();
	
	/// <summary>
	/// File format version. The number is required for backward compatibility and is normally incremented after the file format has been changed in a major update.
	/// </summary>
	public FormatVersion Version
	{
		get => (FormatVersion)m_Version;
		set
		{
			m_Version = (int)value;
			OnVersionAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="Version"/> is set.
	/// </summary>
	partial void OnVersionAssignment(FormatVersion value);
	
	/// <summary>
	/// This controls the byte order of the data structure. False is little endian. True is big endian.
	/// </summary>
	/// <remarks>
	/// This field is normally set to 0.
	/// </remarks>
	private bool m_SwapEndianess = new();
	
	/// <summary>
	/// This controls the byte order of the data structure. False is little endian. True is big endian.
	/// </summary>
	/// <remarks>
	/// This field is normally set to 0.
	/// </remarks>
	public bool SwapEndianess
	{
		get => m_SwapEndianess;
		set
		{
			m_SwapEndianess = value;
			OnSwapEndianessAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="SwapEndianess"/> is set.
	/// </summary>
	partial void OnSwapEndianessAssignment(bool value);
	
	/// <summary>
	/// Size of the metadata section in the file
	/// </summary>
	private uint m_MetadataSize = new();
	
	/// <summary>
	/// Size of the metadata section in the file
	/// </summary>
	public long MetadataSize
	{
		get => m_MetadataSize;
		set
		{
			m_MetadataSize = (uint)value;
			OnMetadataSizeAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="MetadataSize"/> is set.
	/// </summary>
	partial void OnMetadataSizeAssignment(long value);
	
	/// <summary>
	/// Size of the whole file
	/// </summary>
	private long m_FileSize = new();
	
	/// <summary>
	/// Size of the whole file
	/// </summary>
	public long FileSize
	{
		get => m_FileSize;
		set
		{
			m_FileSize = value;
			OnFileSizeAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="FileSize"/> is set.
	/// </summary>
	partial void OnFileSizeAssignment(long value);
	
	/// <summary>
	/// Offset to the serialized object data. It starts at the data for the first object.
	/// </summary>
	private long m_DataOffset = new();
	
	/// <summary>
	/// Offset to the serialized object data. It starts at the data for the first object.
	/// </summary>
	public long DataOffset
	{
		get => m_DataOffset;
		set
		{
			m_DataOffset = value;
			OnDataOffsetAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="DataOffset"/> is set.
	/// </summary>
	partial void OnDataOffsetAssignment(long value);
	
	/// <summary>
	/// An unknown field introduced in version 22.
	/// </summary>
	private long m_Unknown22 = new();
	
	/// <summary>
	/// An unknown field introduced in version 22.
	/// </summary>
	public long Unknown22
	{
		get => m_Unknown22;
		set
		{
			m_Unknown22 = value;
			OnUnknown22Assignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="Unknown22"/> is set.
	/// </summary>
	partial void OnUnknown22Assignment(long value);
	
	public void Read(EndianReader reader)
	{
		reader.ReadUInt32();
		reader.ReadUInt32();
		m_Version = reader.ReadInt32();
		reader.ReadUInt32();
		m_SwapEndianess = reader.ReadBoolean();
		reader.AlignStream();
		m_MetadataSize = reader.ReadUInt32();
		m_FileSize = reader.ReadInt64();
		m_DataOffset = reader.ReadInt64();
		m_Unknown22 = reader.ReadInt64();
		OnReadFinished(reader);
	}
	
	/// <summary>
	/// Called when <see cref="Read"/> is finished.
	/// </summary>
	partial void OnReadFinished(EndianReader reader);
	
	public void Write(EndianWriter writer)
	{
		writer.Write(default(uint));
		writer.Write(default(uint));
		writer.Write(m_Version);
		writer.Write(default(uint));
		writer.Write(m_SwapEndianess);
		writer.AlignStream();
		writer.Write(m_MetadataSize);
		writer.Write(m_FileSize);
		writer.Write(m_DataOffset);
		writer.Write(m_Unknown22);
		OnWriteFinished(writer);
	}
	
	/// <summary>
	/// Called when <see cref="Write"/> is finished.
	/// </summary>
	partial void OnWriteFinished(EndianWriter writer);
	
}
