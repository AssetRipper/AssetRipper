namespace AssetRipper.AssemblyDumper.Passes;

public static class Pass013_UnifyFieldsOfAbstractTypes
{
	private const float MinMatchingProportion = .999f;
	private static readonly bool shouldSkip = true;
	public static void DoPass()
	{
		if (shouldSkip)
		{
			return;
		}

		//We need to get all abstract classes, and we need to do them in order of lowest abstraction to highest.
		//In other words, the most derived classes should be done first, so their values can be used for their own base classes

		//Get all the abstract classes
		List<UniversalClass> abstractClasses = SharedState.Instance.ClassGroups.Values.SelectMany(g => g.Classes).Where(c => c.IsAbstract).ToList();

		//Now sort so that most-derived classes are first, or more crucially, before any of their base classes. 
		abstractClasses.Sort((a, b) => CompareByInheritance(a, b));

		foreach (UniversalClass abstractClass in abstractClasses)
		{
			Console.WriteLine($"{abstractClass.TypeID} {abstractClass.Name}");
			abstractClass.InitializeRootNodes();

			List<UniversalClass> derived = abstractClass.DerivedClasses;

			if (derived.Count == 0)
			{
				continue;
			}

			//Abstract classes that inherit from a nonabstract class will have the inherited fields in the json
			//For example, Behaviour inherits from Component
			List<string> existingEditorFields = abstractClass.EditorRootNode!.SubNodes!.Select(c => c.Name!).ToList();
			List<string> existingReleaseFields = abstractClass.ReleaseRootNode!.SubNodes!.Select(c => c.Name!).ToList();

			//Handle editor node
			foreach (string editorFieldName in GetAllFieldNames(derived, false))
			{
				if (existingEditorFields.Contains(editorFieldName))
				{
					continue;
				}

				UniversalNode subNode = GetFirstNode(derived, false, editorFieldName);

				float matchProportion = GetWeightedMatching(derived, false, editorFieldName, subNode);

				Console.WriteLine($"\t{matchProportion} {editorFieldName} editor");
				if (matchProportion < MinMatchingProportion)
				{
					//Mismatch in too many descendent classes, break out.
					break;
				}

				//This field is common to all sub classes. Add it to base.
				// Console.WriteLine($"\t\tCopying field {subNode.Name} to EDITOR");
				abstractClass.EditorRootNode!.SubNodes!.Add(subNode.DeepClone());
			}

			//Handle release node
			foreach (string releaseFieldName in GetAllFieldNames(derived, true))
			{
				//Console.WriteLine($"")
				if (existingReleaseFields.Contains(releaseFieldName))
				{
					continue;
				}

				UniversalNode subNode = GetFirstNode(derived, true, releaseFieldName);

				float matchProportion = GetWeightedMatching(derived, true, releaseFieldName, subNode);
				Console.WriteLine($"\t{matchProportion} {releaseFieldName} release");
				if (matchProportion < MinMatchingProportion)
				{
					//Mismatch in too many descendent classes, break out.
					break;
				}

				//This field is common to all sub classes. Add it to base.
				// Console.WriteLine($"\t\tCopying field {subNode.Name} to EDITOR");
				abstractClass.ReleaseRootNode!.SubNodes!.Add(subNode.DeepClone());
			}
		}
	}

	private static List<string> GetAllFieldNames(List<UniversalClass> classes, bool isRelease)
	{
		return classes.SelectMany(klass => klass.GetSubNodes(isRelease)) //all the subnodes
			.Select(s => s.Name!) //convert to their field name
			.Distinct() //remove duplicates
			.ToList(); //make list
	}

	private static UniversalNode GetFirstNode(List<UniversalClass> classes, bool isRelease, string fieldName)
	{
		return classes.SelectMany(klass => klass.GetSubNodes(isRelease)) //all the subnodes
			.First(s => s.Name == fieldName); //where the field name matches
	}

	private static List<UniversalNode> GetSubNodes(this UniversalClass klass, bool isRelease)
	{
		List<UniversalNode>? result = isRelease ? klass.ReleaseRootNode?.SubNodes : klass.EditorRootNode?.SubNodes;
		return result ?? new List<UniversalNode>();
	}

	private static float GetWeightedMatching(List<UniversalClass> classes, bool isRelease, string fieldName, UniversalNode baseFieldNode)
	{
		uint total = 0;
		uint matching = 0;

		foreach (UniversalClass universalClass in classes)
		{
			int duplicateCount = classes.Count(c => c.Name == universalClass.Name);
			float weight = (float)universalClass.DescendantCount / duplicateCount;
			if (isRelease)
			{
				if (universalClass.ReleaseRootNode.ContainsField(fieldName, out UniversalNode? fieldNode))
				{
					if (!AreEqual(fieldNode, baseFieldNode))
					{
						Console.WriteLine($"\tConflicts with {universalClass.Name} {universalClass.TypeID}");
						return 0f;
					}
					//matching += weight;
					matching += universalClass.DescendantCount;
				}
			}
			else
			{
				if (universalClass.EditorRootNode.ContainsField(fieldName, out UniversalNode? fieldNode))
				{
					if (!AreEqual(fieldNode, baseFieldNode))
					{
						Console.WriteLine($"\tConflicts with {universalClass.Name} {universalClass.TypeID}");
						return 0f;
					}
					//matching += weight;
					matching += universalClass.DescendantCount;
				}
			}
			//total += weight;
			total += universalClass.DescendantCount;
		}

		float result = matching / total;
		return !float.IsNaN(result)
			? result
			: throw new InvalidOperationException("Proportion cannot be nan");
	}

	private static bool AreEqual(UniversalNode left, UniversalNode right)
	{
		if (left.OriginalName != right.OriginalName) //The root nodes will most likely not have the same name
		{
			return false;
		}
		if (left.Name != right.Name) //The root nodes will most likely not have the same name
		{
			return false;
		}
		if (left.OriginalTypeName != right.OriginalTypeName)
		{
			return false;
		}
		if (left.TypeName != right.TypeName)
		{
			return false;
		}
		if (left.Version != right.Version)
		{
			return false;
		}
		if (left.MetaFlag != right.MetaFlag)
		{
			return false;
		}
		if (left.SubNodes!.Count != right.SubNodes!.Count)
		{
			return false;
		}
		for (int i = 0; i < left.SubNodes.Count; i++)
		{
			if (!AreEqual(left.SubNodes[i], right.SubNodes[i]))
			{
				return false;
			}
		}
		return true;
	}

	private static bool ContainsField(this UniversalNode? rootNode, string fieldName, [NotNullWhen(true)] out UniversalNode? fieldNode)
	{
		fieldNode = rootNode?.SubNodes.SingleOrDefault(node => node.Name == fieldName);
		return fieldNode != null;
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

	/// <summary>
	/// A compare function favoring less derivation
	/// </summary>
	/// <returns>
	/// -1 if the left derives from the right<br/>
	/// 1 if the right derives from the left<br/>
	/// 0 if neither derives from the other
	/// </returns>
	private static int CompareByInheritance(UniversalClass left, UniversalClass right)
	{
		return left.IsSubclassOf(right)
			? -1
			: right.IsSubclassOf(left)
				? 1
				: 0;
	}

	private static bool IsSubclassOf(this UniversalClass potentialSubClass, UniversalClass potentialSuperClass)
	{
		UniversalClass? baseClass = potentialSubClass.BaseClass;
		while (baseClass is not null)
		{
			if (baseClass.Name == potentialSuperClass.Name)
			{
				return true;
			}

			baseClass = baseClass.BaseClass;
		}

		return false;
	}
}