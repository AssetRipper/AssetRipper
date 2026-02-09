namespace AssetRipper.AssemblyDumper.Methods;

public static class ConstructorUtils
{
	/// <summary>
	/// Imports the default constructor for a type. Throws an exception if one doesn't exist.
	/// </summary>
	public static IMethodDefOrRef ImportDefaultConstructor<T>(this CachedReferenceImporter importer)
	{
		return importer.ImportConstructor<T>(0);
	}

	/// <summary>
	/// Imports the default constructor for a type. Throws an exception if one doesn't exist.
	/// </summary>
	public static IMethodDefOrRef ImportDefaultConstructor(this CachedReferenceImporter importer, Type type)
	{
		return importer.ImportConstructor(type, 0);
	}

	/// <summary>
	/// Gets the constructor with that number of parameters. Throws an exception if there's not exactly one.
	/// </summary>
	public static MethodDefinition GetConstructor(this TypeDefinition _this, int numParameters)
	{
		return _this.Methods.Single(m => !m.IsStatic && m.IsConstructor && m.Parameters.Count == numParameters);
	}

	/// <summary>
	/// Imports the constructor with that number of parameters. Throws an exception if there's not exactly one.
	/// </summary>
	public static IMethodDefOrRef ImportConstructor<T>(this CachedReferenceImporter importer, int numParameters)
	{
		return importer.ImportMethod<T>(m => !m.IsStatic && m.IsConstructor && m.Parameters.Count == numParameters);
	}

	/// <summary>
	/// Imports the constructor with that number of parameters. Throws an exception if there's not exactly one.
	/// </summary>
	public static IMethodDefOrRef ImportConstructor(this CachedReferenceImporter importer, Type type, int numParameters)
	{
		return importer.ImportMethod(type, m => !m.IsStatic && m.IsConstructor && m.Parameters.Count == numParameters);
	}

	/// <summary>
	/// Imports the constructor with that number of parameters. Throws an exception if there's not exactly one.
	/// </summary>
	public static IMethodDefOrRef ImportConstructor<T>(this CachedReferenceImporter importer, Func<MethodDefinition, bool> func)
	{
		return importer.ImportMethod<T>(m => !m.IsStatic && m.IsConstructor && func.Invoke(m));
	}

	public static MethodDefinition AddEmptyConstructor(this TypeDefinition typeDefinition, bool isStaticConstructor = false)
	{
		return isStaticConstructor
			? typeDefinition.AddMethod(
				".cctor",
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RuntimeSpecialName | MethodAttributes.Static,
				typeDefinition.DeclaringModule!.CorLibTypeFactory.Void)
			: typeDefinition.AddMethod(
				".ctor",
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RuntimeSpecialName,
				typeDefinition.DeclaringModule!.CorLibTypeFactory.Void);
	}

	/// <summary>
	/// Warning: base class must also have a default constructor
	/// </summary>
	public static MethodDefinition AddDefaultConstructor(this TypeDefinition typeDefinition, CachedReferenceImporter importer)
	{
		MethodDefinition defaultConstructor = typeDefinition.AddEmptyConstructor();
		CilInstructionCollection instructions = defaultConstructor.CilMethodBody!.Instructions;

		IMethodDefOrRef baseConstructor;
		if (typeDefinition.BaseType is null)
		{
			baseConstructor = importer.ImportDefaultConstructor<object>();
		}
		else
		{
			if (typeDefinition.BaseType is TypeDefinition baseType)
			{
				baseConstructor = baseType.GetDefaultConstructor();
			}
			else
			{
				MethodDefinition baseConstructorDefinition = importer.LookupType(typeDefinition.BaseType.FullName)?.GetDefaultConstructor()
					?? throw new Exception($"Could not get default constructor for {typeDefinition.BaseType.FullName}");
				baseConstructor = importer.UnderlyingImporter.ImportMethod(baseConstructorDefinition);
			}
		}

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Call, baseConstructor);
		instructions.Add(CilOpCodes.Ret);

		instructions.OptimizeMacros();

		return defaultConstructor;
	}
}