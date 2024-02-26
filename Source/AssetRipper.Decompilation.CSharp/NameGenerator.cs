using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using System.Text;
using System.Text.RegularExpressions;

namespace AssetRipper.Decompilation.CSharp;

public abstract partial class NameGenerator : ITypeSignatureVisitor<StringBuilder, StringBuilder>
{
	private protected const string GlobalPrefix = "global::";

	public string GetFullName(ITypeDefOrRef type)
	{
		return GetFullName(type.ToTypeSignature());
	}

	public string GetFullName(TypeSignature signature)
	{
		return Visit(signature, new StringBuilder()).ToString();
	}

	public StringBuilder Visit(TypeSignature signature, StringBuilder state)
	{
		return signature.AcceptVisitor(this, state);
	}

	public virtual StringBuilder VisitArrayType(ArrayTypeSignature signature, StringBuilder state)
	{
		throw new NotSupportedException();
	}

	public virtual StringBuilder VisitBoxedType(BoxedTypeSignature signature, StringBuilder state)
	{
		throw new NotSupportedException();
	}

	public virtual StringBuilder VisitByReferenceType(ByReferenceTypeSignature signature, StringBuilder state)
	{
		throw new NotSupportedException();
	}

	public virtual StringBuilder VisitCorLibType(CorLibTypeSignature signature, StringBuilder state)
	{
		return state.Append(signature.ElementType switch
		{
			ElementType.Void => "void",
			ElementType.Boolean => "bool",
			ElementType.Char => "char",
			ElementType.I1 => "sbyte",
			ElementType.U1 => "byte",
			ElementType.I2 => "short",
			ElementType.U2 => "ushort",
			ElementType.I4 => "int",
			ElementType.U4 => "uint",
			ElementType.I8 => "long",
			ElementType.U8 => "ulong",
			ElementType.R4 => "float",
			ElementType.R8 => "double",
			ElementType.String => "string",
			ElementType.I => "nint",
			ElementType.U => "nuint",
			ElementType.Object => "object",
			_ => GlobalPrefix + signature.FullName,
		});
	}

	public virtual StringBuilder VisitCustomModifierType(CustomModifierTypeSignature signature, StringBuilder state)
	{
		throw new NotSupportedException();
	}

	public virtual StringBuilder VisitFunctionPointerType(FunctionPointerTypeSignature signature, StringBuilder state)
	{
		throw new NotSupportedException();
	}

	public virtual StringBuilder VisitGenericInstanceType(GenericInstanceTypeSignature signature, StringBuilder state)
	{
		return AppendTypeParameters(
			Visit(signature.GenericType.ToTypeSignature(), state),
			signature.TypeArguments);
	}

	public virtual StringBuilder VisitGenericParameter(GenericParameterSignature signature, StringBuilder state)
	{
		throw new NotSupportedException();
	}

	public virtual StringBuilder VisitPinnedType(PinnedTypeSignature signature, StringBuilder state)
	{
		return signature.BaseType.AcceptVisitor(this, state);
	}

	public virtual StringBuilder VisitPointerType(PointerTypeSignature signature, StringBuilder state)
	{
		return signature.BaseType.AcceptVisitor(this, state).Append('*');
	}

	public virtual StringBuilder VisitSentinelType(SentinelTypeSignature signature, StringBuilder state)
	{
		throw new NotSupportedException();
	}

	public virtual StringBuilder VisitSzArrayType(SzArrayTypeSignature signature, StringBuilder state)
	{
		return signature.BaseType.AcceptVisitor(this, state).Append("[]");
	}

	public virtual StringBuilder VisitTypeDefOrRef(TypeDefOrRefSignature signature, StringBuilder state)
	{
		return state.Append(GlobalPrefix)
			.Append(RemoveGenericEnding(signature.Type.FullName));
		//This is probably wrong for nested types of generic types.
		//It will be necessary to do our own name generation instead of relying on FullName from AsmResolver.
	}

	private StringBuilder AppendTypeParameters(StringBuilder state, IList<TypeSignature> typeArguments)
	{
		if (typeArguments.Count > 0)
		{
			state.Append('<');
			AppendCommaSeparatedCollection(state, typeArguments, (StringBuilder s, TypeSignature t) => t.AcceptVisitor(this, s));
			state.Append('>');
		}

		return state;
	}

	private static StringBuilder AppendCommaSeparatedCollection<T>(StringBuilder state, IList<T> collection, Action<StringBuilder, T> action)
	{
		for (int i = 0; i < collection.Count; i++)
		{
			action(state, collection[i]);
			if (i < collection.Count - 1)
			{
				state.Append(", ");
			}
		}

		return state;
	}

	private static string RemoveGenericEnding(string name)
	{
		Match match = GenericName().Match(name);
		if (match.Success)
		{
			return name[..match.Index];
		}
		else
		{
			return name;
		}
	}

	[GeneratedRegex(@"(`[0-9]+)$")]
	private static partial Regex GenericName();
}
