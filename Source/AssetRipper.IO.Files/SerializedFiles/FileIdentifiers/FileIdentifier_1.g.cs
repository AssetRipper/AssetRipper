// Auto-generated code. Do not modify manually.
using AssetRipper.IO.Endian;
using AssetRipper.Primitives;
namespace AssetRipper.IO.Files.SerializedFiles.FileIdentifiers;
/// <summary>
/// A serialized file may be linked with other serialized files to create shared dependencies.
/// </summary>
public partial record class FileIdentifier_1 : IFileIdentifier
{
	/// <summary>
	/// Actual file path. This path is relative to the path of the current file.
	/// </summary>
	/// <remarks>
	/// The folder "library" often needs to be translated to "resources" in order to find the file on the file system.
	/// </remarks>
	private string m_PathName = string.Empty;
	
	/// <summary>
	/// Actual file path. This path is relative to the path of the current file.
	/// </summary>
	/// <remarks>
	/// The folder "library" often needs to be translated to "resources" in order to find the file on the file system.
	/// </remarks>
	public string PathName
	{
		get => m_PathName;
		set
		{
			m_PathName = value;
			OnPathNameAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="PathName"/> is set.
	/// </summary>
	partial void OnPathNameAssignment(string value);
	
	/// <summary>
	/// Actually UnityGuid
	/// </summary>
	public UnityGuid Guid
	{
		get => default;
		set { }
	}
	
	/// <summary>
	/// The type of the file
	/// </summary>
	public AssetType Type
	{
		get => default;
		set { }
	}
	
	/// <summary>
	/// Virtual asset path. Used for cached files, otherwise it's empty.
	/// </summary>
	/// <remarks>
	/// The file with that path usually doesn't exist, so it's probably an alias.
	/// </remarks>
	public string AssetPath
	{
		get => default;
		set { }
	}
	
	public void Read(EndianReader reader)
	{
		m_PathName = reader.ReadStringZeroTerm();
		OnReadFinished(reader);
	}
	
	/// <summary>
	/// Called when <see cref="Read"/> is finished.
	/// </summary>
	partial void OnReadFinished(EndianReader reader);
	
	public void Write(EndianWriter writer)
	{
		writer.WriteStringZeroTerm(m_PathName);
		OnWriteFinished(writer);
	}
	
	/// <summary>
	/// Called when <see cref="Write"/> is finished.
	/// </summary>
	partial void OnWriteFinished(EndianWriter writer);
	
}
