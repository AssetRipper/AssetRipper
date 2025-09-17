using AssetRipper.AssemblyDumper.Utils;
using AssetRipper.Numerics;
using System.Diagnostics;

namespace AssetRipper.AssemblyDumper.Passes;

internal static partial class Pass007_ExtractSubclasses
{
	private readonly struct SubclassCandidate
	{
		public readonly UniversalNode? ReleaseNode;
		public readonly UniversalNode? EditorNode;
		public readonly string Name;
		public readonly DiscontinuousRange<UnityVersion> VersionRange;
		public readonly UniversalNode[] NodesToBeAltered;

		public SubclassCandidate(UniversalNode? releaseNode, UniversalNode? editorNode, Range<UnityVersion> versionRange) : this()
		{
			if (releaseNode is null && editorNode is null)
			{
				throw new Exception("Release and editor can't both be null");
			}

			ReleaseNode = releaseNode;
			EditorNode = editorNode;
			VersionRange = versionRange;
			Name = releaseNode?.TypeName ?? editorNode?.TypeName ?? throw new Exception("All type names were null");
			if (releaseNode is null)
			{
				Debug.Assert(editorNode is not null);
				NodesToBeAltered = [editorNode];
			}
			else if (editorNode is null)
			{
				NodesToBeAltered = [releaseNode];
			}
			else
			{
				NodesToBeAltered = [releaseNode, editorNode];
			}
		}

		private SubclassCandidate(UniversalNode? releaseNode, UniversalNode? editorNode, string name, DiscontinuousRange<UnityVersion> versionRange, UniversalNode[] nodesToBeAltered)
		{
			if (releaseNode is null && editorNode is null)
			{
				throw new ArgumentException("Release and editor can't both be null");
			}

			ReleaseNode = releaseNode;
			EditorNode = editorNode;
			Name = name;
			VersionRange = versionRange;
			NodesToBeAltered = nodesToBeAltered;
		}

		public bool Contains(SubclassCandidate candidate)
		{
			return Name == candidate.Name &&
				VersionRange.Contains(candidate.VersionRange) &&
				(candidate.ReleaseNode is null || UniversalNodeComparer.Equals(ReleaseNode, candidate.ReleaseNode, true)) &&
				(candidate.EditorNode is null || UniversalNodeComparer.Equals(EditorNode, candidate.EditorNode, true));
		}

		public bool CanMerge(SubclassCandidate candidate)
		{
			return Name == candidate.Name &&
				UniversalNodeComparer.Equals(ReleaseNode, candidate.ReleaseNode, true) &&
				UniversalNodeComparer.Equals(EditorNode, candidate.EditorNode, true);
		}

		public bool CanMergeRelaxed(SubclassCandidate candidate)
		{
			return Name == candidate.Name &&
				(candidate.ReleaseNode is null || ReleaseNode is null || UniversalNodeComparer.Equals(ReleaseNode, candidate.ReleaseNode, true)) &&
				(candidate.EditorNode is null || EditorNode is null || UniversalNodeComparer.Equals(EditorNode, candidate.EditorNode, true));
		}

		public SubclassCandidate Merge(SubclassCandidate candidate)
		{
			UniversalNode[] nodes = new UniversalNode[NodesToBeAltered.Length + candidate.NodesToBeAltered.Length];
			Array.Copy(NodesToBeAltered, nodes, NodesToBeAltered.Length);
			Array.Copy(candidate.NodesToBeAltered, 0, nodes, NodesToBeAltered.Length, candidate.NodesToBeAltered.Length);
			DiscontinuousRange<UnityVersion> range = VersionRange.Union(candidate.VersionRange);
			return new SubclassCandidate(ReleaseNode ?? candidate.ReleaseNode, EditorNode ?? candidate.EditorNode, Name, range, nodes);
		}

		internal UniversalClass ToUniversalClass()
		{
			return new UniversalClass(ReleaseNode?.ShallowCloneAsRootNode(), EditorNode?.ShallowCloneAsRootNode());
		}

		public override string ToString()
		{
			return $"{Name} ({VersionRange})";
		}
	}

	private static void AddClassesToSharedStateSubclasses(List<SubclassCandidate> unprocessedList)
	{
		List<SubclassCandidate> consolidatedCandidates = ProcessList(unprocessedList);
		if (consolidatedCandidates.Count == 0)
		{
			throw new Exception("Candidate count can't be zero");
		}
		else if (consolidatedCandidates.Count == 1)
		{
			//Single class, don't change the name
			SubclassCandidate candidate = consolidatedCandidates[0];
			VersionedList<UniversalClass> classList = new();
			SharedState.Instance.SubclassInformation.Add(candidate.Name, classList);
			DiscontinuousRange<UnityVersion> versionRange = candidate.VersionRange;
			classList.Add(versionRange[0].Start, candidate.ToUniversalClass());
			UnityVersion end = versionRange[versionRange.Count - 1].End;
			if (end != UnityVersion.MaxVersion)
			{
				classList.Add(end, null);
			}
		}
		else if (AnyIntersections(consolidatedCandidates))
		{
			Console.WriteLine($"Using conflict naming for {consolidatedCandidates[0].Name}");
			//Use _2 naming
			for (int i = 0; i < consolidatedCandidates.Count; i++)
			{
				SubclassCandidate candidate = consolidatedCandidates[i];
				string typeName = $"{candidate.Name}_{i}";
				VersionedList<UniversalClass> classList = new();
				SharedState.Instance.SubclassInformation.Add(typeName, classList);
				foreach (UniversalNode node in candidate.NodesToBeAltered)
				{
					node.TypeName = typeName;
				}
				DiscontinuousRange<UnityVersion> versionRange = candidate.VersionRange;
				classList.Add(versionRange[0].Start, candidate.ToUniversalClass());
				UnityVersion end = versionRange[versionRange.Count - 1].End;
				if (end != UnityVersion.MaxVersion)
				{
					classList.Add(end, null);
				}
				else if (candidate.Name.StartsWith("PPtr"))
				{
					Console.WriteLine($"{candidate.Name} has unresolved conflicts and has no end version");
				}
			}
		}
		else
		{
			//Use _3_4_0f5 naming
			VersionedList<UniversalClass> classList = new();
			SharedState.Instance.SubclassInformation.Add(consolidatedCandidates[0].Name, classList);

			HashSet<UnityVersion> startSet = new();
			HashSet<UnityVersion> combinedSet = new();
			foreach (SubclassCandidate candidate in consolidatedCandidates)
			{
				foreach (UnityVersionRange range in candidate.VersionRange)
				{
					startSet.Add(range.Start);
					combinedSet.Add(range.Start);
					combinedSet.Add(range.End);
				}
			}

			List<(UnityVersion, SubclassCandidate?)> orderedVersions = new(combinedSet.Count);
			foreach (UnityVersion version in combinedSet.Order())
			{
				if (startSet.Contains(version))
				{
					SubclassCandidate? candidate = consolidatedCandidates.First(c => c.VersionRange.Contains(version));
					orderedVersions.Add((version, candidate));
				}
				else if (version != UnityVersion.MaxVersion)
				{
					orderedVersions.Add((version, null));
				}
			}
			SubclassCandidate? previousCandidate = default;//Always not null
			for (int i = 0; i < orderedVersions.Count; i++)
			{
				(UnityVersion version, SubclassCandidate? candidate) = orderedVersions[i];
				bool shouldAdd;
				if (i == 0)
				{
					//This is the first candidate.
					Debug.Assert(previousCandidate is null);
					shouldAdd = true;
					previousCandidate = candidate;
				}
				else if (candidate is null)
				{
					Debug.Assert(previousCandidate is not null);
					if (i < orderedVersions.Count - 1)
					{
						//This is null and not the end. Therefore, it's between two not null candidates.
						//If those candidates are the same, we don't add this.
						//The array is used for an efficient reference equality comparison.
						shouldAdd = orderedVersions[i + 1].Item2?.NodesToBeAltered != previousCandidate?.NodesToBeAltered;
					}
					else
					{
						//This is the last version.
						shouldAdd = true;
					}
				}
				else
				{
					//We only add if this candidate is different from the previous one.
					//The array is used for an efficient reference equality comparison.
					Debug.Assert(previousCandidate is not null);
					shouldAdd = candidate?.NodesToBeAltered != previousCandidate?.NodesToBeAltered;
					previousCandidate = candidate;
				}
				if (shouldAdd)
				{
					classList.Add(version, candidate?.ToUniversalClass());
				}
			}
		}
	}

	private static bool AnyIntersections(List<SubclassCandidate> consolidatedCandidates)
	{
		for (int i = 0; i < consolidatedCandidates.Count; i++)
		{
			for (int j = i + 1; j < consolidatedCandidates.Count; j++)
			{
				if (consolidatedCandidates[i].VersionRange.Intersects(consolidatedCandidates[j].VersionRange))
				{
					Console.WriteLine($"{consolidatedCandidates[i].VersionRange} intersects with {consolidatedCandidates[j].VersionRange}");
					return true;
				}
			}
		}
		return false;
	}

	private static List<SubclassCandidate> ProcessList(List<SubclassCandidate> unprocessedList)
	{
		SplitByNullability(unprocessedList, out List<SubclassCandidate> bothList, out List<SubclassCandidate> releaseList, out List<SubclassCandidate> editorList);
		bothList = GetConsolidatedList(bothList);
		MergeIntoBothList(bothList, releaseList);
		MergeIntoBothList(bothList, editorList);
		releaseList = GetConsolidatedList(releaseList);
		editorList = GetConsolidatedList(editorList);
		FinalMergeAttempts(bothList, releaseList, editorList);
		List<SubclassCandidate> unifiedList = new List<SubclassCandidate>(bothList.Count + releaseList.Count + editorList.Count);
		unifiedList.AddRange(bothList);
		unifiedList.AddRange(releaseList);
		unifiedList.AddRange(editorList);
		return unifiedList;
	}

	private static void SplitByNullability(List<SubclassCandidate> inputList,
		out List<SubclassCandidate> bothList,
		out List<SubclassCandidate> releaseList,
		out List<SubclassCandidate> editorList)
	{
		bothList = inputList.Where(c => c.EditorNode is not null && c.ReleaseNode is not null).ToList();
		releaseList = inputList.Where(c => c.EditorNode is null && c.ReleaseNode is not null).ToList();
		editorList = inputList.Where(c => c.EditorNode is not null && c.ReleaseNode is null).ToList();
	}

	private static bool TryMergeClasses(List<SubclassCandidate> inputList, out List<SubclassCandidate> outputList)
	{
		outputList = new();
		bool result = false;
		foreach (SubclassCandidate candidate in inputList)
		{
			bool merged = false;
			for (int i = 0; i < outputList.Count; i++)
			{
				if (outputList[i].CanMerge(candidate))
				{
					result = true;
					merged = true;
					outputList[i] = outputList[i].Merge(candidate);
					break;
				}
			}
			if (!merged)
			{
				outputList.Add(candidate);
			}
		}
		return result;
	}

	private static List<SubclassCandidate> GetConsolidatedList(List<SubclassCandidate> inputList)
	{
		List<SubclassCandidate> outputList = inputList;
		bool shouldTryAgain = true;
		while (shouldTryAgain)
		{
			shouldTryAgain = TryMergeClasses(outputList, out List<SubclassCandidate> newOutput);
			outputList = newOutput;
		}
		return outputList;
	}

	private static void MergeIntoBothList(List<SubclassCandidate> bothList, List<SubclassCandidate> singleList)
	{
		List<SubclassCandidate> leftovers = new();
		foreach (SubclassCandidate singleCandidate in singleList)
		{
			bool successful = false;
			for (int i = 0; i < bothList.Count; i++)
			{
				if (bothList[i].Contains(singleCandidate))
				{
					successful = true;
					bothList[i] = bothList[i].Merge(singleCandidate);
					break;
				}
			}
			if (!successful)
			{
				leftovers.Add(singleCandidate);
			}
		}
		singleList.Clear();
		singleList.Capacity = leftovers.Count;
		singleList.AddRange(leftovers);
	}

	private static void FinalMergeAttempts(List<SubclassCandidate> bothList, List<SubclassCandidate> releaseList, List<SubclassCandidate> editorList)
	{
		if (bothList.Count == 0)
		{
			if (releaseList.Count == 1 && editorList.Count == 1)
			{
				SubclassCandidate releaseCandidate = releaseList[0];
				SubclassCandidate editorCandidate = editorList[0];
				if (releaseCandidate.Name == editorCandidate.Name
					&& AreCompatible(releaseCandidate.ReleaseNode!, editorCandidate.EditorNode!, true))
				{
					SubclassCandidate mergedCandidate = releaseCandidate.Merge(editorCandidate);
					releaseList.Clear();
					editorList.Clear();
					bothList.Add(mergedCandidate);
				}
			}
		}
		else if (bothList.Count == 1)
		{
			if (releaseList.Count == 1 && editorList.Count == 0)
			{
				SubclassCandidate releaseCandidate = releaseList[0];
				SubclassCandidate bothCandidate = bothList[0];
				if (bothCandidate.CanMergeRelaxed(releaseCandidate))
				{
					SubclassCandidate mergedCandidate = bothCandidate.Merge(releaseCandidate);
					releaseList.Clear();
					bothList[0] = mergedCandidate;
				}
			}
			else if (releaseList.Count == 0 && editorList.Count == 1)
			{
				SubclassCandidate editorCandidate = editorList[0];
				SubclassCandidate bothCandidate = bothList[0];
				if (bothCandidate.CanMergeRelaxed(editorCandidate))
				{
					SubclassCandidate mergedCandidate = bothCandidate.Merge(editorCandidate);
					editorList.Clear();
					bothList[0] = mergedCandidate;
				}
			}
		}
	}
}
