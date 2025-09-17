using AssetRipper.Numerics;
using AssetRipper.Primitives;
using System.Diagnostics;

namespace AssetRipper.AssemblyDumper.Utils;

public static class VersionedList
{
	public static VersionedList<T> Merge<T>(VersionedList<T> first, VersionedList<T> second) where T : class
	{
		VersionedList<T> result = new();
		int i = 0;
		int j = 0;
		UnityVersion currentVersion;
		if (first.Count == 0)
		{
			if (second.Count == 0)
			{
				currentVersion = UnityVersion.MinVersion;
			}
			else
			{
				currentVersion = second[0].Key;
			}
		}
		else if (second.Count == 0)
		{
			currentVersion = first[0].Key;
		}
		else
		{
			UnityVersion firstMinVersion = first[0].Key;
			UnityVersion secondMinVersion = second[0].Key;
			currentVersion = firstMinVersion < secondMinVersion ? firstMinVersion : secondMinVersion;
		}
		while (true)
		{
			if (i == first.Count)
			{
				if (j == second.Count)
				{
					break;
				}
				else
				{
					result.AddIfDifferent(second[j]);
					j++;
					continue;
				}
			}
			else if (j == second.Count)
			{
				result.AddIfDifferent(first[i]);
				i++;
				continue;
			}

			Range<UnityVersion> firstRange = first.GetRange(i);
			Range<UnityVersion> secondRange = second.GetRange(j);

			if (firstRange.Intersects(secondRange, out Range<UnityVersion> intersection))
			{
				if (intersection.Start == currentVersion)
				{
					T? value = EnsureAtLeastOneNull(first[i].Value, second[j].Value);
					result.AddIfDifferent(intersection.Start, value);
					currentVersion = intersection.End;
					if (firstRange.End == intersection.End)
					{
						i++;
					}
					if (secondRange.End == intersection.End)
					{
						j++;
					}
				}
				else if (firstRange.Start == currentVersion)
				{
					result.AddIfDifferent(currentVersion, first[i].Value);
					currentVersion = intersection.Start;
				}
				else
				{
					Debug.Assert(secondRange.Start == currentVersion);
					result.AddIfDifferent(currentVersion, second[j].Value);
					currentVersion = intersection.Start;
				}
			}
			else if (firstRange.Start == currentVersion)
			{
				result.AddIfDifferent(currentVersion, first[i].Value);
				currentVersion = firstRange.End;
				i++;
			}
			else
			{
				Debug.Assert(secondRange.Start == currentVersion);
				result.AddIfDifferent(currentVersion, second[j].Value);
				currentVersion = secondRange.End;
				j++;
			}
		}
		return result;
	}

	private static void AddIfDifferent<T>(this VersionedList<T> list, KeyValuePair<UnityVersion, T?> pair)
	{
		list.AddIfDifferent(pair.Key, pair.Value);
	}

	private static void AddIfDifferent<T>(this VersionedList<T> list, UnityVersion version, T? value)
	{
		if (list.Count == 0 || !EqualityComparer<T>.Default.Equals(list[^1].Value, value))
		{
			list.Add(version, value);
		}
	}

	private static T? EnsureAtLeastOneNull<T>(T? first, T? second) where T : class
	{
		if (first is not null)
		{
			if (second is not null && !EqualityComparer<T>.Default.Equals(first, second))
			{
				throw new Exception("Both values are not null");
			}
			return first;
		}
		else
		{
			return second;
		}
	}
}
