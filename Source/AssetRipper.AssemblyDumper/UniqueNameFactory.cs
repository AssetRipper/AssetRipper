namespace AssetRipper.AssemblyDumper;

internal static class UniqueNameFactory
{
	public static string GetReadWriteName(UniversalNode node, UnityVersion version)
	{
		if (SharedState.Instance.SubclassGroups.TryGetValue(node.TypeName, out SubclassGroup? subclassGroup))
		{
			TypeDefinition fieldType = subclassGroup.GetTypeForVersion(version);
			return node.AlignBytes ? $"{fieldType.Name}Align" : fieldType.Name ?? throw new NullReferenceException();
		}

		switch (node.NodeType)
		{
			case NodeType.Vector:
				{
					UniversalNode arrayNode = node.SubNodes[0];
					UniversalNode listTypeNode = arrayNode.SubNodes[1];
					string listName = GetReadWriteName(listTypeNode, version);
					return node.AlignBytes || arrayNode.AlignBytes ? $"ArrayAlign_{listName}" : $"Array_{listName}";
				}
			case NodeType.Map:
				{
					UniversalNode arrayNode = node.SubNodes[0];
					UniversalNode pairNode = arrayNode.SubNodes[1];
					UniversalNode firstTypeNode = pairNode.SubNodes[0];
					UniversalNode secondTypeNode = pairNode.SubNodes[1];
					string firstTypeName = GetReadWriteName(firstTypeNode, version);
					string secondTypeName = GetReadWriteName(secondTypeNode, version);
					return node.AlignBytes || arrayNode.AlignBytes
						? $"MapAlign_{firstTypeName}_{secondTypeName}"
						: $"Map_{firstTypeName}_{secondTypeName}";
				}
			case NodeType.Pair:
				{
					UniversalNode firstTypeNode = node.SubNodes[0];
					UniversalNode secondTypeNode = node.SubNodes[1];
					string firstTypeName = GetReadWriteName(firstTypeNode, version);
					string secondTypeName = GetReadWriteName(secondTypeNode, version);
					return node.AlignBytes ? $"PairAlign_{firstTypeName}_{secondTypeName}" : $"Pair_{firstTypeName}_{secondTypeName}";
				}
			case NodeType.TypelessData: //byte array
				{
					return node.AlignBytes ? "TypelessDataAlign" : "TypelessData";
				}
			case NodeType.Array:
				{
					UniversalNode listTypeNode = node.SubNodes[1];
					string listName = GetReadWriteName(listTypeNode, version);
					return node.AlignBytes ? $"ArrayAlign_{listName}" : $"Array_{listName}";
				}
			default:
				{
					string name = node.NodeType.ToPrimitiveTypeName();
					return node.AlignBytes ? $"{name}Align" : name;
				}
		}
	}

	public static string GetYamlName(UniversalNode node, UnityVersion version)
	{
		if (SharedState.Instance.SubclassGroups.TryGetValue(node.TypeName, out SubclassGroup? subclassGroup))
		{
			TypeDefinition fieldType = subclassGroup.GetTypeForVersion(version);
			return fieldType.Name ?? throw new NullReferenceException();
		}

		switch (node.NodeType)
		{
			case NodeType.Vector:
				{
					UniversalNode arrayNode = node.SubNodes[0];
					UniversalNode listTypeNode = arrayNode.SubNodes[1];
					string listName = GetYamlName(listTypeNode, version);
					return $"Array_{listName}";
				}
			case NodeType.Map:
				{
					UniversalNode arrayNode = node.SubNodes[0];
					UniversalNode pairNode = arrayNode.SubNodes[1];
					UniversalNode firstTypeNode = pairNode.SubNodes[0];
					UniversalNode secondTypeNode = pairNode.SubNodes[1];
					string firstTypeName = GetYamlName(firstTypeNode, version);
					string secondTypeName = GetYamlName(secondTypeNode, version);
					return $"Map_{firstTypeName}_{secondTypeName}";
				}
			case NodeType.Pair:
				{
					UniversalNode firstTypeNode = node.SubNodes[0];
					UniversalNode secondTypeNode = node.SubNodes[1];
					string firstTypeName = GetYamlName(firstTypeNode, version);
					string secondTypeName = GetYamlName(secondTypeNode, version);
					return $"Pair_{firstTypeName}_{secondTypeName}";
				}
			case NodeType.TypelessData: //byte array
				{
					return "TypelessData";
				}
			case NodeType.Array:
				{
					UniversalNode listTypeNode = node.SubNodes[1];
					string listName = GetYamlName(listTypeNode, version);
					return $"Array_{listName}";
				}
			default:
				{
					return node.NodeType.ToPrimitiveTypeName();
				}
		}
	}

	public static string MakeUniqueName(TypeSignature type)
	{
		return type switch
		{
			CorLibTypeSignature or TypeDefOrRefSignature => type.Name,
			SzArrayTypeSignature arrayType => $"Array{MakeUniqueName(arrayType.BaseType)}",
			GenericInstanceTypeSignature genericType => $"{genericType.GenericType.Name?.ToString()[..^2]}_{string.Join('_', genericType.TypeArguments.Select(t => MakeUniqueName(t)))}",
			_ => throw new NotSupportedException(),
		}
		?? throw new NullReferenceException();
	}
}
