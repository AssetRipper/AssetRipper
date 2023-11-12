using System.Buffers.Binary;

namespace AssetRipper.Addressables;

public readonly record struct Bucket(int DataOffset, int[] Entries)
{
	/// <summary>
	/// Reads a <paramref name="bucket"/> from a <paramref name="buffer"/>.
	/// </summary>
	/// <param name="buffer">The span of byte data.</param>
	/// <param name="bucket">The new bucket read from the <paramref name="buffer"/>.</param>
	/// <returns>The number of bytes read.</returns>
	public static int Read(ReadOnlySpan<byte> buffer, out Bucket bucket)
	{
		int bi = 0;
		int index = BinaryPrimitives.ReadInt32LittleEndian(buffer);
		bi += 4;
		int entryCount = BinaryPrimitives.ReadInt32LittleEndian(buffer[sizeof(int)..]);
		bi += 4;
		int[] entryArray = new int[entryCount];
		for (int c = 0; c < entryCount; c++)
		{
			entryArray[c] = BinaryPrimitives.ReadInt32LittleEndian(buffer[bi..]);
			bi += 4;
		}
		bucket = new Bucket(index, entryArray);
		return bi;
	}
}
