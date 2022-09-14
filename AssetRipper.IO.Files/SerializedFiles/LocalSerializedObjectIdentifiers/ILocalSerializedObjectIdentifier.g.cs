// Auto-generated code. Do not modify manually.
using AssetRipper.IO.Endian;
namespace AssetRipper.IO.Files.SerializedFiles.LocalSerializedObjectIdentifiers;
public partial interface ILocalSerializedObjectIdentifier : IEndianReadable, IEndianWritable
{
	public int LocalSerializedFileIndex { get; set; }
	
	public long LocalIdentifierInFile { get; set; }
	
}
