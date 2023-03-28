namespace AssetRipper.IO.Endian;

public interface IEndianSpanReadable
{
	void ReadEditor(ref EndianSpanReader reader);
	void ReadRelease(ref EndianSpanReader reader);
}
