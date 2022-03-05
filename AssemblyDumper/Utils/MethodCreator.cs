using AsmResolver.DotNet.Signatures;

namespace AssemblyDumper.Utils
{
	internal static class MethodCreator
	{
		public static MethodDefinition AddMethod(this TypeDefinition type, string methodName, MethodAttributes methodAttributes, ITypeDefOrRef returnType)
		{
			return type.AddMethod(methodName, methodAttributes, returnType.ToTypeSignature());
		}

		public static MethodDefinition AddMethod(this TypeDefinition type, string methodName, MethodAttributes methodAttributes, TypeSignature returnType)
		{
			MethodDefinition method = CreateMethod(methodName, methodAttributes, returnType);
			type.Methods.Add(method);
			return method;
		}

		public static MethodDefinition CreateMethod(string methodName, MethodAttributes methodAttributes, ITypeDefOrRef returnType)
		{
			return CreateMethod(methodName, methodAttributes, returnType.ToTypeSignature());
		}

		public static MethodDefinition CreateMethod(string methodName, MethodAttributes methodAttributes, TypeSignature returnType)
		{
			bool isStatic = (methodAttributes & MethodAttributes.Static) != 0;
			MethodSignature methodSignature;
			if (isStatic)
				methodSignature = MethodSignature.CreateStatic(returnType);
			else
				methodSignature = MethodSignature.CreateInstance(returnType);

			MethodDefinition result = new MethodDefinition(methodName, methodAttributes, methodSignature);

			result.CilMethodBody = new CilMethodBody(result);

			return result;
		}

		public static Parameter AddParameter(this MethodDefinition method, string parameterName, ITypeDefOrRef parameterType)
		{
			return method.AddParameter(parameterName, parameterType.ToTypeSignature());
		}

		public static Parameter AddParameter(this MethodDefinition method, string parameterName, TypeSignature parameterSignature)
		{
			ParameterDefinition parameterDefinition = new ParameterDefinition((ushort)(method.Signature!.ParameterTypes.Count + 1), parameterName, default);
			method.Signature.ParameterTypes.Add(parameterSignature);
			method.ParameterDefinitions.Add(parameterDefinition);
			Assertions.AssertEquality(method.Signature.ParameterTypes.Count, method.ParameterDefinitions.Count);
			method.Parameters.PullUpdatesFromMethodSignature();
			return method.Parameters.Single(parameter => parameter.Name == parameterName && parameter.ParameterType == parameterSignature);
		}
	}
}
