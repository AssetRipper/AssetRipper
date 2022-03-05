using System.Text.Json.Serialization;

namespace AssemblyDumper.Unity
{
	public class UnityClass
	{
		private string name = "";
		private string @namespace = "";
		private string fullName = "";
		private string module = "";
		private string @base = "";
		private List<string> derived = new();

		/// <summary>
		/// The name of the class not including the namespace
		/// </summary>
		public string Name { get => name; set => name = value ?? ""; }
		/// <summary>
		/// The namespace of the class if it exists
		/// </summary>
		public string Namespace { get => @namespace; set => @namespace = value ?? ""; }
		/// <summary>
		/// The full name of the class including the namespace but not an assembly specification
		/// </summary>
		public string FullName { get => fullName; set => fullName = value ?? ""; }
		/// <summary>
		/// The module containing the class
		/// </summary>
		public string Module { get => module; set => module = value ?? ""; }
		/// <summary>
		/// The unique number used to identify the class
		/// </summary>
		public int TypeID { get; set; }
		/// <summary>
		/// The name of the base class if it exists. Namespace not included
		/// </summary>
		public string Base { get => @base; set => @base = value ?? ""; }
		/// <summary>
		/// The names of the classes that directly derive from this. Namespaces not included
		/// </summary>
		public List<string> Derived { get => derived; set => derived = value ?? new(); }
		/// <summary>
		/// The count of all classes that descend from this class<br/>
		/// It includes this class, so the count is always positive<br/>
		/// However, some older unity versions don't generate this, so sometimes we have to set it in SharedState initialization
		/// </summary>
		public uint DescendantCount { get; set; }
		/// <summary>
		/// The size in bytes of one object. Doesn't include alignments. May be wildly inaccurate, especially for classes with variable size.
		/// </summary>
		public int Size { get; set; }
		/// <summary>
		/// The zero based index of this class in the list of acquired classes. Doesn't mean much.
		/// </summary>
		public uint TypeIndex { get; set; }
		/// <summary>
		/// Is the class abstract?
		/// </summary>
		public bool IsAbstract { get; set; }
		/// <summary>
		/// Is the class sealed? Not necessarily accurate.
		/// </summary>
		public bool IsSealed { get; set; }
		/// <summary>
		/// Does the class only appear in the editor?
		/// </summary>
		public bool IsEditorOnly { get; set; }
		/// <summary>
		/// Does the class only appear in the editor?
		/// </summary>
		[JsonIgnore]
		public bool IsReleaseOnly { get; set; }
		/// <summary>
		/// Is the class stripped?
		/// </summary>
		public bool IsStripped { get; set; }
		public UnityNode? EditorRootNode { get; set; }
		public UnityNode? ReleaseRootNode { get; set; }

		/// <summary>
		/// The constructor used in json deserialization
		/// </summary>
		public UnityClass() { }

		/// <summary>
		/// The constructor used to make dependent class definitions
		/// </summary>
		public UnityClass(UnityNode? releaseRootNode, UnityNode? editorRootNode)
		{
			if (releaseRootNode == null && editorRootNode == null)
				throw new ArgumentException("Both root nodes cannot be null");

			ReleaseRootNode = releaseRootNode;
			EditorRootNode = editorRootNode;
			UnityNode? mainRootNode = releaseRootNode ?? editorRootNode;

			Name = mainRootNode!.TypeName;
			FullName = Name;
			TypeID = -1;
			Derived = new List<string>();
			DescendantCount = 1;
			Size = mainRootNode.ByteSize;
			IsAbstract = false;
			IsSealed = true;
			IsEditorOnly = releaseRootNode == null;
			IsReleaseOnly = editorRootNode == null;
			IsStripped = false;
		}

		/// <summary>
		/// Gets the original name of the type and asserts compatible naming
		/// </summary>
		/// <param name="originalTypeName">The original name of the type before any changes were applied</param>
		/// <returns>True if the original name is different from the current name</returns>
		public bool GetOriginalTypeName(out string originalTypeName)
		{
			if (ReleaseRootNode == null && EditorRootNode == null)
			{
				originalTypeName = Name;
				return false;
			}
			else if (ReleaseRootNode == null)
			{
				Assertions.AssertEquality(Name, EditorRootNode!.TypeName);
				originalTypeName = EditorRootNode.OriginalTypeName;
				return originalTypeName != Name;
			}
			else if (EditorRootNode == null)
			{
				Assertions.AssertEquality(Name, ReleaseRootNode.TypeName);
				originalTypeName = ReleaseRootNode.OriginalTypeName;
				return originalTypeName != Name;
			}
			else
			{
				Assertions.AssertEquality(Name, ReleaseRootNode.TypeName);
				Assertions.AssertEquality(Name, EditorRootNode.TypeName);
				Assertions.AssertEquality(ReleaseRootNode.OriginalTypeName, EditorRootNode.OriginalTypeName);
				originalTypeName = ReleaseRootNode.OriginalTypeName;
				return originalTypeName != Name;
			}
		}
	}
}