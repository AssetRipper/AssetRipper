using AssetRipper.AssemblyDumper.Utils;
using AssetRipper.Tpk.Shared;
using AssetRipper.Tpk.TypeTrees;

namespace AssetRipper.AssemblyDumper;

internal sealed class UniversalClass : IDeepCloneable<UniversalClass>
{
	/// <summary>
	/// The name of the class not including the namespace.
	/// </summary>
	public string Name { get; set; } = "";
	/// <summary>
	/// The original name of the class not including the namespace.
	/// </summary>
	public string OriginalName { get; set; } = "";
	/// <summary>
	/// The unique number used to identify the class. Negative value indicates that the class doesn't have a type id
	/// </summary>
	public int TypeID { get; set; }
	/// <summary>
	/// <see cref="TypeID"/> might be overridden. This is the original type id.
	/// </summary>
	public int OriginalTypeID { get; set; }
	/// <summary>
	/// The name of the base class if it exists. Namespace not included
	/// </summary>
	public string? BaseString { get; set; }
	/// <summary>
	/// The base class if it exists
	/// </summary>
	public UniversalClass? BaseClass { get; set; }
	/// <summary>
	/// The names of the classes that directly derive from this. Namespaces not included
	/// </summary>
	public List<UniversalClass> DerivedClasses { get; } = new();
	/// <summary>
	/// The count of all classes that descend from this class<br/>
	/// It includes this class, so the count is always positive<br/>
	/// This gets generated in <see cref="Passes.Pass011_ApplyInheritance"/>
	/// </summary>
	public uint DescendantCount { get; internal set; } = 1;
	/// <summary>
	/// Is the class abstract?
	/// </summary>
	public bool IsAbstract { get; set; }
	/// <summary>
	/// Is the class sealed?
	/// </summary>
	public bool IsSealed => DerivedClasses.Count == 0 && !IsAbstract;
	/// <summary>
	/// Does the class only appear in the editor?
	/// </summary>
	public bool IsEditorOnly { get; set; }
	/// <summary>
	/// Does the class only appear in game files?
	/// </summary>
	public bool IsReleaseOnly { get; set; }
	/// <summary>
	/// Is the class stripped?
	/// </summary>
	public bool IsStripped { get; set; }
	public UniversalNode? EditorRootNode { get; set; }
	public UniversalNode? ReleaseRootNode { get; set; }

	private UniversalClass() { }

	/// <summary>
	/// The constructor used to make dependent class definitions
	/// </summary>
	public UniversalClass(UniversalNode? releaseRootNode, UniversalNode? editorRootNode)
	{
		ReleaseRootNode = releaseRootNode;
		EditorRootNode = editorRootNode;
		UniversalNode mainRootNode = releaseRootNode ?? editorRootNode ?? throw new ArgumentException("Both root nodes cannot be null");

		Name = mainRootNode.TypeName;
		OriginalName = mainRootNode.OriginalTypeName;
		TypeID = -1;
		OriginalTypeID = -1;
		IsAbstract = false;
		IsEditorOnly = releaseRootNode == null;
		IsReleaseOnly = editorRootNode == null;
		IsStripped = false;
	}

	public static UniversalClass FromTpkUnityClass(TpkUnityClass tpkClass, int typeId, TpkStringBuffer stringBuffer, TpkUnityNodeBuffer nodeBuffer)
	{
		string name = stringBuffer[tpkClass.Name];
		return new()
		{
			Name = name,
			OriginalName = name,
			TypeID = typeId,
			OriginalTypeID = typeId,
			BaseString = stringBuffer[tpkClass.Base],
			IsAbstract = tpkClass.Flags.IsAbstract(),
			IsEditorOnly = tpkClass.Flags.IsEditorOnly(),
			IsReleaseOnly = tpkClass.Flags.IsReleaseOnly(),
			IsStripped = tpkClass.Flags.IsStripped(),
			EditorRootNode = tpkClass.Flags.HasEditorRootNode()
				? UniversalNode.FromTpkUnityNode(nodeBuffer[tpkClass.EditorRootNode], stringBuffer, nodeBuffer)
				: null,
			ReleaseRootNode = tpkClass.Flags.HasReleaseRootNode()
				? UniversalNode.FromTpkUnityNode(nodeBuffer[tpkClass.ReleaseRootNode], stringBuffer, nodeBuffer)
				: null
		};
	}

	public UniversalClass DeepClone()
	{
		UniversalClass newClass = new();
		newClass.Name = Name;
		newClass.OriginalName = OriginalName;
		newClass.TypeID = TypeID;
		newClass.OriginalTypeID = OriginalTypeID;
		newClass.BaseString = BaseString;
		newClass.BaseClass = BaseClass;
		newClass.DerivedClasses.Capacity = DerivedClasses.Count;
		newClass.DerivedClasses.AddRange(DerivedClasses);
		newClass.DescendantCount = DescendantCount;
		newClass.IsAbstract = IsAbstract;
		newClass.IsEditorOnly = IsEditorOnly;
		newClass.IsReleaseOnly = IsReleaseOnly;
		newClass.IsStripped = IsStripped;
		newClass.EditorRootNode = EditorRootNode?.DeepClone();
		newClass.ReleaseRootNode = ReleaseRootNode?.DeepClone();
		return newClass;
	}

	public override string ToString()
	{
		return Name;
	}

	public bool ContainsField(string fieldName)
	{
		UniversalNode? node = ReleaseRootNode?.TryGetSubNodeByName(fieldName) ?? EditorRootNode?.TryGetSubNodeByName(fieldName);
		return node is not null;
	}
}
