// Auto-generated code. Do not modify manually.
using AssetRipper.IO.Endian;
namespace AssetRipper.IO.Files.SerializedFiles.LocalSerializedObjectIdentifiers;
public partial record class LocalSerializedObjectIdentifier_1 : ILocalSerializedObjectIdentifier
{
	private int m_LocalSerializedFileIndex = new();
	
	public int LocalSerializedFileIndex
	{
		get => m_LocalSerializedFileIndex;
		set
		{
			m_LocalSerializedFileIndex = value;
			OnLocalSerializedFileIndexAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="LocalSerializedFileIndex"/> is set.
	/// </summary>
	partial void OnLocalSerializedFileIndexAssignment(int value);
	
	private int m_LocalIdentifierInFile = new();
	
	public long LocalIdentifierInFile
	{
		get => m_LocalIdentifierInFile;
		set
		{
			m_LocalIdentifierInFile = (int)value;
			OnLocalIdentifierInFileAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="LocalIdentifierInFile"/> is set.
	/// </summary>
	partial void OnLocalIdentifierInFileAssignment(long value);
	
	public void Read(EndianReader reader)
	{
		m_LocalSerializedFileIndex = reader.ReadInt32();
		m_LocalIdentifierInFile = reader.ReadInt32();
		OnReadFinished(reader);
	}
	
	/// <summary>
	/// Called when <see cref="Read"/> is finished.
	/// </summary>
	partial void OnReadFinished(EndianReader reader);
	
	public void Write(EndianWriter writer)
	{
		writer.Write(m_LocalSerializedFileIndex);
		writer.Write(m_LocalIdentifierInFile);
		OnWriteFinished(writer);
	}
	
	/// <summary>
	/// Called when <see cref="Write"/> is finished.
	/// </summary>
	partial void OnWriteFinished(EndianWriter writer);
	
}
