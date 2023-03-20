// Auto-generated code. Do not modify manually.
using AssetRipper.IO.Endian;
namespace AssetRipper.IO.Files.SerializedFiles.Header;
/// <summary>
/// The file header is found at the beginning of an asset file. The header is always using big endian byte order.
/// </summary>
public partial interface ISerializedFileHeader : IEndianWritable
{
	/// <summary>
	/// Size of the metadata section in the file
	/// </summary>
	public long MetadataSize { get; set; }
	
	/// <summary>
	/// Size of the whole file
	/// </summary>
	public long FileSize { get; set; }
	
	/// <summary>
	/// File format version. The number is required for backward compatibility and is normally incremented after the file format has been changed in a major update.
	/// </summary>
	public FormatVersion Version { get; set; }
	
	/// <summary>
	/// Offset to the serialized object data. It starts at the data for the first object.
	/// </summary>
	public long DataOffset { get; set; }
	
	/// <summary>
	/// This controls the byte order of the data structure. False is little endian. True is big endian.
	/// </summary>
	/// <remarks>
	/// This field is normally set to 0.
	/// </remarks>
	public bool SwapEndianess { get; set; }
	
	/// <summary>
	/// An unknown field introduced in version 22.
	/// </summary>
	public long Unknown22 { get; set; }
	
}
