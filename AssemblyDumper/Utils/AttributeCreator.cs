namespace AssemblyDumper.Utils
{
	public static class AttributeCreator
	{
		public static TypeDefinition CreateDefaultAttribute(string @namespace, string name, AttributeTargets targets)
		{
			ITypeDefOrRef systemAttributeReference = SharedState.Importer.ImportSystemType("System.Attribute");
			ITypeDefOrRef attributeTargetsReference = SharedState.Importer.ImportSystemType("System.AttributeTargets");

			IMethodDefOrRef constructorMethod = SystemTypeGetter.LookupSystemType("System.AttributeUsageAttribute")!.GetAllConstructors().First(c => c.Parameters.Count == 1 && c.Parameters[0].ParameterType.Name == "AttributeTargets");

			var attributeDefinition = new TypeDefinition(@namespace, name, TypeAttributes.Public | TypeAttributes.BeforeFieldInit, systemAttributeReference);

			SharedState.Module.TopLevelTypes.Add(attributeDefinition);
			ConstructorUtils.AddDefaultConstructor(attributeDefinition);

			var attrDef = new CustomAttribute((ICustomAttributeType?)SharedState.Importer.ImportMethod(constructorMethod), new CustomAttributeSignature());
			attrDef.AddFixedArgument(attributeTargetsReference.ToTypeSignature(), targets);
			attributeDefinition.CustomAttributes.Add(attrDef);

			return attributeDefinition;
		}

		public static TypeDefinition CreateSingleValueAttribute(string @namespace, string name, AttributeTargets targets, string fieldName, TypeSignature fieldType, bool hasDefaultConstructor, out MethodDefinition singleParamConstructor)
		{
			ITypeDefOrRef systemAttributeReference = SharedState.Importer.ImportSystemType("System.Attribute");
			ITypeDefOrRef attributeTargetsReference = SharedState.Importer.ImportSystemType("System.AttributeTargets");

			IMethodDefOrRef usageConstructorMethod = SharedState.Importer.ImportMethod(SystemTypeGetter.LookupSystemType("System.AttributeUsageAttribute")!.GetAllConstructors().First(c => c.Parameters.Count == 1 && c.Parameters[0].ParameterType.Name == "AttributeTargets"));
			IMethodDefOrRef defaultAttributeConstructor = SharedState.Importer.ImportMethod(SystemTypeGetter.LookupSystemType("System.Attribute")!.GetDefaultConstructor());

			var attributeDefinition = new TypeDefinition(@namespace, name, TypeAttributes.Public | TypeAttributes.BeforeFieldInit, systemAttributeReference);

			SharedState.Module.TopLevelTypes.Add(attributeDefinition);
			if (hasDefaultConstructor)
			{
				var defaultConstructor = new MethodDefinition(
					".ctor",
					MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RuntimeSpecialName,
					MethodSignature.CreateInstance(SystemTypeGetter.Void)
				);

				var defaultProcessor = defaultConstructor.CilMethodBody!.Instructions;
				defaultProcessor.Add(CilOpCodes.Ldarg_0);
				defaultProcessor.Add(CilOpCodes.Call, defaultAttributeConstructor);
				defaultProcessor.Add(CilOpCodes.Ret);

				attributeDefinition.Methods.Add(defaultConstructor);
			}


			var attrDef = new CustomAttribute((ICustomAttributeType?)SharedState.Importer.ImportMethod(usageConstructorMethod), new CustomAttributeSignature());
			attrDef.AddFixedArgument(attributeTargetsReference.ToTypeSignature(), targets);
			attributeDefinition.CustomAttributes.Add(attrDef);


			var field = new FieldDefinition(fieldName, FieldAttributes.Public, FieldSignature.CreateInstance(fieldType));
			attributeDefinition.Fields.Add(field);


			singleParamConstructor = new MethodDefinition(
				".ctor",
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RuntimeSpecialName,
				MethodSignature.CreateInstance(SystemTypeGetter.Void)
			);
			singleParamConstructor.AddParameter(fieldName, fieldType);

			var processor = singleParamConstructor.CilMethodBody!.Instructions;
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Call, defaultAttributeConstructor);
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldarg_1);
			processor.Add(CilOpCodes.Stfld, field);
			processor.Add(CilOpCodes.Ret);

			attributeDefinition.Methods.Add(singleParamConstructor);


			return attributeDefinition;
		}
	}
}
