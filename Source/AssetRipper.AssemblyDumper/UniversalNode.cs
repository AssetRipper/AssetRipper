using AssetRipper.AssemblyDumper.Utils;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Tpk.Shared;
using AssetRipper.Tpk.TypeTrees;
using System.Diagnostics;

namespace AssetRipper.AssemblyDumper;

internal sealed class UniversalNode : IEquatable<UniversalNode>, IDeepCloneable<UniversalNode>
{
	/// <summary>
	/// The current type name
	/// </summary>
	public string TypeName { get => typeName; set => typeName = value ?? ""; }
	/// <summary>
	/// The original type name as obtained from the tpk file
	/// </summary>
	public string OriginalTypeName
	{
		get => string.IsNullOrEmpty(originalTypeName) ? TypeName : originalTypeName;
		set => originalTypeName = value ?? "";
	}
	public string Name { get => name; set => name = value ?? ""; }
	/// <summary>
	/// The original name as obtained from the tpk file
	/// </summary>
	public string OriginalName
	{
		get => string.IsNullOrEmpty(originalName) ? Name : originalName;
		set => originalName = value ?? "";
	}
	public short Version { get; set; }
	public TransferMetaFlags MetaFlag { get; set; }
	public List<UniversalNode> SubNodes { get => subNodes; set => subNodes = value ?? new(); }

	private string originalTypeName = "";
	private string originalName = "";
	private string typeName = "";
	private string name = "";
	private List<UniversalNode> subNodes = new();

	public bool IgnoreInMetaFiles => MetaFlag.IsIgnoreInMetaFiles();
	public bool AlignBytes => MetaFlag.IsAlignBytes();
	public bool TreatIntegerAsBoolean => MetaFlag.IsTreatIntegerValueAsBoolean();
	private bool TreatIntegerAsChar => MetaFlag.IsCharPropertyMask();

	public NodeType NodeType
	{
		get
		{
			return subNodes.Count == 0
				? TypeName switch
				{
					"bool" => NodeType.Boolean,
					//"char" => NodeType.Character,
					"char" => NodeType.UInt8,
					"SInt8" => NodeType.Int8,
					"UInt8" => NodeType.UInt8,
					"short" or "SInt16" => NodeType.Int16,
					"ushort" or "UInt16" or "unsigned short" => TreatIntegerAsChar ? NodeType.Character : NodeType.UInt16,
					"int" or "SInt32" or "Type*" or "EntityId" => NodeType.Int32,
					"uint" or "UInt32" or "unsigned int" => NodeType.UInt32,
					"SInt64" or "long long" => NodeType.Int64,
					"UInt64" or "FileSize" or "unsigned long long" => NodeType.UInt64,//FileSize is used in StreamedResource.m_Offset 2020.1+
					"float" => NodeType.Single,
					"double" => NodeType.Double,
					_ => NodeType.Type,
				}
				: TypeName switch
				{
					"Array" => NodeType.Array,
					"vector" or "staticvector" or "set" => NodeType.Vector,
					"map" => NodeType.Map,
					"pair" => NodeType.Pair,
					"TypelessData" => NodeType.TypelessData,
					"string" or Passes.Pass002_RenameSubnodes.Utf8StringName => NodeType.String,
					_ => NodeType.Type,
				};
		}
	}

	public UniversalNode()
	{
	}

	public bool TryGetSubNodeByName(string nodeName, [NotNullWhen(true)] out UniversalNode? subnode)
	{
		subnode = SubNodes.SingleOrDefault(n => n.Name == nodeName);
		return subnode is not null;
	}

	public UniversalNode? TryGetSubNodeByName(string nodeName)
	{
		return SubNodes.SingleOrDefault(n => n.Name == nodeName);
	}

	public bool TryGetSubNodeByTypeAndName(string nodeTypeName, string nodeName, [NotNullWhen(true)] out UniversalNode? subnode)
	{
		subnode = SubNodes.SingleOrDefault(n => n.Name == nodeName && n.TypeName == nodeTypeName);
		return subnode is not null;
	}

	public UniversalNode GetSubNodeByName(string nodeName)
	{
		return SubNodes.Single(n => n.Name == nodeName);
	}

	public static UniversalNode FromTpkUnityNode(TpkUnityNode tpkNode, TpkStringBuffer stringBuffer, TpkUnityNodeBuffer nodeBuffer)
	{
		UniversalNode result = new UniversalNode();
		result.TypeName = GetFixedTypeName(stringBuffer[tpkNode.TypeName]);
		result.OriginalTypeName = result.TypeName;
		result.Name = stringBuffer[tpkNode.Name];
		result.OriginalName = result.Name;
		result.Version = tpkNode.Version;
		result.MetaFlag = (TransferMetaFlags)tpkNode.MetaFlag;
		result.SubNodes = tpkNode.SubNodes
			.Select(nodeIndex => FromTpkUnityNode(nodeBuffer[nodeIndex], stringBuffer, nodeBuffer))
			.ToList();
		return result;
	}

	/// <summary>
	/// Only store one name for each primitive integer size.
	/// </summary>
	/// <remarks>
	/// Although this deduplicates, it also prevents these loaded type trees from being used in making new serialized files.
	/// </remarks>
	/// <param name="originalName"></param>
	/// <returns></returns>
	private static string GetFixedTypeName(string originalName)
	{
		return originalName switch
		{
			"short" => "SInt16",
			"int" => "SInt32",
			"long long" => "SInt64",
			"unsigned short" => "UInt16",
			"unsigned int" => "UInt32",
			"unsigned long long" => "UInt64",
			_ => originalName,
		};
	}

	/// <summary>
	/// Deep clones a node and all its subnodes<br/>
	/// Warning: Deep cloning a node with a circular hierarchy will cause an endless loop
	/// </summary>
	/// <returns>The new node</returns>
	public UniversalNode DeepClone()
	{
		UniversalNode clone = new UniversalNode();
		clone.TypeName = TypeName;
		clone.originalTypeName = originalTypeName;
		clone.Name = Name;
		clone.originalName = originalName;
		clone.Version = Version;
		clone.MetaFlag = MetaFlag;
		clone.SubNodes = SubNodes.ConvertAll(x => x.DeepClone());
		return clone;
	}

	/// <summary>
	/// Shallow clones a node but not its subnodes
	/// </summary>
	/// <returns>The new node</returns>
	public UniversalNode ShallowClone()
	{
		UniversalNode clone = new UniversalNode();
		clone.TypeName = TypeName;
		clone.originalTypeName = originalTypeName;
		clone.Name = Name;
		clone.originalName = originalName;
		clone.Version = Version;
		clone.MetaFlag = MetaFlag;
		clone.SubNodes = SubNodes.ToList();
		return clone;
	}

	public UniversalNode DeepCloneAsRootNode()
	{
		UniversalNode clone = DeepClone();
		clone.Name = "Base";
		clone.OriginalName = clone.Name;
		return clone;
	}

	public UniversalNode ShallowCloneAsRootNode()
	{
		UniversalNode clone = ShallowClone();
		clone.Name = "Base";
		clone.OriginalName = clone.Name;
		return clone;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as UniversalNode);
	}

	public bool Equals(UniversalNode? other)
	{
		return other is not null &&
			   TypeName == other.TypeName &&
			   OriginalTypeName == other.OriginalTypeName &&
			   Name == other.Name &&
			   OriginalName == other.OriginalName &&
			   Version == other.Version &&
			   MetaFlag == other.MetaFlag &&
			   EqualityComparer<List<UniversalNode>>.Default.Equals(SubNodes, other.SubNodes);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(TypeName, OriginalTypeName, Name, OriginalName, Version, MetaFlag, SubNodes);
	}

	public static bool operator ==(UniversalNode? left, UniversalNode? right)
	{
		return EqualityComparer<UniversalNode>.Default.Equals(left, right);
	}

	public static bool operator !=(UniversalNode? left, UniversalNode? right)
	{
		return !(left == right);
	}

	public override string ToString()
	{
		return $"{TypeName} {Name}";
	}
}
