using AssetRipper.Import.Logging;
using AssetRipper.IO.Files;

namespace AssetRipper.Processing.AudioMixers;

internal readonly struct GuidIndexTable
{
	private readonly Dictionary<uint, UnityGUID> table = new();

	public GuidIndexTable()
	{
	}

	public UnityGUID this[uint index] => table[index];

	public bool ContainsKey(uint index) => table.ContainsKey(index);

	public bool TryGetValue(uint index, out UnityGUID guid) => table.TryGetValue(index, out guid);

	public UnityGUID IndexNewGuid(uint index)
	{
		if (table.TryGetValue(index, out UnityGUID guid))
		{
			Logger.Warning(LogCategory.Processing, $"Constant index #{index} conflicts with another one.");
		}
		else
		{
			guid = UnityGUID.NewGuid();
			table.Add(index, guid);
		}
		return guid;
	}
}
