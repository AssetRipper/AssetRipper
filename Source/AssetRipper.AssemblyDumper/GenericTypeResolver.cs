using AssetRipper.Assets.Generics;

namespace AssetRipper.AssemblyDumper;

internal static class GenericTypeResolver
{
	public static GenericInstanceTypeSignature ResolveDictionaryType(UniversalNode node, UnityVersion version)
	{
		UniversalNode pairNode = node.SubNodes![0] //Array
			.SubNodes![1]; //Pair

		GenericInstanceTypeSignature genericKvp = ResolvePairType(pairNode, version);

		return SharedState.Instance.Importer.ImportType(typeof(AssetDictionary<,>)).MakeGenericInstanceType(genericKvp.TypeArguments[0], genericKvp.TypeArguments[1]);
	}

	public static TypeSignature ResolveVectorType(UniversalNode vectorNode, UnityVersion version)
	{
		return ResolveArrayType(vectorNode.SubNodes![0], version);
	}

	public static TypeSignature ResolveArrayType(UniversalNode arrayNode, UnityVersion version)
	{
		UniversalNode contentNode = arrayNode.SubNodes![1];
		TypeSignature elementType = ResolveNode(contentNode, version);

		if (elementType is CorLibTypeSignature { ElementType: ElementType.U1 })
		{
			return elementType.MakeSzArrayType();
		}
		else if (elementType is SzArrayTypeSignature)
		{
			throw new NotSupportedException();
		}

		return SharedState.Instance.Importer.ImportType(typeof(AssetList<>)).MakeGenericInstanceType(elementType);
	}

	public static SzArrayTypeSignature MakeAndImportArrayType(this ITypeDefOrRef type)
	{
		return MakeAndImportArrayType(type.ToTypeSignature());
	}

	public static SzArrayTypeSignature MakeAndImportArrayType(this TypeSignature typeSignature)
	{
		return new SzArrayTypeSignature(SharedState.Instance.Importer.UnderlyingImporter.ImportTypeSignature(typeSignature));
	}

	public static GenericInstanceTypeSignature ResolvePairType(UniversalNode pairNode, UnityVersion version)
	{
		return ResolvePairType(pairNode.SubNodes![0], pairNode.SubNodes[1], version);
	}
	public static GenericInstanceTypeSignature ResolvePairType(UniversalNode first, UniversalNode second, UnityVersion version)
	{
		TypeSignature firstType = ResolveNode(first, version);
		TypeSignature secondType = ResolveNode(second, version);

		if (firstType is SzArrayTypeSignature || secondType is SzArrayTypeSignature)
		{
			throw new Exception("Arrays not supported in pairs/dictionaries");
		}

		//Construct a KeyValuePair
		ITypeDefOrRef kvpType = SharedState.Instance.Importer.ImportType(typeof(AssetPair<,>));
		GenericInstanceTypeSignature genericKvp = kvpType.MakeGenericInstanceType(firstType, secondType);
		return genericKvp;
	}

	public static TypeSignature ResolveNode(UniversalNode node, UnityVersion version)
	{
		return node.NodeType switch
		{
			NodeType.Pair => ResolvePairType(node, version),
			NodeType.Map => ResolveDictionaryType(node, version),
			NodeType.Vector => ResolveVectorType(node, version),
			NodeType.TypelessData => SharedState.Instance.Importer.UInt8.MakeSzArrayType(),
			NodeType.Array => ResolveArrayType(node, version),
			NodeType.Type => SharedState.Instance.SubclassGroups[node.TypeName].GetTypeForVersion(version).ToTypeSignature(),
			_ => node.NodeType.ToPrimitiveTypeSignature(),
		};
	}
}
