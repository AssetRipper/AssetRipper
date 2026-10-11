using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using ICSharpCode.Decompiler.TypeSystem;

namespace AssetRipper.Export.UnityProjects.Scripts;

/// <summary>
/// Recovers C# expressions from patterns emitted by Unity IL postprocessors.
/// </summary>
public sealed class UnityIlPostProcessingTransform : IAstTransform
{
	public void Run(AstNode rootNode, TransformContext context)
	{
		RestoreRegistryReferences(rootNode);
		FixRpcReceiverAccess(rootNode);
		RemoveRedundantObjectConstructorCalls(rootNode);
	}

	private static void RestoreRegistryReferences(AstNode rootNode)
	{
		foreach (DirectionExpression reference in rootNode.Descendants.OfType<DirectionExpression>().ToArray())
		{
			if (reference.FieldDirection != FieldDirection.Ref || reference.Expression is not CastExpression cast ||
				reference.GetParent<TypeDeclaration>()?.GetSymbol() is not ITypeDefinition
				{ FullName: "Unity.Entities.CodeGeneratedRegistry.AssemblyTypeRegistry" })
			{
				continue;
			}

			IType type = cast.Type.GetResolveResult().Type;
			TypeReferenceExpression unsafeType = new(new MemberType(
				new MemberType(new MemberType(new MemberType(new SimpleType("global"), "System") { IsDoubleColon = true }, "Runtime"),
					"CompilerServices"), "Unsafe"));
			if (type.IsReferenceType == false)
			{
				reference.Expression = new InvocationExpression(new MemberReferenceExpression(unsafeType, "Unbox", cast.Type.Clone()), cast.Expression.Clone());
			}
			else if (type.IsReferenceType == true && cast.Expression is IdentifierExpression)
			{
				reference.Expression = new InvocationExpression(new MemberReferenceExpression(unsafeType, "As", new PrimitiveType("object"), cast.Type.Clone()),
					new DirectionExpression(FieldDirection.Ref, cast.Expression.Clone()));
			}
		}
	}

	private static void FixRpcReceiverAccess(AstNode rootNode)
	{
		TypeSystemAstBuilder builder = new() { AlwaysUseGlobal = true };
		foreach (MemberReferenceExpression reference in rootNode.Descendants.OfType<MemberReferenceExpression>().ToArray())
		{
			if (reference.MemberName != "__rpc_exec_stage" || reference.Target is not IdentifierExpression ||
				reference.GetParent<MethodDeclaration>() is not { Name: var methodName } || !methodName.StartsWith("__rpc_handler_", StringComparison.Ordinal) ||
				reference.GetParent<TypeDeclaration>()?.GetSymbol() is not ITypeDefinition type)
			{
				continue;
			}
			reference.Target = new ParenthesizedExpression(new CastExpression(builder.ConvertType(type), reference.Target.Clone()));
		}
	}

	private static void RemoveRedundantObjectConstructorCalls(AstNode rootNode)
	{
		foreach (InvocationExpression invocation in rootNode.Descendants.OfType<InvocationExpression>().ToArray())
		{
			if (invocation.Target is MemberReferenceExpression { Target: BaseReferenceExpression } && invocation.Arguments.Count == 0 &&
				invocation.GetSymbol() is IMethod { IsConstructor: true, DeclaringType.FullName: "System.Object" } &&
				invocation.Parent is ExpressionStatement statement && invocation.GetParent<ConstructorDeclaration>() is not null)
			{
				statement.Remove();
			}
		}
	}
}
