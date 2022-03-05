using AssemblyDumper.Unity;

namespace AssemblyDumper.Passes
{
	public static class Pass004_ExtractDependentNodeTrees
	{
		/// <summary>
		/// OriginalTypeName : List(newTypeName,releaseRoot,editorRoot)
		/// </summary>
		private static readonly Dictionary<string, List<(string, UnityNode?, UnityNode?)>> generatedTypes = new Dictionary<string, List<(string, UnityNode?, UnityNode?)>>();

		public static void DoPass()
		{
			Console.WriteLine("Pass 004: Extract Dependent Node Trees");
			AddExistingClassDictionaryToGeneratedTypes();
			AddDependentTypes();
			CreateNewClasses();
			CheckCompatibility();
		}

		private static void AddExistingClassDictionaryToGeneratedTypes()
		{
			foreach(UnityClass unityClass in SharedState.ClassDictionary.Values)
			{
				List<(string, UnityNode?, UnityNode?)> newList = new List<(string, UnityNode?, UnityNode?)>();
				newList.Add((unityClass.Name!, unityClass.ReleaseRootNode, unityClass.EditorRootNode));
				generatedTypes.Add(unityClass.Name!, newList);
			}
		}

		private static void CreateNewClasses()
		{
			foreach ((string originalName, List<(string, UnityNode?, UnityNode?)> variantList) in generatedTypes)
			{
				foreach((string uniqueName, UnityNode? releaseNode, UnityNode? editorNode) in variantList)
				{
					if(!SharedState.ClassDictionary.TryGetValue(uniqueName, out UnityClass _))
					{
						UnityClass? newClass = new UnityClass(releaseNode, editorNode);
						//newClass.Name = uniqueName;
						//newClass.FullName = uniqueName;
						SharedState.ClassDictionary.Add(uniqueName, newClass);
					}
				}
			}
		}

		private static void CheckCompatibility()
		{
			foreach (UnityClass unityClass in SharedState.ClassDictionary.Values)
			{
				if(!AreCompatibleWithLogging(unityClass.ReleaseRootNode, unityClass.EditorRootNode, false))
				{
					Console.WriteLine($"{unityClass.Name} has incompatible release and editor root nodes");
				}
			}
		}

		private static void AddDependentTypes()
		{
			foreach(KeyValuePair<string, UnityClass> pair in SharedState.ClassDictionary)
			{
				AddDependentTypes(pair.Value);
			}
		}

		private static void AddDependentTypes(UnityClass unityClass) => AddDependentTypes(unityClass.ReleaseRootNode, unityClass.EditorRootNode);
		private static void AddDependentTypes(UnityNode? releaseNode, UnityNode? editorNode)
		{
			List<(UnityNode?, UnityNode?)> fieldList = GenerateFieldDictionary(releaseNode, editorNode);
			foreach((UnityNode? releaseField,UnityNode? editorField) in fieldList)
			{
				string typeName = releaseField?.TypeName! ?? editorField!.TypeName!;
				if (PrimitiveTypes.primitiveNames.Contains(typeName))
					continue;

				string originalTypeName = releaseField?.OriginalTypeName! ?? editorField!.OriginalTypeName!;

				AddDependentTypes(releaseField, editorField);

				if (generatedTypes.TryGetValue(typeName, out List<(string, UnityNode?, UnityNode?)>? list))
				{
					bool alreadyCovered = false;
					int relevantIndex = -1;
					for(int i = 0; i < list.Count; i++)
					{
						(string elementTypeName, UnityNode? releaseTypeNode, UnityNode? editorTypeNode) = list[i];
						bool releaseIsEqual = AreEqual(releaseField, releaseTypeNode, true);
						bool editorIsEqual = AreEqual(editorField, editorTypeNode, true);

						bool isUsuable = (releaseIsEqual && editorIsEqual) ||
							(releaseIsEqual && editorField == null) ||
							(editorIsEqual && releaseField == null);

						if (isUsuable)
						{
							alreadyCovered = true;
							relevantIndex = i;
							break;
						}

						bool incomingEditorExistingRelease = releaseField == null && releaseTypeNode != null &&
							editorField != null && editorTypeNode == null &&
							AreCompatible(releaseTypeNode, editorField, true);

						bool existingHasIdenticalReleaseButNoEditor = releaseIsEqual && editorTypeNode == null && editorField != null;

						if (existingHasIdenticalReleaseButNoEditor || incomingEditorExistingRelease)
						{
							UnityNode newEditorNode = editorField!.DeepClone();
							newEditorNode.Name = "Base";
							newEditorNode.OriginalName = "Base";
							newEditorNode.OriginalTypeName = originalTypeName;
							newEditorNode.TypeName = elementTypeName;
							newEditorNode.RecalculateLevel(0); //Needed for type tree method generation

							list[i] = (elementTypeName, releaseTypeNode, newEditorNode);

							alreadyCovered = true;
							relevantIndex = i;
							break;
						}

						bool incomingReleaseExistingEditor = releaseField != null && releaseTypeNode == null &&
							editorField == null && editorTypeNode != null &&
							AreCompatible(releaseField, editorTypeNode, true);

						bool existingHasIdenticalEditorButNoRelease = editorIsEqual && releaseTypeNode == null && releaseField != null;

						if (existingHasIdenticalEditorButNoRelease || incomingReleaseExistingEditor)
						{
							UnityNode newReleaseNode = releaseField!.DeepClone();
							newReleaseNode.Name = "Base";
							newReleaseNode.OriginalName = "Base";
							newReleaseNode.OriginalTypeName= originalTypeName;
							newReleaseNode.TypeName = elementTypeName;
							newReleaseNode.RecalculateLevel(0); //Needed for type tree method generation

							list[i] = (elementTypeName, newReleaseNode, editorTypeNode);

							alreadyCovered = true;
							relevantIndex = i;
							break;
						}
					}

					if (!alreadyCovered)
					{
						relevantIndex = list.Count;
						string newTypeName = $"{typeName}_{list.Count}";
						UnityNode? newReleaseNode = releaseField?.DeepClone();
						if (newReleaseNode != null)
						{
							newReleaseNode.Name = "Base";
							newReleaseNode.OriginalName = "Base";
							newReleaseNode.OriginalTypeName = originalTypeName;
							newReleaseNode.TypeName = newTypeName;
							newReleaseNode.RecalculateLevel(0); //Needed for type tree method generation
						}
						UnityNode? newEditorNode = editorField?.DeepClone();
						if (newEditorNode != null)
						{
							newEditorNode.Name = "Base";
							newEditorNode.OriginalName = "Base";
							newEditorNode.OriginalTypeName = originalTypeName;
							newEditorNode.TypeName = newTypeName;
							newEditorNode.RecalculateLevel(0); //Needed for type tree method generation
						}
						list.Add((newTypeName, newReleaseNode, newEditorNode));
					}

					if (relevantIndex != 0)
					{
						if (releaseField != null)
						{
							//Console.WriteLine($"Release field {releaseField.Name} of type {releaseField.TypeName} renamed to {list[relevantIndex].Item1}");
							releaseField.OriginalTypeName = originalTypeName;
							releaseField.TypeName = list[relevantIndex].Item1;
						}
						if (editorField != null)
						{
							//Console.WriteLine($"Editor field {editorField.Name} of type {editorField.TypeName} renamed to {list[relevantIndex].Item1}");
							editorField.OriginalTypeName = originalTypeName;
							editorField.TypeName = list[relevantIndex].Item1;
						}
					}
				}
				else
				{
					if (!PrimitiveTypes.generics.Contains(typeName) && !SharedState.ClassDictionary.ContainsKey(typeName))
					{
						UnityNode? newReleaseNode = releaseField?.DeepClone();
						if(newReleaseNode != null)
						{
							newReleaseNode.Name = "Base";
							newReleaseNode.OriginalName = "Base";
							newReleaseNode.RecalculateLevel(0); //Needed for type tree method generation
						}
						UnityNode? newEditorNode = editorField?.DeepClone();
						if(newEditorNode != null)
						{
							newEditorNode.Name = "Base";
							newEditorNode.OriginalName = "Base";
							newEditorNode.RecalculateLevel(0); //Needed for type tree method generation
						}
						List<(string, UnityNode?, UnityNode?)> newList = new List<(string,UnityNode?,UnityNode?)>();
						newList.Add((typeName, newReleaseNode, newEditorNode));
						generatedTypes.Add(typeName, newList);
					}
				}
			}
		}
		
		private static List<(UnityNode?,UnityNode?)> GenerateFieldDictionary(UnityNode? releaseRoot, UnityNode? editorRoot)
		{
			if(releaseRoot?.SubNodes == null)
			{
				Func<UnityNode, (UnityNode?, UnityNode?)> func = new Func<UnityNode, (UnityNode?, UnityNode?)>(node => (null, node));
				return editorRoot?.SubNodes?.Select(func).ToList() ?? new List<(UnityNode?, UnityNode?)>();
			}
			else if (editorRoot?.SubNodes == null)
			{
				Func<UnityNode, (UnityNode?, UnityNode?)>? func = new Func<UnityNode, (UnityNode?, UnityNode?)>(node => (node, null));
				return releaseRoot?.SubNodes?.Select(func).ToList() ?? new List<(UnityNode?, UnityNode?)>();
			}
			Dictionary<string, (UnityNode?, UnityNode?)> result = new Dictionary<string, (UnityNode?, UnityNode?)>();
			foreach(UnityNode releaseNode in releaseRoot.SubNodes)
			{
				UnityNode? editorNode = editorRoot.SubNodes.FirstOrDefault(node => node.Name == releaseNode.Name);
				result.Add(releaseNode.Name!, (releaseNode, editorNode));
			}
			foreach(UnityNode editorNode in editorRoot.SubNodes)
			{
				if (!result.ContainsKey(editorNode.Name!))
				{
					result.Add(editorNode.Name!, (null, editorNode));
				}
			}
			return result.Values.ToList();
		}

		private static void RecalculateLevel(this UnityNode node, int depth)
		{
			node.Level = (byte)depth;
			if(node.SubNodes != null)
			{
				foreach (UnityNode subNode in node.SubNodes)
				{
					RecalculateLevel(subNode, depth + 1);
				}
			}
		}

		private static bool AreEqual(UnityNode? left, UnityNode? right, bool root)
		{
			if(left == null || right == null)
			{
				return left == null && right == null;
			}
			if(!root && left.OriginalName != right.OriginalName) //The root nodes will not have the same name if one has already been renamed to "Base"
			{
				//Console.WriteLine($"\tInequal because name {left.OriginalName} doesn't match {right.OriginalName}");
				return false;
			}
			if (left.OriginalTypeName != right.OriginalTypeName)
			{
				//Console.WriteLine($"\tInequal because type name {left.OriginalTypeName} doesn't match {right.OriginalTypeName}");
				return false;
			}
			if (left.SubNodes!.Count != right.SubNodes!.Count)
			{
				//Console.WriteLine($"\tInequal because subnode count {left.SubNodes.Count} doesn't match {right.SubNodes.Count}");
				return false;
			}
			for (int i = 0; i < left.SubNodes.Count; i++)
			{
				if(!AreEqual(left.SubNodes[i], right.SubNodes[i], false))
					return false;
			}
			return true;
		}

		private static bool AreCompatible(UnityNode releaseNode, UnityNode editorNode, bool root)
		{
			if (releaseNode == null || editorNode == null)
				return true;
			if(!root && releaseNode.OriginalName != editorNode.OriginalName) //The root nodes will not have the same name if one has already been renamed to "Base"
				return false;
			if(releaseNode.OriginalTypeName != editorNode.OriginalTypeName)
				return false;
			if (releaseNode.SubNodes == null || editorNode.SubNodes == null)
				return true;
			Dictionary<string, UnityNode> releaseFields = releaseNode.SubNodes.ToDictionary(x => x.Name!, x => x);
			Dictionary<string, UnityNode> editorFields = editorNode.SubNodes.ToDictionary(x => x.Name!, x => x);
			foreach(KeyValuePair<string, UnityNode> releasePair in releaseFields)
			{
				if(editorFields.TryGetValue(releasePair.Key, out UnityNode? editorField))
				{
					if(!AreCompatible(releasePair.Value, editorField, false))
					{
						return false;
					}
				}
			}
			return true;
		}

		private static bool AreCompatibleWithLogging(UnityNode? releaseNode, UnityNode? editorNode, bool root)
		{
			if (releaseNode == null || editorNode == null)
			{
				return true;
			}

			if (!root && releaseNode.OriginalName != editorNode.OriginalName) //The root nodes will not have the same name if one has already been renamed to "Base"
			{
				Console.WriteLine($"Original Name {releaseNode.OriginalName} does not equal {editorNode.OriginalName}");
				return false;
			}

			if (releaseNode.OriginalTypeName != editorNode.OriginalTypeName)
			{
				Console.WriteLine($"Original Type Name {releaseNode.OriginalTypeName} does not equal {editorNode.OriginalTypeName}");
				return false;
			}

			if (releaseNode.SubNodes == null || editorNode.SubNodes == null)
			{
				return true;
			}

			Dictionary<string, UnityNode> releaseFields = releaseNode.SubNodes.ToDictionary(x => x.Name!, x => x);
			Dictionary<string, UnityNode> editorFields = editorNode.SubNodes.ToDictionary(x => x.Name!, x => x);
			foreach (KeyValuePair<string, UnityNode> releasePair in releaseFields)
			{
				if (editorFields.TryGetValue(releasePair.Key, out UnityNode? editorField))
				{
					if (!AreCompatibleWithLogging(releasePair.Value, editorField, false))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}