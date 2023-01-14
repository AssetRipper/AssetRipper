using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures.Types;

namespace AssetRipper.Processing.Assemblies;

internal static class AsmResolverExtensions
{
	public static CilLocalVariable AddLocalVariable(this CilInstructionCollection instructions, TypeSignature variableType)
	{
		CilLocalVariable variable = new CilLocalVariable(variableType);
		instructions.Owner.LocalVariables.Add(variable);
		return variable;
	}

	public static bool IsManagedMethodWithBody(this MethodDefinition managedMethod)
	{
		return managedMethod.Managed
			&& !managedMethod.IsAbstract
			&& !managedMethod.IsPInvokeImpl
			&& !managedMethod.IsInternalCall
			&& !managedMethod.IsNative
			&& !managedMethod.IsRuntime;
	}
}
