namespace Mono.Cecil.Extensions
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

			var definition = module.Resolve(typeReference);
			if (definition == null)
				return typeReference;

			return definition;
		}
	}
}
