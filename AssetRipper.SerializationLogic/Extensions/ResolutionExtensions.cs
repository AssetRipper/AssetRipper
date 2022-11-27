namespace AssetRipper.SerializationLogic.Extensions
{
	public static class ResolutionExtensions
	{
		public static TypeDefinition CheckedResolve(this ITypeDescriptor reference)
		{
			if (reference.Module == null)
			{
				throw new ResolutionException(reference);
			}

			if (reference is not TypeDefinition definition)
			{
				definition = reference.Resolve() ?? throw new ResolutionException(reference);
			}

			return definition;
		}

		public static MethodDefinition CheckedResolve(this IMethodDefOrRef reference)
		{
			if (reference.Module == null)
			{
				throw new ResolutionException(reference);
			}

			if (reference is not MethodDefinition definition)
			{
				definition = reference.Resolve() ?? throw new ResolutionException(reference);
			}

			return definition;
		}
	}
}
