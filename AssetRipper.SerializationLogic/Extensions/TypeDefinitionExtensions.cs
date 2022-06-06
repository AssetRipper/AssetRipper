using AsmResolver.DotNet;

namespace AssetRipper.SerializationLogic.Extensions
{
	public static class TypeDefinitionExtensions
	{
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

		public static bool IsSubclassOf(this TypeDefinition type, params string[] baseTypeNames)
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

			return baseTypeDef.IsSubclassOf(baseTypeNames);
		}
	}
}
