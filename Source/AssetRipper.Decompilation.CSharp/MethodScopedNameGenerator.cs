using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using System.Text;

namespace AssetRipper.Decompilation.CSharp;

public class MethodScopedNameGenerator : TypeScopedNameGenerator
{
	private readonly string[] _methodTypeParameters;

	public ReadOnlySpan<string> MethodTypeParameters => _methodTypeParameters;

	private MethodScopedNameGenerator(TypeDefinition type, string[] methodTypeParameters) : base(type)
	{
		_methodTypeParameters = methodTypeParameters;
	}

	public override StringBuilder VisitGenericParameter(GenericParameterSignature signature, StringBuilder state)
	{
		if (signature.ParameterType == GenericParameterType.Method)
		{
			return state.Append(_methodTypeParameters[signature.Index]);
		}
		else
		{
			return base.VisitGenericParameter(signature, state);
		}
	}

	public static NameGenerator Create(MethodDefinition method)
	{
		if (method.DeclaringType is null)
		{
			throw new ArgumentException("Method must have a declaring type", nameof(method));
		}

		if (method.GenericParameters.Count == 0)
		{
			return Create(method.DeclaringType);
		}
		else
		{
			return new MethodScopedNameGenerator(method.DeclaringType, GetTypeParameterNames(method));
		}
	}

	private static string[] GetTypeParameterNames(MethodDefinition method)
	{
		if (method.GenericParameters.Count == 0)
		{
			return Array.Empty<string>();
		}
		else
		{
			string[] strings = new string[method.GenericParameters.Count];
			for (int i = 0; i < strings.Length; i++)
			{
				//It's extremely unlikely that only some of the generic parameters are unnamed,
				//so we don't worry about pontential conflicts here.
				strings[i] = method.GenericParameters[i].Name ?? $"M{i + 1}";
			}
			return strings;
		}
	}
}
