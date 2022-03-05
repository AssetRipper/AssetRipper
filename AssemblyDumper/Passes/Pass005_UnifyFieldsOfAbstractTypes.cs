using AssemblyDumper.Unity;

namespace AssemblyDumper.Passes
{
	public static class Pass005_UnifyFieldsOfAbstractTypes
	{
		private const float MinMatchingProportion = 0.7f;
		private const string ObjectHideFlagsName = "m_ObjectHideFlags";
		public static void DoPass()
		{
			Console.WriteLine("Pass 005: Merging fields of abstract types");

			//We need to get all abstract classes, and we need to do them in order of lowest abstraction to highest.
			//In other words, the most derived classes should be done first, so their values can be used for their own base classes

			//Get all the abstract classes
			List<UnityClass> abstractClasses = SharedState.ClassDictionary.Values.Where(c => c.IsAbstract).ToList();

			//Now sort so that most-derived classes are first, or more crucially, before any of their base classes. 
			abstractClasses.Sort((a, b) => Compare(a, b));

			foreach (UnityClass abstractClass in abstractClasses)
			{
				// Console.WriteLine($"\t{abstractClass.Name}");
				abstractClass.InitializeRootNodes();

				List<UnityClass>? derived = abstractClass.AllDerivedClasses();

				if (derived.Count == 0)
					continue;

				//Abstract classes that inherit from a nonabstract class will have the inherited fields in the json
				//For example, Behaviour inherits from Component
				List<string> existingEditorFields = abstractClass.EditorRootNode!.SubNodes!.Select(c => c.Name!).ToList();
				List<string> existingReleaseFields = abstractClass.ReleaseRootNode!.SubNodes!.Select(c => c.Name!).ToList();

				//Handle editor node
				foreach (string editorFieldName in GetAllFieldNames(derived, false))
				{
					if (existingEditorFields.Contains(editorFieldName))
						continue;

					float matchProportion = GetWeightedMatching(derived, false, editorFieldName);
					if(matchProportion < MinMatchingProportion)
					{
						//Mismatch in too many descendent classes, break out.
						break;
					}
					
					//This field is common to all sub classes. Add it to base.
					UnityNode subNode = GetFirstNode(derived, false, editorFieldName);
					// Console.WriteLine($"\t\tCopying field {subNode.Name} to EDITOR");
					abstractClass.EditorRootNode!.SubNodes!.Add(subNode.DeepClone());
				}

				//Handle release node
				foreach (string releaseFieldName in GetAllFieldNames(derived, true))
				{
					if (existingReleaseFields.Contains(releaseFieldName))
						continue;

					float matchProportion = GetWeightedMatching(derived, true, releaseFieldName);
					if (matchProportion < MinMatchingProportion)
					{
						//Mismatch in too many descendent classes, break out.
						break;
					}
					
					//This field is common to all sub classes. Add it to base.
					UnityNode subNode = GetFirstNode(derived, true, releaseFieldName);
					// Console.WriteLine($"\t\tCopying field {subNode.Name} to EDITOR");
					abstractClass.ReleaseRootNode!.SubNodes!.Add(subNode.DeepClone());
				}
			}
		}

		private static List<UnityClass> AllDerivedClasses(this UnityClass parent) => parent.Derived!.Select(c => SharedState.ClassDictionary[c]).ToList();

		private static List<string> GetAllFieldNames(List<UnityClass> classes, bool isRelease)
		{
			return classes.SelectMany(klass => klass.GetSubNodes(isRelease)) //all the subnodes
				.Select(s => s.Name!) //convert to their field name
				.Distinct() //remove duplicates
				.ToList(); //make list
		}

		private static UnityNode GetFirstNode(List<UnityClass> classes, bool isRelease, string fieldName)
		{
			return classes.SelectMany(klass => klass.GetSubNodes(isRelease)) //all the subnodes
				.First(s => s.Name == fieldName); //where the field name matches
		}

		private static List<UnityNode> GetSubNodes(this UnityClass klass, bool isRelease)
		{
			List<UnityNode>? result = isRelease ? klass.ReleaseRootNode?.SubNodes : klass.EditorRootNode?.SubNodes;
			return result ?? new List<UnityNode>();
		}

		private static float GetWeightedMatching(List<UnityClass> classes, bool isRelease, string fieldName)
		{
			if (fieldName == ObjectHideFlagsName)
				return 1f;

			uint total = 0;
			uint matching = 0;

			foreach(UnityClass unityClass in classes)
			{
				if (isRelease)
				{
					if (unityClass.ReleaseRootNode!.ContainsField(fieldName))
					{
						matching += unityClass.DescendantCount;
					}
				}
				else
				{
					if (unityClass.EditorRootNode!.ContainsField(fieldName))
					{
						matching += unityClass.DescendantCount;
					}
				}
				total += unityClass.DescendantCount;
			}

			float result = (float)matching / total;
			if (float.IsNaN(result))
			{
				throw new InvalidOperationException("Proportion cannot be nan");
			}
			return result;
		}

		private static bool ContainsField(this UnityNode rootNode, string fieldName)
		{
			return rootNode?.SubNodes?.Select(node => node.Name).Contains(fieldName) ?? false;
		}

		private static void InitializeRootNodes(this UnityClass abstractClass)
		{
			abstractClass.EditorRootNode ??= new()
			{
				Name = "Base",
				OriginalName = "Base",
				TypeName = abstractClass.Name,
				Index = 0,
				Level = 0,
				SubNodes = new(),
				Version = 1,
			};
			abstractClass.ReleaseRootNode ??= new()
			{
				Name = "Base",
				OriginalName = "Base",
				TypeName = abstractClass.Name,
				Index = 0,
				Level = 0,
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
		private static int Compare(UnityClass left, UnityClass right)
		{
			if(left.IsSubclassOf(right))
				return -1;
			else if(right.IsSubclassOf(left))
				return 1;
			else
				return 0;
		}

		private static bool IsSubclassOf(this UnityClass potentialSubClass, UnityClass potentialSuperClass)
		{
			string? baseTypeName = potentialSubClass.Base;
			while (!string.IsNullOrEmpty(baseTypeName))
			{
				if (baseTypeName == potentialSuperClass.Name)
					return true;

				baseTypeName = SharedState.ClassDictionary[baseTypeName].Base;
			}

			return false;
		}
	}
}