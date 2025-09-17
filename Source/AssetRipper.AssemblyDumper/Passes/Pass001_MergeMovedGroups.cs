using AssetRipper.AssemblyDumper.Utils;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass001_MergeMovedGroups
{
	public static IReadOnlyDictionary<int, IReadOnlyList<int>> Changes { get; } = new Dictionary<int, IReadOnlyList<int>>
	{
		{ 238, new int[] { 194 } },//NavMesh
		{ 258, new int[] { 197 } },//LightProbes
		{ 319, new int[] { 1011 } },//AvatarMask
		{ 329, new int[] { 327 } },//VideoClip
	};

	public static void DoPass()
	{
		// Fix old NavMesh before merging
		{
			// This prevents a conflict with the new NavMesh class.
			// Between versions 5.0 and 5.3, they both existed, but the old one had no data.
			// We're removing that section to prevent any issues.
			// In the AssetFactory, we'll return null for 194 on version 5.0 and higher.

			VersionedList<UniversalClass> versionedList = SharedState.Instance.ClassInformation[194];

			versionedList.Pop();
			versionedList.Pop();

			versionedList.Add(new UnityVersion(5), null);
		}

		foreach ((int mainID, IReadOnlyList<int> idList) in Changes)
		{
			VersionedList<UniversalClass> versionedList = SharedState.Instance.ClassInformation[mainID];
			foreach (int id in idList)
			{
				VersionedList<UniversalClass> otherVersionedList = SharedState.Instance.ClassInformation[id];
				versionedList = VersionedList.Merge(versionedList, otherVersionedList);
				SharedState.Instance.ClassInformation.Remove(id);
			}
			foreach (UniversalClass? universalClass in versionedList.Values)
			{
				if (universalClass is not null)
				{
					universalClass.TypeID = mainID;
				}
			}
			SharedState.Instance.ClassInformation[mainID] = versionedList;
		}
	}
}
