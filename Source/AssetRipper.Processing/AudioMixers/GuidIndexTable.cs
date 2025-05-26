using AssetRipper.Import.Logging;

namespace AssetRipper.Processing.AudioMixers;

internal readonly struct GuidIndexTable
{
	private readonly Dictionary<uint, UnityGuid> table = new();

	public GuidIndexTable()
	{
	}

	public UnityGuid this[uint index] => table[index];

	public bool ContainsKey(uint index) => table.ContainsKey(index);

	public bool TryGetValue(uint index, out UnityGuid guid) => table.TryGetValue(index, out guid);

	public UnityGuid IndexNewGuid(uint index)
	{
		if (table.TryGetValue(index, out UnityGuid guid))
		{
			Logger.Warning(LogCategory.Processing, $"Constant index #{index} conflicts with another one.");
		}
		else
		{
			guid = UnityGuid.NewGuid();
			table.Add(index, guid);
		}
		return guid;
	}
}
