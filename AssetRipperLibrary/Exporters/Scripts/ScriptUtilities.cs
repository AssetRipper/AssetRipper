using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using System.Collections.Generic;

namespace AssetRipper.Library.Exporters.Scripts
{
	/// <summary>
	/// Utilities for script exporting to reduce code duplication.
	/// </summary>
	internal static class ScriptUtilities
	{
		public static AstType ConvertType(TypeSystemAstBuilder typeSystem, IType type)
		{
			AstType nonGenericType = typeSystem.ConvertType(new FullTypeName(type.FullName));
			return AddGenericParameters(typeSystem, nonGenericType, type.TypeArguments);
		}

		private static AstType AddGenericParameters(TypeSystemAstBuilder typeSystem, AstType type, IReadOnlyList<IType> parameters)
		{
			if (type is SimpleType simpleType)
			{
				return AddGenericParameters(typeSystem, simpleType, parameters);
			}
			else if (type is MemberType memberType)
			{
				return AddGenericParameters(typeSystem, memberType, parameters);
			}
			else if (type is ComposedType composedType)
			{
				return AddGenericParameters(typeSystem, composedType, parameters);
			}
			return type;
		}

		private static AstType AddGenericParameters(TypeSystemAstBuilder typeSystem, SimpleType type, IReadOnlyList<IType> parameters)
		{
			foreach (IType parameter in parameters)
			{
				type.TypeArguments.Add(ConvertType(typeSystem, parameter));
			}
			return type;
		}
		private static AstType AddGenericParameters(TypeSystemAstBuilder typeSystem, MemberType type, IReadOnlyList<IType> parameters)
		{
			foreach (IType parameter in parameters)
			{
				type.TypeArguments.Add(ConvertType(typeSystem, parameter));
			}
			return type;
		}
		private static AstType AddGenericParameters(TypeSystemAstBuilder typeSystem, ComposedType type, IReadOnlyList<IType> parameters)
		{
			AddGenericParameters(typeSystem, type.BaseType, parameters);
			return type;
		}
	}
}
