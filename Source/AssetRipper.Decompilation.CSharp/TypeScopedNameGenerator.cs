using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using System.Text;

namespace AssetRipper.Decompilation.CSharp;

public class TypeScopedNameGenerator : NameGenerator
{
	private static TypeScopedNameGenerator ParameterlessInstance { get; } = new(Array.Empty<string>());

	private readonly string[] _typeParameters;

	public ReadOnlySpan<string> TypeParameters => _typeParameters;

	private TypeScopedNameGenerator(string[] typeParameters)
	{
		_typeParameters = typeParameters;
	}

	protected TypeScopedNameGenerator(TypeDefinition type)
		: this(GetTypeParameterNames(type))
	{
	}

	public override StringBuilder VisitGenericParameter(GenericParameterSignature signature, StringBuilder state)
	{
		if (signature.ParameterType == GenericParameterType.Type)
		{
			return state.Append(_typeParameters[signature.Index]);
		}
		else
		{
			throw new NotSupportedException();
		}
	}

	public static NameGenerator Create(TypeDefinition type)
	{
		string[] typeParameters = GetTypeParameterNames(type);
		if (typeParameters.Length == 0)
		{
			return ParameterlessInstance;
		}
		else
		{
			return new TypeScopedNameGenerator(typeParameters);
		}
	}

	private static string[] GetTypeParameterNames(TypeDefinition type)
	{
		string[] parentParameters;
		if (type.DeclaringType is not null)
		{
			parentParameters = GetTypeParameterNames(type.DeclaringType);
		}
		else
		{
			parentParameters = Array.Empty<string>();
		}

		if (type.GenericParameters.Count == 0)
		{
			return parentParameters;
		}
		else
		{
			string[] strings = new string[parentParameters.Length + type.GenericParameters.Count];
			new ReadOnlySpan<string>(parentParameters).CopyTo(strings);
			for (int i = parentParameters.Length; i < strings.Length; i++)
			{
				//It's extremely unlikely that only some of the generic parameters are unnamed,
				//so we don't worry about pontential conflicts here.
				strings[i] = type.GenericParameters[i].Name ?? $"T{i + 1}";
			}
			return strings;
		}
	}
}
