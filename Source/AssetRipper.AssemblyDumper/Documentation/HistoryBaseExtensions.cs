using AssetRipper.DocExtraction.DataStructures;

namespace AssetRipper.AssemblyDumper.Documentation;

internal static class HistoryBaseExtensions
{

	public static IEnumerable<UnityVersionRange> GetVersionRange(this HistoryBase history)
	{
		if (history.Exists.Count == 0)
		{
			yield return default;
		}
		else if (history.Exists.Count == 1)
		{
			yield return new UnityVersionRange(history.Exists[0].Key, UnityVersion.MaxVersion);
		}
		else
		{
			for (int i = 0; i < history.Exists.Count; i++)
			{
				if (history.Exists[i].Value)
				{
					yield return history.Exists.GetRange(i);
				}
			}
		}
	}
}