using AssetRipper.AssemblyDumper.Methods;

namespace AssetRipper.AssemblyDumper.Types;

public static class ExceptionCreator
{
	public static TypeDefinition CreateSimpleException(CachedReferenceImporter importer, string @namespace, string name, string errorMessage)
	{
		ITypeDefOrRef exceptionRef = importer.ImportType<Exception>();
		TypeDefinition type = new TypeDefinition(@namespace, name, TypeAttributes.Public | TypeAttributes.Sealed, exceptionRef);
		importer.TargetModule.TopLevelTypes.Add(type);
		IMethodDefOrRef exceptionConstructor = importer.ImportConstructor<Exception>(1);
		MethodDefinition constructor = type.AddEmptyConstructor();
		CilInstructionCollection instructions = constructor.CilMethodBody!.Instructions;
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldstr, errorMessage);
		instructions.Add(CilOpCodes.Call, exceptionConstructor);
		instructions.Add(CilOpCodes.Ret);
		return type;
	}
}
