// Auto-generated code. Do not modify manually.
using AssetRipper.IO.Endian;
namespace AssetRipper.IO.Files.SerializedFiles.TypeTrees;
public partial record class TypeTreeNode_10 : ITypeTreeNode
{
	/// <summary>
	/// Field type version, starts with 1 and is incremented after the type information has been significantly updated in a new release.
	/// </summary>
	/// <remarks>
	/// Equal to serializedVersion in Yaml format files
	/// </remarks>
	private ushort m_Version = new();
	
	/// <summary>
	/// Field type version, starts with 1 and is incremented after the type information has been significantly updated in a new release.
	/// </summary>
	/// <remarks>
	/// Equal to serializedVersion in Yaml format files
	/// </remarks>
	public int Version
	{
		get => m_Version;
		set
		{
			m_Version = (ushort)value;
			OnVersionAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="Version"/> is set.
	/// </summary>
	partial void OnVersionAssignment(int value);
	
	/// <summary>
	/// Depth of current type relative to root.
	/// </summary>
	private byte m_Level = new();
	
	/// <summary>
	/// Depth of current type relative to root.
	/// </summary>
	public byte Level
	{
		get => m_Level;
		set
		{
			m_Level = value;
			OnLevelAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="Level"/> is set.
	/// </summary>
	partial void OnLevelAssignment(byte value);
	
	/// <summary>
	/// Array flag, set to 1 if type is "Array" or "TypelessData".
	/// </summary>
	private byte m_TypeFlags = new();
	
	/// <summary>
	/// Array flag, set to 1 if type is "Array" or "TypelessData".
	/// </summary>
	public int TypeFlags
	{
		get => m_TypeFlags;
		set
		{
			m_TypeFlags = (byte)value;
			OnTypeFlagsAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="TypeFlags"/> is set.
	/// </summary>
	partial void OnTypeFlagsAssignment(int value);
	
	/// <summary>
	/// Type offset in the string buffer.
	/// </summary>
	private uint m_TypeStrOffset = new();
	
	/// <summary>
	/// Type offset in the string buffer.
	/// </summary>
	public uint TypeStrOffset
	{
		get => m_TypeStrOffset;
		set
		{
			m_TypeStrOffset = value;
			OnTypeStrOffsetAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="TypeStrOffset"/> is set.
	/// </summary>
	partial void OnTypeStrOffsetAssignment(uint value);
	
	/// <summary>
	/// Name offset in the string buffer.
	/// </summary>
	private uint m_NameStrOffset = new();
	
	/// <summary>
	/// Name offset in the string buffer.
	/// </summary>
	public uint NameStrOffset
	{
		get => m_NameStrOffset;
		set
		{
			m_NameStrOffset = value;
			OnNameStrOffsetAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="NameStrOffset"/> is set.
	/// </summary>
	partial void OnNameStrOffsetAssignment(uint value);
	
	/// <summary>
	/// Size of the data value in bytes, e.g. 4 for int. -1 means that there is an array somewhere inside the hierarchy.
	/// </summary>
	/// <remarks>
	/// Note: The padding for the alignment is not included in the size.
	/// </remarks>
	private int m_ByteSize = new();
	
	/// <summary>
	/// Size of the data value in bytes, e.g. 4 for int. -1 means that there is an array somewhere inside the hierarchy.
	/// </summary>
	/// <remarks>
	/// Note: The padding for the alignment is not included in the size.
	/// </remarks>
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
	/// Index of the field that is unique within a tree.
	/// </summary>
	/// <remarks>
	/// Normally starts with 0 and is incremented with each additional field.
	/// </remarks>
	private int m_Index = new();
	
	/// <summary>
	/// Index of the field that is unique within a tree.
	/// </summary>
	/// <remarks>
	/// Normally starts with 0 and is incremented with each additional field.
	/// </remarks>
	public int Index
	{
		get => m_Index;
		set
		{
			m_Index = value;
			OnIndexAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="Index"/> is set.
	/// </summary>
	partial void OnIndexAssignment(int value);
	
	/// <summary>
	/// Meta flags of the field.
	/// </summary>
	private uint m_MetaFlag = new();
	
	/// <summary>
	/// Meta flags of the field.
	/// </summary>
	public TransferMetaFlags MetaFlag
	{
		get => (TransferMetaFlags)m_MetaFlag;
		set
		{
			m_MetaFlag = (uint)value;
			OnMetaFlagAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="MetaFlag"/> is set.
	/// </summary>
	partial void OnMetaFlagAssignment(TransferMetaFlags value);
	
	/// <summary>
	/// Name of the data type. This can be the name of any substructure or a static predefined type.
	/// </summary>
	private string m_Type = string.Empty;
	
	/// <summary>
	/// Name of the data type. This can be the name of any substructure or a static predefined type.
	/// </summary>
	public string Type
	{
		get => m_Type;
		set
		{
			m_Type = value;
			OnTypeAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="Type"/> is set.
	/// </summary>
	partial void OnTypeAssignment(string value);
	
	/// <summary>
	/// Name of the field.
	/// </summary>
	private string m_Name = string.Empty;
	
	/// <summary>
	/// Name of the field.
	/// </summary>
	public string Name
	{
		get => m_Name;
		set
		{
			m_Name = value;
			OnNameAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="Name"/> is set.
	/// </summary>
	partial void OnNameAssignment(string value);
	
	public ulong RefTypeHash
	{
		get => default;
		set { }
	}
	
	public void Read(EndianReader reader)
	{
		m_Version = reader.ReadUInt16();
		m_Level = reader.ReadByte();
		m_TypeFlags = reader.ReadByte();
		m_TypeStrOffset = reader.ReadUInt32();
		m_NameStrOffset = reader.ReadUInt32();
		m_ByteSize = reader.ReadInt32();
		m_Index = reader.ReadInt32();
		m_MetaFlag = reader.ReadUInt32();
		OnReadFinished(reader);
	}
	
	/// <summary>
	/// Called when <see cref="Read"/> is finished.
	/// </summary>
	partial void OnReadFinished(EndianReader reader);
	
	public void Write(EndianWriter writer)
	{
		writer.Write(m_Version);
		writer.Write(m_Level);
		writer.Write(m_TypeFlags);
		writer.Write(m_TypeStrOffset);
		writer.Write(m_NameStrOffset);
		writer.Write(m_ByteSize);
		writer.Write(m_Index);
		writer.Write(m_MetaFlag);
		OnWriteFinished(writer);
	}
	
	/// <summary>
	/// Called when <see cref="Write"/> is finished.
	/// </summary>
	partial void OnWriteFinished(EndianWriter writer);
	
}
