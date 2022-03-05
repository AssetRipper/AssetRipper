using AssemblyDumper.Unity;

namespace AssemblyDumper
{
	public static class GenericTypeResolver
	{
		public static GenericInstanceTypeSignature ResolveDictionaryType(UnityNode node)
		{
			UnityNode pairNode = node.SubNodes![0] //Array
				.SubNodes![1]; //Pair

			GenericInstanceTypeSignature genericKvp = ResolvePairType(pairNode);
			
			return CommonTypeGetter.AssetDictionaryType!.MakeGenericInstanceType(genericKvp.TypeArguments[0], genericKvp.TypeArguments[1]);
		}

		public static SzArrayTypeSignature ResolveVectorType(UnityNode vectorNode)
		{
			return ResolveArrayType(vectorNode.SubNodes![0]);
		}

		public static SzArrayTypeSignature ResolveArrayType(UnityNode arrayNode)
		{
			UnityNode contentNode = arrayNode.SubNodes![1];
			TypeSignature elementType = ResolveNode(contentNode);
			return elementType.MakeAndImportArrayType();
		}

		public static SzArrayTypeSignature MakeAndImportArrayType(this ITypeDefOrRef type)
		{
			return MakeAndImportArrayType(type.ToTypeSignature());
		}

		public static SzArrayTypeSignature MakeAndImportArrayType(this TypeSignature typeSignature)
		{
			return new SzArrayTypeSignature(SharedState.Importer.ImportTypeSignature(typeSignature));
		}

		public static GenericInstanceTypeSignature ResolvePairType(UnityNode pairNode)
		{
			return ResolvePairType(pairNode.SubNodes![0], pairNode.SubNodes[1]);
		}
		public static GenericInstanceTypeSignature ResolvePairType(UnityNode first, UnityNode second)
		{
			TypeSignature firstType = ResolveNode(first);
			TypeSignature secondType = ResolveNode(second);

			//Construct a KeyValuePair
			ITypeDefOrRef kvpType = CommonTypeGetter.NullableKeyValuePair!;
			GenericInstanceTypeSignature genericKvp = kvpType.MakeGenericInstanceType(firstType, secondType);
			return genericKvp;
		}

		public static TypeSignature ResolveNode(UnityNode node)
		{
			string typeName = node.TypeName!;
			switch (typeName)
			{
				case "pair":
					return ResolvePairType(node);
				case "map":
					return ResolveDictionaryType(node);
				case "vector" or "set" or "staticvector":
					return ResolveVectorType(node);
				case "TypelessData":
					return SystemTypeGetter.UInt8!.MakeSzArrayType();
				case "Array":
					return ResolveArrayType(node);
				default:
					return SystemTypeGetter.GetCppPrimitiveTypeSignature(typeName) ??
						SharedState.Importer.ImportType(
							SystemTypeGetter.LookupSystemType(typeName) ??
							SharedState.TypeDictionary[typeName]
						).ToTypeSignature();
			}
		}
	}
}
