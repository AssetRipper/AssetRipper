namespace Mono.Cecil.Extensions
{
	public static class MetadataResolverExtensions 
	{
		/*
		 * MetadataResolver.AreSame(TypeReference, TypeReference)
		 */

		
		/*public static bool AreSame (TypeReference a, TypeReference b)
		{
			if (ReferenceEquals (a, b))
				return true;

			if (a == null || b == null)
				return false;

			if (a.etype != b.etype)
				return false;

			if (a.IsGenericParameter)
				return AreSame ((GenericParameter) a, (GenericParameter) b);

			if (a.IsTypeSpecification ())
				return AreSame ((TypeSpecification) a, (TypeSpecification) b);

			if (a.Name != b.Name || a.Namespace != b.Namespace)
				return false;

			//TODO: check scope

			return AreSame (a.DeclaringType, b.DeclaringType);
		}*/
	}
}
