// Auto-generated code. Do not modify manually.
using AssetRipper.IO.Endian;
using AssetRipper.Primitives;
namespace AssetRipper.IO.Files.SerializedFiles.FileIdentifiers;
/// <summary>
/// A serialized file may be linked with other serialized files to create shared dependencies.
/// </summary>
public partial record class FileIdentifier_6 : IFileIdentifier
{
	/// <summary>
	/// Virtual asset path. Used for cached files, otherwise it's empty.
	/// </summary>
	/// <remarks>
	/// The file with that path usually doesn't exist, so it's probably an alias.
	/// </remarks>
	private string m_AssetPath = string.Empty;
	
	/// <summary>
	/// Virtual asset path. Used for cached files, otherwise it's empty.
	/// </summary>
	/// <remarks>
	/// The file with that path usually doesn't exist, so it's probably an alias.
	/// </remarks>
	public string AssetPath
	{
		get => m_AssetPath;
		set
		{
			m_AssetPath = value;
			OnAssetPathAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="AssetPath"/> is set.
	/// </summary>
	partial void OnAssetPathAssignment(string value);
	
	/// <summary>
	/// Actually UnityGuid
	/// </summary>
	private UnityGuid m_Guid = new();
	
	/// <summary>
	/// Actually UnityGuid
	/// </summary>
	public UnityGuid Guid
	{
		get => m_Guid;
		set
		{
			m_Guid = value;
			OnGuidAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="Guid"/> is set.
	/// </summary>
	partial void OnGuidAssignment(UnityGuid value);
	
	/// <summary>
	/// The type of the file
	/// </summary>
	private int m_Type = new();
	
	/// <summary>
	/// The type of the file
	/// </summary>
	public AssetType Type
	{
		get => (AssetType)m_Type;
		set
		{
			m_Type = (int)value;
			OnTypeAssignment(value);
		}
	}
	
	/// <summary>
	/// Called when <see cref="Type"/> is set.
	/// </summary>
	partial void OnTypeAssignment(AssetType value);
	
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
	
	public void Read(EndianReader reader)
	{
		m_AssetPath = reader.ReadStringZeroTerm();
		m_Guid = reader.ReadUnityGuid();
		m_Type = reader.ReadInt32();
		m_PathName = reader.ReadStringZeroTerm();
		OnReadFinished(reader);
	}
	
	/// <summary>
	/// Called when <see cref="Read"/> is finished.
	/// </summary>
	partial void OnReadFinished(EndianReader reader);
	
	public void Write(EndianWriter writer)
	{
		writer.WriteStringZeroTerm(m_AssetPath);
		writer.Write(m_Guid);
		writer.Write(m_Type);
		writer.WriteStringZeroTerm(m_PathName);
		OnWriteFinished(writer);
	}
	
	/// <summary>
	/// Called when <see cref="Write"/> is finished.
	/// </summary>
	partial void OnWriteFinished(EndianWriter writer);
	
}
