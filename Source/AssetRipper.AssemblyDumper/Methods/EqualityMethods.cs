namespace AssetRipper.AssemblyDumper.Methods;

public static class EqualityMethods
{
	public static void AddDefaultEqualityOperators(
		this TypeDefinition type,
		CachedReferenceImporter importer,
		out MethodDefinition equalityMethod,
		out MethodDefinition inequalityMethod)
	{
		equalityMethod = type.AddDefaultEqualityOperator(importer);
		inequalityMethod = type.AddDefaultInequalityOperator(importer, equalityMethod);
	}

	public static void MakeEqualityComparerGenericMethods(
		TypeSignature typeParameter,
		CachedReferenceImporter importer,
		out IMethodDefOrRef defaultReference,
		out IMethodDefOrRef equalsReference)
	{
		GenericInstanceTypeSignature genericSignature = importer.ImportType(typeof(EqualityComparer<>)).MakeGenericInstanceType(typeParameter);
		MethodDefinition defaultDefinition = importer.LookupMethod(typeof(EqualityComparer<>), m => m.Name == $"get_{nameof(EqualityComparer<int>.Default)}");
		MethodDefinition equalsDefinition = importer.LookupMethod(typeof(EqualityComparer<>), m => m.Name == nameof(EqualityComparer<int>.Equals));
		defaultReference = MethodUtils.MakeMethodOnGenericType(importer, genericSignature, defaultDefinition);
		equalsReference = MethodUtils.MakeMethodOnGenericType(importer, genericSignature, equalsDefinition);
	}

	private static MethodDefinition AddDefaultEqualityOperator(this TypeDefinition type, CachedReferenceImporter importer)
	{
		//Goal:
		//return EqualityComparer<TheClass>.Default.Equals(left, right);

		MethodDefinition method = type.AddMethod(
			"op_Equality",
			MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static,
			importer.Boolean);
		method.AddParameter(type.ToTypeSignature(), "left");
		method.AddParameter(type.ToTypeSignature(), "right");

		MakeEqualityComparerGenericMethods(type.ToTypeSignature(), importer, out IMethodDefOrRef defaultReference, out IMethodDefOrRef equalsReference);

		CilInstructionCollection instructions = method.GetInstructions();
		instructions.Add(CilOpCodes.Call, defaultReference);
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.Add(CilOpCodes.Callvirt, equalsReference);
		instructions.Add(CilOpCodes.Ret);
		return method;
	}

	private static MethodDefinition AddDefaultInequalityOperator(this TypeDefinition type, CachedReferenceImporter importer, MethodDefinition equalityMethod)
	{
		MethodDefinition method = type.AddMethod(
			"op_Inequality",
			MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static,
			importer.Boolean);
		method.AddParameter(type.ToTypeSignature(), "left");
		method.AddParameter(type.ToTypeSignature(), "right");

		CilInstructionCollection instructions = method.GetInstructions();
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.Add(CilOpCodes.Call, equalityMethod);
		instructions.Add(CilOpCodes.Ldc_I4_0);
		instructions.Add(CilOpCodes.Ceq);
		instructions.Add(CilOpCodes.Ret);
		return method;
	}
}
