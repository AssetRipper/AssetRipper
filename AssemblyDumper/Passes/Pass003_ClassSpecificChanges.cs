using AssemblyDumper.Unity;

namespace AssemblyDumper.Passes
{
	public static class Pass003_ClassSpecificChanges
	{
		public static void DoPass()
		{
			Console.WriteLine("Pass 003: Class Specific Changes");
			
			foreach(UnityClass textClass in GetAllTextAssetClasses())
			{
				textClass.FixTextAssetScript();
			}
		}

		private static List<UnityClass> GetAllTextAssetClasses() => GetClassAndAllSubClasses("TextAsset");

		private static List<UnityClass> GetClassAndAllSubClasses(string className)
		{
			List<UnityClass> result = new List<UnityClass>();
			UnityClass baseClass = SharedState.ClassDictionary[className];
			result.Add(baseClass);
			result.AddAllSubClassesRecursively(baseClass);
			return result;
		}

		private static void AddAllSubClassesRecursively(this List<UnityClass> list, UnityClass baseClass)
		{
			foreach (string className in baseClass.Derived)
			{
				UnityClass unityClass = SharedState.ClassDictionary[className];
				list.Add(unityClass);
				list.AddAllSubClassesRecursively(unityClass);
			}
		}

		private static void FixTextAssetScript(this UnityClass textAssetClass)
		{
			textAssetClass.EditorRootNode?.SubNodes.SingleOrDefault(node => node.Name == "m_Script")?.FixScriptNode();
			textAssetClass.ReleaseRootNode?.SubNodes.SingleOrDefault(node => node.Name == "m_Script")?.FixScriptNode();
		}

		private static void FixScriptNode(this UnityNode scriptNode)
		{
			scriptNode.TypeName = "TextAssetContent";
			UnityNode arrayNode = scriptNode.SubNodes.Single();
			arrayNode.Name = "content";
		}
	}
}
