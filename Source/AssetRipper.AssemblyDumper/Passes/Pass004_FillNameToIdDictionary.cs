using AssetRipper.AssemblyDumper.Utils;

namespace AssetRipper.AssemblyDumper.Passes;

public static class Pass004_FillNameToTypeIdDictionary
{
	public static void DoPass()
	{
		Dictionary<string, HashSet<int>> dictionary = SharedState.Instance.NameToTypeID;

		foreach ((int id, VersionedList<UniversalClass> list) in SharedState.Instance.ClassInformation)
		{
			foreach (UniversalClass? universalClass in list.Values)
			{
				if (universalClass is not null)
				{
					string name = universalClass.Name;
					if (!dictionary.TryGetValue(name, out HashSet<int>? classIds))
					{
						classIds = new HashSet<int>();
						dictionary.Add(name, classIds);
					}
					classIds.Add(id);
				}
			}
		}
	}
}
