using System.Buffers.Binary;

namespace AssetRipper.Addressables;

public readonly record struct Entry(
	int InternalId,
	int ProviderIndex,
	int DependencyKeyIndex,
	int DependencyHash,
	int DataIndex,
	int PrimaryKey,
	int ResourceType,
	object? Data)
{
	public const int Size = 7 * sizeof(int);

	public static Entry Read(ReadOnlySpan<byte> entryData, byte[] extraData)
	{
		int internalId = BinaryPrimitives.ReadInt32LittleEndian(entryData);
		int providerIndex = BinaryPrimitives.ReadInt32LittleEndian(entryData[sizeof(int)..]);
		int dependencyKeyIndex = BinaryPrimitives.ReadInt32LittleEndian(entryData[(2 * sizeof(int))..]);
		int depHash = BinaryPrimitives.ReadInt32LittleEndian(entryData[(3 * sizeof(int))..]);
		int dataIndex = BinaryPrimitives.ReadInt32LittleEndian(entryData[(4 * sizeof(int))..]);
		int primaryKey = BinaryPrimitives.ReadInt32LittleEndian(entryData[(5 * sizeof(int))..]);
		int resourceType = BinaryPrimitives.ReadInt32LittleEndian(entryData[(6 * sizeof(int))..]);
		object? data;
		if (dataIndex < 0)
		{
			data = null;
		}
		else
		{
			Serialization.ReadObjectFromData(extraData, dataIndex, out _, out data);
		}
		return new Entry(internalId, providerIndex, dependencyKeyIndex, depHash, dataIndex, primaryKey, resourceType, data);
	}
}
