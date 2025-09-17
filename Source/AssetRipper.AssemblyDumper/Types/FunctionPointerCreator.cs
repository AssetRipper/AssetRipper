namespace AssetRipper.AssemblyDumper.Types;

public static class FunctionPointerCreator
{
	public static FunctionPointerTypeSignature CreateUnmanaged(TypeSignature returnType, IEnumerable<TypeSignature> parameterTypes)
	{
		MethodSignature methodSignature = new MethodSignature(CallingConventionAttributes.Unmanaged | CallingConventionAttributes.C, returnType, parameterTypes);
		return new FunctionPointerTypeSignature(methodSignature);
	}
}
