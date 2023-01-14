namespace AssetRipper.SerializationLogic.Extensions
{
	public static class TypeDefinitionExtensions
	{
		public static bool IsSubclassOf(this TypeDefinition type, string ns, string name)
		{
			ITypeDefOrRef? baseType = type.BaseType;
			while (baseType != null)
			{
				if (baseType.Namespace == ns && baseType.Name == name)
				{
					return true;
				}
				baseType = baseType.Resolve()?.BaseType;
			}

			return false;
		}
		
		public static bool IsSubclassOf(this TypeDefinition type, string baseTypeName)
		{
			ITypeDefOrRef? baseType = type.BaseType;
			if (baseType == null)
			{
				return false;
			}

			if (baseType.FullName == baseTypeName)
			{
				return true;
			}

			TypeDefinition? baseTypeDef = baseType.Resolve();
			if (baseTypeDef == null)
			{
				return false;
			}

			return baseTypeDef.IsSubclassOf(baseTypeName);
		}

		public static bool IsSubclassOfAny(this TypeDefinition type, params string[] baseTypeNames)
		{
			ITypeDefOrRef? baseType = type.BaseType;
			if (baseType == null)
			{
				return false;
			}

			for (int i = 0; i < baseTypeNames.Length; i++)
			{
				if (baseType.FullName == baseTypeNames[i])
				{
					return true;
				}
			}

			TypeDefinition? baseTypeDef = baseType.Resolve();
			if (baseTypeDef == null)
			{
				return false;
			}

			return baseTypeDef.IsSubclassOfAny(baseTypeNames);
		}

		public static bool InheritsFromMonoBehaviour(this TypeDefinition type)
		{
			return type.InheritsFrom("UnityEngine.MonoBehaviour");
		}

		public static bool InheritsFromObject(this TypeDefinition type)
		{
			return type.InheritsFrom("UnityEngine.Object");
		}

		public static TypeDefinition? TryGetBaseClass(this TypeDefinition current)
		{
			return current.BaseType?.Resolve();
		}
	}
}
