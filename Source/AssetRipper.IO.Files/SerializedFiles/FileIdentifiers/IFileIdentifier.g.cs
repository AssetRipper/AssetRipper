// Auto-generated code. Do not modify manually.
using AssetRipper.IO.Endian;
using AssetRipper.Primitives;
namespace AssetRipper.IO.Files.SerializedFiles.FileIdentifiers;
/// <summary>
/// A serialized file may be linked with other serialized files to create shared dependencies.
/// </summary>
public partial interface IFileIdentifier : IEndianWritable
{
	/// <summary>
	/// Actual file path. This path is relative to the path of the current file.
	/// </summary>
	/// <remarks>
	/// The folder "library" often needs to be translated to "resources" in order to find the file on the file system.
	/// </remarks>
	public string PathName { get; set; }
	
	/// <summary>
	/// Actually UnityGuid
	/// </summary>
	public UnityGuid Guid { get; set; }
	
	/// <summary>
	/// The type of the file
	/// </summary>
	public AssetType Type { get; set; }
	
	/// <summary>
	/// Virtual asset path. Used for cached files, otherwise it's empty.
	/// </summary>
	/// <remarks>
	/// The file with that path usually doesn't exist, so it's probably an alias.
	/// </remarks>
	public string AssetPath { get; set; }
	
}
