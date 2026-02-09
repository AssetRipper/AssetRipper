using AssetRipper.AssemblyDumper.Methods;

namespace AssetRipper.AssemblyDumper.Extensions;

internal static class CilInstructionCollectionExtensions
{
	public static void AddNotSupportedException(this CilInstructionCollection instructions)
	{
		IMethodDefOrRef constructor = SharedState.Instance.Importer.ImportDefaultConstructor<NotSupportedException>();
		instructions.Add(CilOpCodes.Newobj, constructor);
		instructions.Add(CilOpCodes.Throw);
	}

	/*public static void AddLogStatement(this CilInstructionCollection instructions, string text)
	{
		Func<MethodDefinition, bool> func = m => m.IsStatic && m.Name == nameof(Logger.Info) && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.Name == "String";
		IMethodDefOrRef writeMethod = SharedState.Instance.Importer.ImportMethod(typeof(Logger), func);
		instructions.Add(CilOpCodes.Ldstr, text);
		instructions.Add(CilOpCodes.Call, writeMethod);
	}*/
}
