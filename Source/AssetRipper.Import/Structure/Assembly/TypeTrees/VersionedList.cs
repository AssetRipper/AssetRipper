namespace AssetRipper.Import.Structure.Assembly.TypeTrees;

internal readonly record struct VersionedList<T>(List<KeyValuePair<UnityVersion, T>> List)
{
	//Convert to extension type when available in C#
	public static implicit operator VersionedList<T>(List<KeyValuePair<UnityVersion, T>> list) => new(list);
	public static implicit operator List<KeyValuePair<UnityVersion, T>>(VersionedList<T> list) => list.List;
	public T? GetItemForVersion(UnityVersion version)
	{
		if (List.Count == 0 || List[0].Key > version)
		{
			return default;
		}

		for (int i = 0; i < List.Count - 1; i++)
		{
			if (List[i].Key <= version && version < List[i + 1].Key)
			{
				return List[i].Value;
			}
		}

		return List[^1].Value;
	}
}
