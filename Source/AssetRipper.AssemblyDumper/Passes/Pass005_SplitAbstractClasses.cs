using AssetRipper.AssemblyDumper.Utils;
using RangeClassList = System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<AssetRipper.Numerics.Range<AssetRipper.Primitives.UnityVersion>, AssetRipper.AssemblyDumper.UniversalClass>>;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass005_SplitAbstractClasses
{
	private static readonly HashSet<int> processedClasses = new();
	private const int MaxRunCount = 1000;
	private const float MinimumProportion = 0.7f;

	public static void DoPass()
	{
		//ListAbstractClassIds();
		AssignInheritance();
		DoOtherStuff();
	}

	private static void ListAbstractClassIds()
	{
		HashSet<int> abstractIds = GetAbstractClassIds();
		foreach (int abstractId in abstractIds.OrderBy(i => i))
		{
			VersionedList<UniversalClass> list = SharedState.Instance.ClassInformation[abstractId];
			if (list.All(c => c.Value?.IsAbstract ?? true))
			{
				Console.WriteLine($"\t{abstractId} abstract");
			}
			else
			{
				Console.WriteLine($"\t{abstractId} abstract sometimes");
			}
		}
	}

	private static void DoOtherStuff()
	{
		HashSet<int> abstractIds = GetAbstractClassIds();
		int runCount = 0;
		while (abstractIds.Count > 0)
		{
			foreach (int abstractId in abstractIds.ToList())
			{
				VersionedList<UniversalClass> abstractClassList = SharedState.Instance.ClassInformation[abstractId];
				if (abstractClassList.AnyDerivedAbstractAndUnprocessed())
				{
					continue;
				}
				else
				{
					RangeClassList rangeClassList = abstractClassList.MakeRangeClassList();
					List<Section> sections = rangeClassList.MakeSectionList();

					UnifySectionFields(sections, rangeClassList);

					sections = GetMergedSections(sections);
					sections.ReplaceClassesWithClones();
					sections.FixInheritance();
					sections.ApplyApprovedFields();

					abstractClassList.UpdateWithSectionData(sections);

					abstractIds.Remove(abstractId);
					processedClasses.Add(abstractId);
				}
			}
			runCount++;
			if (runCount >= MaxRunCount)
			{
				throw new Exception("Hit max run count");
			}
		}
	}

	private static void ReplaceClassesWithClones(this List<Section> sectionList)
	{
		foreach (Section section in sectionList)
		{
			UniversalClass? baseClass = section.Class.BaseClass;
			if (baseClass is not null && baseClass.DerivedClasses.Contains(section.Class))
			{
				baseClass.DerivedClasses.Remove(section.Class);
			}
			section.Class = section.Class.DeepClone();
			if (baseClass is not null)
			{
				baseClass.DerivedClasses.Add(section.Class);
			}
		}
	}

	private static void FixInheritance(this List<Section> sectionList)
	{
		foreach (Section section in sectionList)
		{
			section.Class.DerivedClasses.Clear();
			foreach (UniversalClass derivedClass in section.DerivedClasses)
			{
				derivedClass.BaseClass = null;
			}
		}
		foreach (Section section in sectionList)
		{
			foreach (UniversalClass derivedClass in section.DerivedClasses)
			{
				if (derivedClass.BaseClass is null)
				{
					derivedClass.BaseClass = section.Class;
					section.Class.DerivedClasses.Add(derivedClass);
				}
			}
		}
	}

	private static void ApplyApprovedFields(this List<Section> sectionList)
	{
		foreach (Section section in sectionList)
		{
			section.Class.InitializeRootNodes();
			foreach ((_, (UniversalNode? releaseNode, UniversalNode? editorNode)) in section.ApprovedFields)
			{
				if (releaseNode is not null)
				{
					section.Class.ReleaseRootNode!.SubNodes.Add(releaseNode);
				}
				if (editorNode is not null)
				{
					section.Class.EditorRootNode!.SubNodes.Add(editorNode);
				}
			}
		}
	}

	private static void InitializeRootNodes(this UniversalClass abstractClass)
	{
		abstractClass.EditorRootNode ??= new()
		{
			Name = "Base",
			OriginalName = "Base",
			TypeName = abstractClass.Name,
			SubNodes = new(),
			Version = 1,
		};
		abstractClass.ReleaseRootNode ??= new()
		{
			Name = "Base",
			OriginalName = "Base",
			TypeName = abstractClass.Name,
			SubNodes = new(),
			Version = 1,
		};
	}

	private static void UpdateWithSectionData(this VersionedList<UniversalClass> versionedList, List<Section> sectionList)
	{
		List<KeyValuePair<UnityVersion, UniversalClass?>> originalList = versionedList.ToList();
		versionedList.Clear();
		int i = 0, j = 0;
		while (i < originalList.Count || j < sectionList.Count)
		{
			(UnityVersion originalStart, UniversalClass? originalClass) = originalList[i];
			if (originalClass is null || !originalClass.IsAbstract)
			{
				versionedList.Add(originalStart, originalClass);
				i++;
			}
			else
			{
				UnityVersion originalEnd = i == originalList.Count - 1 ? UnityVersion.MaxVersion : originalList[i + 1].Key;
				Section currentSection = sectionList[j];
				if (originalEnd <= currentSection.Range.Start)
				{
					i++;
				}
				else if (originalStart <= currentSection.Range.Start && currentSection.Range.End <= originalEnd)
				{
					versionedList.Add(currentSection.Range.Start, currentSection.Class);
					j++;
					if (originalEnd == currentSection.Range.End)
					{
						i++;
					}
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
		}
	}

	private static List<Section> GetMergedSections(List<Section> sectionList)
	{
		List<Section> mergedSections = new();
		Section currentSection = sectionList[0].Clone();

		for (int i = 1; i < sectionList.Count; i++)
		{
			Section nextSection = sectionList[i];
			if (CanBeMerged(currentSection, nextSection))
			{
				currentSection.Range = currentSection.Range.MakeUnion(nextSection.Range);
				currentSection.DerivedClasses.AddRange(nextSection.DerivedClasses);
			}
			else
			{
				mergedSections.Add(currentSection);
				currentSection = nextSection.Clone();
			}
		}
		mergedSections.Add(currentSection);

		return mergedSections;
	}

	private static void UnifySectionFields(List<Section> sections, RangeClassList rangeClassList)
	{
		foreach (string fieldName in rangeClassList.GetAllFieldNames())
		{
			int count = 0;
			double proportionSum = 0;
			foreach (Section section in sections)
			{
				if (section.HasField(fieldName, out float proportion))
				{
					count++;
					proportionSum += proportion;
				}
			}

			float averageProportion = (float)(proportionSum / count);
			if (averageProportion <= MinimumProportion)
			{
				continue;
			}

			bool useField = true;
			Dictionary<Section, (UniversalNode?, UniversalNode?)> nodeDictionary = new();
			foreach (Section section in sections)
			{
				if (!useField)
				{
					break;
				}
				UniversalNode? releaseNode = section.DerivedClasses
					.SelectMany(c => c.ReleaseRootNode?.SubNodes ?? Enumerable.Empty<UniversalNode>())
					.FirstOrDefault(n => n.Name == fieldName);
				if (releaseNode is not null)
				{
					foreach (UniversalClass derivedClass in section.DerivedClasses)
					{
						if (derivedClass.ReleaseRootNode is not null
							&& derivedClass.ReleaseRootNode.TryGetSubNodeByName(fieldName, out UniversalNode? derivedNode)
							&& !UniversalNodeComparer.Equals(releaseNode, derivedNode, false))
						{
							useField = false;
							break;
						}
					}
				}
				UniversalNode? editorNode = section.DerivedClasses
					.SelectMany(c => c.EditorRootNode?.SubNodes ?? Enumerable.Empty<UniversalNode>())
					.FirstOrDefault(n => n.Name == fieldName);
				if (editorNode is not null)
				{
					foreach (UniversalClass derivedClass in section.DerivedClasses)
					{
						if (derivedClass.EditorRootNode is not null
							&& derivedClass.EditorRootNode.TryGetSubNodeByName(fieldName, out UniversalNode? derivedNode)
							&& !UniversalNodeComparer.Equals(editorNode, derivedNode, false))
						{
							useField = false;
							break;
						}
					}
				}

				nodeDictionary.Add(section, (releaseNode, editorNode));
			}

			if (useField)
			{
				foreach ((Section section, (UniversalNode?, UniversalNode?) pair) in nodeDictionary)
				{
					section.ApprovedFields.Add(fieldName, pair);
				}
			}
		}
	}

	private static List<Section> MakeSectionList(this RangeClassList rangeClassList)
	{
		List<UnityVersion> versions = rangeClassList.GetAllUnityVersions();
		Dictionary<UniversalClass, UnityVersionRange> derivedRangeDictionary = rangeClassList
						.SelectMany(pair => pair.Value.DerivedClasses)
						.Distinct()
						.ToDictionary(derived => derived, derived => GetRangeForClass(derived));
		List<Section> sections = new();

		int i = 0, j = 0;
		while (i < rangeClassList.Count && j < versions.Count)
		{
			UnityVersionRange originalRange = rangeClassList[i].Key;
			UnityVersion currentStart = versions[j];
			UnityVersion currentEnd = j == versions.Count - 1 ? UnityVersion.MaxVersion : versions[j + 1];
			if (currentEnd <= originalRange.Start)
			{
				j++;
			}
			else if (originalRange.End <= currentStart)
			{
				i++;
			}
			else if (originalRange.Start <= currentStart && currentEnd <= originalRange.End)
			{
				sections.Add(Section.Create(rangeClassList[i].Value, new UnityVersionRange(currentStart, currentEnd), derivedRangeDictionary));
				j++;
			}
			else
			{
				throw new InvalidOperationException();
			}
		}
		return sections;
	}

	private static HashSet<string> GetAllFieldNames(this RangeClassList rangeClassList)
	{
		HashSet<string> fieldNames = new();
		foreach ((_, UniversalClass universalClass) in rangeClassList)
		{
			foreach (UniversalClass derivedClass in universalClass.DerivedClasses)
			{
				if (derivedClass.ReleaseRootNode is not null)
				{
					foreach (UniversalNode subnode in derivedClass.ReleaseRootNode.SubNodes)
					{
						if (universalClass.ReleaseRootNode is null || !universalClass.ReleaseRootNode.TryGetSubNodeByName(subnode.Name, out _))
						{
							fieldNames.Add(subnode.Name);
						}
					}
				}
				if (derivedClass.EditorRootNode is not null)
				{
					foreach (UniversalNode subnode in derivedClass.EditorRootNode.SubNodes)
					{
						if (universalClass.EditorRootNode is null || !universalClass.EditorRootNode.TryGetSubNodeByName(subnode.Name, out _))
						{
							fieldNames.Add(subnode.Name);
						}
					}
				}
			}
		}
		return fieldNames;
	}

	private static List<UnityVersion> GetAllUnityVersions(this RangeClassList rangeClassList)
	{
		UnityVersion minimumVersion = rangeClassList[0].Key.Start;
		UnityVersion maximumVersion = rangeClassList[rangeClassList.Count - 1].Key.End;
		HashSet<UnityVersion> versionHashSet = new();
		foreach ((UnityVersionRange range, UniversalClass universalClass) in rangeClassList)
		{
			versionHashSet.Add(range.Start);
			versionHashSet.Add(range.End);
			foreach (UniversalClass derivedClass in universalClass.DerivedClasses)
			{
				UnityVersionRange derivedRange = GetRangeForClass(derivedClass);
				if (minimumVersion < derivedRange.Start)
				{
					versionHashSet.Add(derivedRange.Start);
				}
				if (derivedRange.End < maximumVersion)
				{
					versionHashSet.Add(derivedRange.End);
				}
			}
		}
		List<UnityVersion> versionList = new List<UnityVersion>(versionHashSet.Count);
		versionList.AddRange(versionHashSet);
		versionList.Sort();
		return versionList;
	}

	private static UnityVersionRange GetRangeForClass(UniversalClass universalClass)
	{
		return SharedState.Instance.ClassInformation[universalClass.TypeID].GetRangeForItem(universalClass);
	}

	private static RangeClassList MakeRangeClassList(this VersionedList<UniversalClass> abstractClassList)
	{
		RangeClassList result = new();
		for (int i = 0; i < abstractClassList.Count; i++)
		{
			UniversalClass? universalClass = abstractClassList[i].Value;
			if (universalClass is not null && universalClass.IsAbstract)
			{
				result.Add(new KeyValuePair<UnityVersionRange, UniversalClass>(abstractClassList.GetRange(i), universalClass));
			}
		}
		return result;
	}

	private static bool AnyDerivedAbstractAndUnprocessed(this VersionedList<UniversalClass> abstractClassList)
	{
		return abstractClassList
			.Select(pair => pair.Value)
			.Where(universalClass => universalClass is not null && universalClass.IsAbstract)
			.SelectMany(universalClass => universalClass!.DerivedClasses)
			.Any(derivedClass => derivedClass.IsAbstract && !processedClasses.Contains(derivedClass.TypeID));
	}

	private static HashSet<int> GetAbstractClassIds()
	{
		return SharedState.Instance.ClassInformation
			.Where(dictPair => dictPair.Value.Any(listPair => listPair.Value?.IsAbstract ?? false))
			.Select(pair => pair.Key)
			.ToHashSet();
	}

	private static void AssignInheritance()
	{
		foreach ((_, VersionedList<UniversalClass> list) in SharedState.Instance.ClassInformation)
		{
			foreach ((UnityVersion startVersion, UniversalClass? universalClass) in list)
			{
				if (!string.IsNullOrEmpty(universalClass?.BaseString))
				{
					UniversalClass baseClass = GetClass(universalClass.BaseString, startVersion);
					universalClass.BaseClass = baseClass;
					baseClass.DerivedClasses.Add(universalClass);
				}
			}
		}
	}

	private static UniversalClass GetClass(string name, UnityVersion version)
	{
		return SharedState.Instance.NameToTypeID[name]
			.Select(id => SharedState.Instance.ClassInformation[id].TryFindMatch(name, version))
			.Where(c => c is not null)
			.Single()!;
	}

	private static UniversalClass? TryFindMatch(this VersionedList<UniversalClass> list, string name, UnityVersion version)
	{
		UniversalClass? result = list.GetItemForVersion(version);
		return result is not null && result.Name == name ? result : null;
	}

	private static bool CanBeMerged(Section section1, Section section2)
	{
		if (!section1.Class.Equals(section2.Class))
		{
			return false;
		}
		else if (!section1.Range.CanUnion(section2.Range))
		{
			return false;
		}
		else if (section1.ApprovedFields.Count != section2.ApprovedFields.Count)
		{
			return false;
		}

		foreach ((string fieldName, (UniversalNode? releaseNode1, UniversalNode? editorNode1)) in section1.ApprovedFields)
		{
			if (section2.ApprovedFields.TryGetValue(fieldName, out (UniversalNode?, UniversalNode?) pair2))
			{
				if (!UniversalNodeComparer.Equals(releaseNode1, pair2.Item1, false))
				{
					return false;
				}
				if (!UniversalNodeComparer.Equals(editorNode1, pair2.Item2, false))
				{
					return false;
				}
			}
		}

		return true;
	}

	private static void AddRange<T>(this HashSet<T> hashset, IEnumerable<T> enumerable)
	{
		foreach (T item in enumerable)
		{
			hashset.Add(item);
		}
	}

	private class Section
	{
		public Section(UniversalClass @class, UnityVersionRange range, HashSet<UniversalClass> derivedClasses)
		{
			Class = @class;
			Range = range;
			DerivedClasses = derivedClasses;
		}

		public static Section Create(UniversalClass @class, UnityVersionRange range, Dictionary<UniversalClass, UnityVersionRange> dictionary)
		{
			HashSet<UniversalClass> derivedClasses = @class.DerivedClasses.Where(derived => dictionary[derived].Contains(range)).ToHashSet();
			return new Section(@class, range, derivedClasses);
		}

		public UniversalClass Class { get; set; }
		public UnityVersionRange Range { get; set; }
		public HashSet<UniversalClass> DerivedClasses { get; }
		/// <summary>
		/// FieldName : (ReleaseNode?, EditorNode?)
		/// </summary>
		public Dictionary<string, (UniversalNode?, UniversalNode?)> ApprovedFields { get; } = new();

		public Section Clone()
		{
			HashSet<UniversalClass> newDerivedClasses = new(DerivedClasses.Count);
			newDerivedClasses.AddRange(DerivedClasses);
			Section newSection = new Section(Class, Range, newDerivedClasses);
			foreach ((string fieldName, (UniversalNode?, UniversalNode?) pair) in ApprovedFields)
			{
				newSection.ApprovedFields.Add(fieldName, pair);
			}
			return newSection;
		}

		public bool HasField(string fieldName, out float proportion)
		{
			int sum = 0;
			foreach (UniversalClass derivedClass in DerivedClasses)
			{
				if (derivedClass.ReleaseRootNode?.TryGetSubNodeByName(fieldName) is not null
					|| derivedClass.EditorRootNode?.TryGetSubNodeByName(fieldName) is not null)
				{
					sum++;
				}
			}
			proportion = (float)sum / DerivedClasses.Count;
			return sum > 0;
		}

		public override string ToString()
		{
			return $"{Class.Name} {Range}";
		}
	}
}
