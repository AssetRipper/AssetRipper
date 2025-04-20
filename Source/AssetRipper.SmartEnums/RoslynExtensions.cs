using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AssetRipper.SmartEnums;

internal static class RoslynExtensions
{
	public static bool IsPartial(this MemberDeclarationSyntax c)
	{
		return c.Modifiers.Any(SyntaxKind.PartialKeyword);
	}

	public static bool IsReadOnly(this MemberDeclarationSyntax c)
	{
		return c.Modifiers.Any(SyntaxKind.ReadOnlyKeyword);
	}
}
