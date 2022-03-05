using AssemblyDumper.Unity;

namespace AssemblyDumper
{
	public static class SharedState
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public static AssemblyDefinition Assembly { get; set; }
		public static ModuleDefinition Module => Assembly.ManifestModule!;
		public static ReferenceImporter Importer { get; set; }
		public static string Version { get; private set; }
		public static List<UnityString> Strings { get; private set; }
		/// <summary>
		/// The name of the class (no namespace) : its UnityClass object
		/// </summary>
		public static Dictionary<string, UnityClass> ClassDictionary { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		/// <summary>
		/// The name of the class (no namespace) : its generated TypeDefinition
		/// </summary>
		public static Dictionary<string, TypeDefinition> TypeDictionary { get; } = new Dictionary<string, TypeDefinition>();


		//Namespaces
		public static string RootNamespace { get; set; } = "AssemblyDumperOutput";
		public static string AttributesNamespace => RootNamespace + ".Attributes";
		public static string ClassesNamespace => RootNamespace + ".Classes";
		public static string EnumsNamespace => RootNamespace + ".Enums";
		public static string ExamplesNamespace => RootNamespace + ".Examples";
		public static string InterfacesNamespace => RootNamespace + ".Interfaces";
		public static string IONamespace => RootNamespace + ".IO";
		public static string UtilsNamespace => RootNamespace + ".Utils";


		public static void Initialize(UnityInfo info)
		{
			Version = info.Version!;
			Strings = info.Strings ?? new List<UnityString>();
			ClassDictionary = info.Classes!.Where(y => y.TypeID < 100_000 || y.TypeID > 100_011).ToDictionary(x => x.Name!, x => x);
			//100,000 to 100,011 are excluded here because they always have null root nodes
			//The non primitives will get re-added in pass 4

			FixDescendantCountOnOlderUnityVersions();
		}

		/// <summary>
		/// On Unity 4 and lower, descendant count isn't available
		/// </summary>
		private static void FixDescendantCountOnOlderUnityVersions()
		{
			foreach(UnityClass @class in ClassDictionary.Values)
			{
				@class.FixDescendantCount();
			}
		}

		private static void FixDescendantCount(this UnityClass unityClass)
		{
			if (unityClass.DescendantCount == 0)
			{
				foreach(string derivedClassName in unityClass.Derived!)
				{
					UnityClass derivedClass = ClassDictionary[derivedClassName];
					derivedClass.FixDescendantCount();
					unityClass.DescendantCount += derivedClass.DescendantCount;
				}
				unityClass.DescendantCount++;
			}
		}
	}
}