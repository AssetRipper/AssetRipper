using AssetRipper.Assets.Generics;
using K4os.Compression.LZ4;

namespace AssetRipper.Export.Modules.Shaders.ShaderBlob;

public readonly record struct DecompressedBlob(byte[] DecompressedData)
{
	public static implicit operator byte[](DecompressedBlob blob) => blob.DecompressedData;
	public static explicit operator DecompressedBlob(byte[] data) => new(data);

	public static DecompressedBlob DecompressBlob(byte[] compressedBlob, uint offset, uint compressedLength, uint decompressedLength)
	{
		DecompressedBlob result = (DecompressedBlob)new byte[decompressedLength];
		int bytesWritten = LZ4Codec.Decode(compressedBlob, (int)offset, (int)compressedLength, result, 0, (int)decompressedLength);
		if (bytesWritten != decompressedLength)
		{
			throw new Exception($"Read {bytesWritten}, which was less than expected {decompressedLength}");
		}

		return result;
	}

	public static DecompressedBlob[] DecompressBlobs(byte[] compressedBlob, AssetList<uint> offsets, AssetList<uint> compressedLengths, AssetList<uint> decompressedLengths)
	{
		DecompressedBlob[] blobs = new DecompressedBlob[offsets.Count];
		for (int i = 0; i < offsets.Count; i++)
		{
			blobs[i] = DecompressBlob(compressedBlob, offsets[i], compressedLengths[i], decompressedLengths[i]);
		}
		return blobs;
	}

	public static DecompressedBlob[][] DecompressBlobs(byte[] compressedBlob, AssetList<AssetList<uint>> offsets, AssetList<AssetList<uint>> compressedLengths, AssetList<AssetList<uint>> decompressedLengths)
	{
		DecompressedBlob[][] blobs = new DecompressedBlob[offsets.Count][];
		for (int i = 0; i < offsets.Count; i++)
		{
			blobs[i] = DecompressBlobs(compressedBlob, offsets[i], compressedLengths[i], decompressedLengths[i]);
		}
		return blobs;
	}
}
