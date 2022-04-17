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
			var module = typeReference.Module;
			if (module == null)
				return typeReference;

			var definition = module.MetadataResolver.Resolve(typeReference);
			if (definition == null)
				return typeReference;

			return definition;
		}

		static bool initialized;
		static FieldInfo etypeFieldInfo;

		private static void Initialize()
		{
			etypeFieldInfo = typeof(TypeReference).GetField("etype", BindingFlags.NonPublic | BindingFlags.Instance);
			initialized = true;
		}

		public static ElementType GetEType(this TypeReference _this)
		{
			if (!initialized) Initialize();
			return (ElementType)etypeFieldInfo.GetValue(_this);
		}

		public static void SetEType(this TypeReference _this, ElementType value)
		{
			if (!initialized) Initialize();
			etypeFieldInfo.SetValue(_this, value);
		}
	}
}
