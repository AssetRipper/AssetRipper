using Mono.Cecil;
using System.Reflection;

namespace AssetRipper.Core.Structure.Assembly.Mono.Extensions
{
	public static class TypeReferenceExtensions
	{
		/*
		 * ResolveOrDefault
		 * 
		 * Other changes:
		 * etype
		 * public ElementType
		 */

		public static TypeReference ResolveOrDefault(this TypeReference typeReference)
		{
			ModuleDefinition module = typeReference.Module;
			if (module == null)
			{
				return typeReference;
			}

			TypeDefinition definition = module.MetadataResolver.Resolve(typeReference);
			if (definition == null)
			{
				return typeReference;
			}

			return definition;
		}

		static FieldInfo etypeFieldInfo = typeof(TypeReference).GetField("etype", BindingFlags.NonPublic | BindingFlags.Instance)
			?? throw new Exception();

		public static ElementType GetEType(this TypeReference _this)
		{
			return (ElementType)(etypeFieldInfo.GetValue(_this) ?? throw new Exception());
		}

		public static void SetEType(this TypeReference _this, ElementType value)
		{
			etypeFieldInfo.SetValue(_this, value);
		}
	}
}
