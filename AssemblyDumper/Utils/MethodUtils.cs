using AsmResolver.DotNet.Signatures;

namespace AssemblyDumper.Utils
{
	public static class MethodUtils
	{
		public static IMethodDefOrRef MakeConstructorOnGenericType(GenericInstanceTypeSignature instanceType, int paramCount)
		{
			MethodDefinition constructorDefinition = instanceType.GenericType.Resolve()!.Methods.Single(m => m.Name == ".ctor" && m.Parameters.Count == paramCount);
			return MakeMethodOnGenericType(instanceType, constructorDefinition);
		}

		public static IMethodDefOrRef MakeMethodOnGenericType(GenericInstanceTypeSignature instanceType, MethodDefinition definition)
		{
			IMethodDefOrRef? importedMethod = SharedState.Importer.ImportMethod(definition);
			return new MemberReference(instanceType.ToTypeDefOrRef(), importedMethod.Name, importedMethod.Signature);
		}

		public static IMethodDefOrRef MakeMethodOnGenericType(GenericInstanceTypeSignature instanceType, string methodName)
		{
			MethodDefinition methodDefinition = instanceType.GenericType.Resolve()!.Methods.Single(m => m.Name == methodName);
			return MakeMethodOnGenericType(instanceType, methodDefinition);
		}

		public static MethodSpecification MakeGenericInstanceMethod(IMethodDefOrRef method, params TypeSignature[] typeArguments)
		{
			return MakeGenericInstanceMethod(method, new GenericInstanceMethodSignature(typeArguments));
		}

		private static MethodSpecification MakeGenericInstanceMethod(IMethodDefOrRef method, GenericInstanceMethodSignature instanceMethodSignature)
		{
			return SharedState.Importer.ImportMethod(new MethodSpecification(method, instanceMethodSignature));
		}
	}
}
