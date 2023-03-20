// Auto-generated code. Do not modify manually.
using AssetRipper.IO.Endian;
namespace AssetRipper.IO.Files.SerializedFiles.TypeTrees;
public partial interface ITypeTreeNode : IEndianWritable
{
	/// <summary>
	/// Name of the data type. This can be the name of any substructure or a static predefined type.
	/// </summary>
	public string Type { get; set; }
	
	/// <summary>
	/// Name of the field.
	/// </summary>
	public string Name { get; set; }
	
	/// <summary>
	/// Size of the data value in bytes, e.g. 4 for int. -1 means that there is an array somewhere inside the hierarchy.
	/// </summary>
	/// <remarks>
	/// Note: The padding for the alignment is not included in the size.
	/// </remarks>
	public int ByteSize { get; set; }
	
	/// <summary>
	/// Index of the field that is unique within a tree.
	/// </summary>
	/// <remarks>
	/// Normally starts with 0 and is incremented with each additional field.
	/// </remarks>
	public int Index { get; set; }
	
	/// <summary>
	/// Array flag, set to 1 if type is "Array" or "TypelessData".
	/// </summary>
	public int TypeFlags { get; set; }
	
	/// <summary>
	/// Field type version, starts with 1 and is incremented after the type information has been significantly updated in a new release.
	/// </summary>
	/// <remarks>
	/// Equal to serializedVersion in Yaml format files
	/// </remarks>
	public int Version { get; set; }
	
	/// <summary>
	/// Meta flags of the field.
	/// </summary>
	public TransferMetaFlags MetaFlag { get; set; }
	
	/// <summary>
	/// Depth of current type relative to root.
	/// </summary>
	public byte Level { get; set; }
	
	/// <summary>
	/// Type offset in the string buffer.
	/// </summary>
	public uint TypeStrOffset { get; set; }
	
	/// <summary>
	/// Name offset in the string buffer.
	/// </summary>
	public uint NameStrOffset { get; set; }
	
	public ulong RefTypeHash { get; set; }
	
}
