using AssetRipper.AssemblyDumper.Utils;

namespace AssetRipper.AssemblyDumper.Passes;

internal static partial class Pass007_ExtractSubclasses
{
	private static Dictionary<string, List<SubclassCandidate>> candidateListDictionary = new();

	public static void DoPass()
	{
		foreach (VersionedList<UniversalClass> classList in SharedState.Instance.ClassInformation.Values)
		{
			List<ClassData> classDataList = MakeClassData(classList);
			foreach (ClassData classData in classDataList)
			{
				AddDependentTypes(classData);
			}
		}
		foreach (List<SubclassCandidate> candidateList in candidateListDictionary.Values)
		{
			AddClassesToSharedStateSubclasses(candidateList);
		}
		candidateListDictionary.Clear();
		CheckCompatibility();
		DoCustomInjections();
	}

	private static void CheckCompatibility()
	{
		foreach (VersionedList<UniversalClass> list in SharedState.Instance.SubclassInformation.Values)
		{
			foreach (UniversalClass? @class in list.Values)
			{
				if (@class is not null && !AreCompatibleWithLogging(@class.ReleaseRootNode, @class.EditorRootNode))
				{
					Console.WriteLine($"{@class.Name} has incompatible release and editor root nodes");
				}
			}
		}
	}

	private static List<ClassData> MakeClassData(VersionedList<UniversalClass> list)
	{
		List<ClassData> result = new List<ClassData>(list.Count);
		for (int i = 0; i < list.Count - 1; i++)
		{
			if (list[i].Value is not null)
			{
				result.Add(new ClassData(list[i].Value!.Name, list[i].Value!, new UnityVersionRange(list[i].Key, list[i + 1].Key)));
			}
		}
		if (list.Count > 0)
		{
			var lastPair = list[list.Count - 1];
			if (lastPair.Value is not null)
			{
				result.Add(new ClassData(lastPair.Value.Name, lastPair.Value, new UnityVersionRange(lastPair.Key, UnityVersion.MaxVersion)));
			}
		}
		return result;
	}

	private static List<SubclassCandidate> GetOrAddSubclassCandidateList(string name)
	{
		if (!candidateListDictionary.TryGetValue(name, out List<SubclassCandidate>? result))
		{
			result = new();
			candidateListDictionary.Add(name, result);
		}
		return result;
	}

	private static void AddDependentTypes(ClassData classData) => AddDependentTypes(classData.Class.ReleaseRootNode, classData.Class.EditorRootNode, classData);
	private static void AddDependentTypes(UniversalNode? releaseNode, UniversalNode? editorNode, ClassData classData)
	{
		List<(UniversalNode?, UniversalNode?)> fieldList = GenerateFieldList(releaseNode, editorNode);
		foreach ((UniversalNode? releaseField, UniversalNode? editorField) in fieldList)
		{
			UniversalNode mainNode = releaseField ?? editorField ?? throw new NullReferenceException();
			NodeType nodeType = mainNode.NodeType;

			if (!nodeType.IsPrimitive())
			{
				if (nodeType == NodeType.Type)
				{
					List<SubclassCandidate> candidateList = GetOrAddSubclassCandidateList(mainNode.TypeName);
					candidateList.Add(new SubclassCandidate(releaseField, editorField, classData.VersionRange));
				}
				AddDependentTypes(releaseField, editorField, classData);
			}
		}
	}

	private static List<(UniversalNode?, UniversalNode?)> GenerateFieldList(UniversalNode? releaseRoot, UniversalNode? editorRoot)
	{
		if (releaseRoot?.SubNodes == null)
		{
			Func<UniversalNode, (UniversalNode?, UniversalNode?)> func = new Func<UniversalNode, (UniversalNode?, UniversalNode?)>(node => (null, node));
			return editorRoot?.SubNodes?.Select(func).ToList() ?? new List<(UniversalNode?, UniversalNode?)>();
		}
		else if (editorRoot?.SubNodes == null)
		{
			Func<UniversalNode, (UniversalNode?, UniversalNode?)>? func = new Func<UniversalNode, (UniversalNode?, UniversalNode?)>(node => (node, null));
			return releaseRoot?.SubNodes?.Select(func).ToList() ?? new List<(UniversalNode?, UniversalNode?)>();
		}
		Dictionary<string, (UniversalNode?, UniversalNode?)> result = new Dictionary<string, (UniversalNode?, UniversalNode?)>();
		foreach (UniversalNode releaseNode in releaseRoot.SubNodes)
		{
			UniversalNode? editorNode = editorRoot.SubNodes.FirstOrDefault(node => node.Name == releaseNode.Name);
			result.Add(releaseNode.Name!, (releaseNode, editorNode));
		}
		foreach (UniversalNode editorNode in editorRoot.SubNodes)
		{
			if (!result.ContainsKey(editorNode.Name!))
			{
				result.Add(editorNode.Name!, (null, editorNode));
			}
		}
		return result.Values.ToList();
	}

	private static bool AreCompatible(UniversalNode releaseNode, UniversalNode editorNode, bool root)
	{
		if (releaseNode == null || editorNode == null)
		{
			return true;
		}

		if (!root && releaseNode.OriginalName != editorNode.OriginalName) //The root nodes might not have the same name
		{
			return false;
		}

		if (releaseNode.OriginalTypeName != editorNode.OriginalTypeName)
		{
			return false;
		}

		if (releaseNode.SubNodes == null || editorNode.SubNodes == null)
		{
			return true;
		}

		Dictionary<string, UniversalNode> releaseFields = releaseNode.SubNodes.ToDictionary(x => x.Name!, x => x);
		Dictionary<string, UniversalNode> editorFields = editorNode.SubNodes.ToDictionary(x => x.Name!, x => x);
		foreach (KeyValuePair<string, UniversalNode> releasePair in releaseFields)
		{
			if (editorFields.TryGetValue(releasePair.Key, out UniversalNode? editorField))
			{
				if (!AreCompatible(releasePair.Value, editorField, false))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static bool AreCompatibleWithLogging(UniversalNode? releaseNode, UniversalNode? editorNode)
	{
		if (releaseNode == null || editorNode == null)
		{
			return true;
		}

		if (releaseNode.OriginalName != editorNode.OriginalName)
		{
			Console.WriteLine($"Original Name {releaseNode.OriginalName} does not equal {editorNode.OriginalName}");
			return false;
		}

		if (releaseNode.Name != editorNode.Name)
		{
			Console.WriteLine($"Name {releaseNode.OriginalName} does not equal {editorNode.OriginalName}");
			return false;
		}

		if (releaseNode.OriginalTypeName != editorNode.OriginalTypeName)
		{
			Console.WriteLine($"Original Type Name {releaseNode.OriginalTypeName} does not equal {editorNode.OriginalTypeName}");
			return false;
		}

		if (releaseNode.TypeName != editorNode.TypeName)
		{
			Console.WriteLine($"Original Type Name {releaseNode.OriginalTypeName} does not equal {editorNode.OriginalTypeName}");
			return false;
		}

		if (releaseNode.SubNodes == null || editorNode.SubNodes == null)
		{
			return true;
		}

		Dictionary<string, UniversalNode> releaseFields = releaseNode.SubNodes.ToDictionary(x => x.Name!, x => x);
		Dictionary<string, UniversalNode> editorFields = editorNode.SubNodes.ToDictionary(x => x.Name!, x => x);
		foreach (KeyValuePair<string, UniversalNode> releasePair in releaseFields)
		{
			if (editorFields.TryGetValue(releasePair.Key, out UniversalNode? editorField))
			{
				if (!AreCompatibleWithLogging(releasePair.Value, editorField))
				{
					return false;
				}
			}
		}
		return true;
	}
}
