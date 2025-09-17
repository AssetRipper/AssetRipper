namespace AssetRipper.AssemblyDumper.Methods;

public static class MethodUtils
{
	public static IMethodDefOrRef MakeConstructorOnGenericType(CachedReferenceImporter importer, GenericInstanceTypeSignature instanceType, int paramCount)
	{
		MethodDefinition constructorDefinition = importer.LookupType(instanceType.GenericType.FullName)!.GetConstructor(paramCount);
		return MakeMethodOnGenericType(importer, instanceType, constructorDefinition);
	}

	public static IMethodDefOrRef MakeMethodOnGenericType(CachedReferenceImporter importer, GenericInstanceTypeSignature instanceType, IMethodDefOrRef definition)
	{
		IMethodDefOrRef? importedMethod = importer.UnderlyingImporter.ImportMethod(definition);
		return new MemberReference(instanceType.ToTypeDefOrRef(), importedMethod.Name, importedMethod.Signature);
	}

	public static IMethodDefOrRef MakeMethodOnGenericType(CachedReferenceImporter importer, GenericInstanceTypeSignature instanceType, Func<IMethodDefOrRef, bool> filter)
	{
		IMethodDefOrRef methodDefinition = importer.LookupType(instanceType.GenericType.FullName)!.Methods.Single(filter);
		return MakeMethodOnGenericType(importer, instanceType, methodDefinition);
	}

	public static MethodSpecification MakeGenericInstanceMethod(CachedReferenceImporter importer, IMethodDefOrRef method, params TypeSignature[] typeArguments)
	{
		return MakeGenericInstanceMethod(importer, method, new GenericInstanceMethodSignature(typeArguments));
	}

	private static MethodSpecification MakeGenericInstanceMethod(CachedReferenceImporter importer, IMethodDefOrRef method, GenericInstanceMethodSignature instanceMethodSignature)
	{
		return importer.UnderlyingImporter.ImportMethod(new MethodSpecification(method, instanceMethodSignature));
	}
}
