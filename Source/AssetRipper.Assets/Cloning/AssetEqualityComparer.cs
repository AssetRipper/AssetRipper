using AssetRipper.Assets.Metadata;
using System.Diagnostics;

namespace AssetRipper.Assets.Cloning;

public sealed class AssetEqualityComparer : IEqualityComparer<IUnityObjectBase>
{
	private readonly Dictionary<UnorderedPair, bool> compareCache = new();
	private readonly Dictionary<UnorderedPair, List<UnorderedPair>> dependentEqualityPairs = new();
	public IUnityObjectBase CallingObject { get; private set; } = default!;
	public IUnityObjectBase OtherObject { get; private set; } = default!;

	/// <summary>
	/// Used for source generation.
	/// </summary>
	/// <param name="pptrFromCallingObject"></param>
	/// <param name="pptrFromOtherObject"></param>
	/// <returns>True if they're equal, false if they're inequal, or null if it was added to the list of dependent pairs.</returns>
	public bool? MaybeAddDependentComparison(IPPtr pptrFromCallingObject, IPPtr pptrFromOtherObject)
	{
		IUnityObjectBase? x = CallingObject.Collection.TryGetAsset(pptrFromCallingObject.FileID, pptrFromCallingObject.PathID);
		IUnityObjectBase? y = OtherObject.Collection.TryGetAsset(pptrFromOtherObject.FileID, pptrFromOtherObject.PathID);

		if (ReferenceEquals(x, y)) //Both null or both same instance
		{
			return true;
		}
		else if (x is null || y is null || x.GetType() != y.GetType())
		{
			return false;
		}
		else if (compareCache.TryGetValue((x, y), out bool value))
		{
			return value;
		}
		else
		{
			if (dependentEqualityPairs.TryGetValue((CallingObject, OtherObject), out List<UnorderedPair>? list))
			{
				list.Add((x, y));
			}
			else
			{
				list = [(x, y)];
				dependentEqualityPairs.Add((CallingObject, OtherObject), list);
			}
			return null;
		}
	}

	public bool Equals(IUnityObjectBase? x, IUnityObjectBase? y)
	{
		if (ReferenceEquals(x, y)) //Both null or both same instance
		{
			return true;
		}
		else if (x is null || y is null || x.GetType() != y.GetType())
		{
			return false;
		}

		DoComparison(x, y);
		EvaluateDependentEqualityComparisons();
		Debug.Assert(dependentEqualityPairs.Count == 0, "Dependent equality pairs should have been resolved");

		return compareCache[(x, y)];
	}

	private void EvaluateDependentEqualityComparisons()
	{
		if (dependentEqualityPairs.Count == 0)
		{
			return;
		}

		List<UnorderedPair> pairsToCompare = new();
		bool hasChanged;
		do
		{
			hasChanged = false;
			pairsToCompare.Clear();

			foreach ((UnorderedPair keyPair, List<UnorderedPair> list) in dependentEqualityPairs.ToArray())
			{
				for (int i = list.Count - 1; i >= 0; i--)
				{
					UnorderedPair valuePair = list[i];
					if (compareCache.TryGetValue(valuePair, out bool value))
					{
						hasChanged = true;
						if (value)
						{
							list.RemoveAt(i);
						}
						else
						{
							compareCache[keyPair] = false;
							dependentEqualityPairs.Remove(keyPair);
							break;
						}
					}
					else if (!dependentEqualityPairs.ContainsKey(valuePair))
					{
						pairsToCompare.Add(valuePair);
					}
				}
			}

			if (pairsToCompare.Count > 0)
			{
				hasChanged = true;

				foreach (UnorderedPair pair in pairsToCompare)
				{
					DoComparison(pair.First, pair.Second);
				}
			}
		} while (hasChanged);

		if (dependentEqualityPairs.Count > 0)
		{
			foreach ((UnorderedPair keyPair, _) in dependentEqualityPairs)
			{
				compareCache[keyPair] = true;
			}
			dependentEqualityPairs.Clear();
		}
	}

	private void DoComparison(IUnityObjectBase x, IUnityObjectBase y)
	{
		CallingObject = x;
		OtherObject = y;

		bool? result = x.AddToEqualityComparer(y, this);
		if (result is { } value)
		{
			dependentEqualityPairs.Remove((x, y));
			compareCache[(x, y)] = value;
		}

		CallingObject = default!;
		OtherObject = default!;
	}

	public int GetHashCode(IUnityObjectBase? obj)
	{
		if (obj == null)
		{
			return 0;
		}

		return HashCode.Combine(obj.GetType(), obj.GetBestName());
	}

	private readonly record struct UnorderedPair(IUnityObjectBase First, IUnityObjectBase Second)
	{
		public bool Equals(UnorderedPair other)
		{
			return (First == other.First && Second == other.Second) || (First == other.Second && Second == other.First);
		}

		public override int GetHashCode()
		{
			return First.GetHashCode() ^ Second.GetHashCode();
		}

		public static implicit operator UnorderedPair((IUnityObjectBase First, IUnityObjectBase Second) pair)
		{
			return new(pair.First, pair.Second);
		}

		public static implicit operator (IUnityObjectBase, IUnityObjectBase)(UnorderedPair pair)
		{
			return (pair.First, pair.Second);
		}
	}
}
