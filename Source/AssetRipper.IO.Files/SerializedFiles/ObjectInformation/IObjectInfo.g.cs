// Auto-generated code. Do not modify manually.
using AssetRipper.IO.Endian;
namespace AssetRipper.IO.Files.SerializedFiles.ObjectInformation;
/// <summary>
/// Contains information for a block of raw serialized object data.
/// </summary>
public partial interface IObjectInfo : IEndianWritable
{
	/// <summary>
	/// ObjectID
	/// </summary>
	/// <remarks>
	/// Unique ID that identifies the object. Can be used as a key for a map.
	/// </remarks>
	public long FileID { get; set; }
	
	/// <summary>
	/// Offset to the object data.
	/// </summary>
	/// <remarks>
	/// Add to <see cref="SerializedFileHeader.DataOffset"/> to get the absolute offset within the serialized file.
	/// </remarks>
	public long ByteStart { get; set; }
	
	/// <summary>
	/// Size of the object data.
	/// </summary>
	public int ByteSize { get; set; }
	
	/// <summary>
	/// Type ID of the object, which is mapped to <see cref="SerializedType.TypeID"/>. Equals to classID if the object is not <see cref="ClassIDType.MonoBehaviour"/>
	/// </summary>
	public int TypeID { get; set; }
	
	/// <summary>
	/// Class ID of the object.
	/// </summary>
	public short ClassID { get; set; }
	
	public ushort IsDestroyed { get; set; }
	
	public short ScriptTypeIndex { get; set; }
	
	public bool Stripped { get; set; }
	
	/// <summary>
	/// Type index in <see cref="SerializedFileMetadata.Types"/> array.
	/// </summary>
	public int SerializedTypeIndex { get; set; }
	
}
