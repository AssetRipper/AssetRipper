using AssetRipper.Export.UnityProjects.Scripts;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using ICSharpCode.Decompiler.TypeSystem;
using Unity.Entities.CodeGeneratedRegistry;

namespace AssetRipper.Tests;

public class UnityIlPostProcessingTransformTests
{
	[TestCase("GetValue", "Unbox")]
	[TestCase("GetReference", "As")]
	public void RegistryCastsBecomeReferencesInsteadOfValueCopies(string methodName, string expectedMethod)
	{
		CSharpDecompiler decompiler = new(typeof(AssemblyTypeRegistry).Assembly.Location, new DecompilerSettings());
		decompiler.AstTransforms.Add(new RegistryCastInjector());
		decompiler.AstTransforms.Add(new UnityIlPostProcessingTransform());
		SyntaxTree tree = decompiler.DecompileType(new FullTypeName(typeof(AssemblyTypeRegistry).FullName!));
		MethodDeclaration method = tree.Descendants.OfType<MethodDeclaration>().Single(method => method.Name == methodName);
		DirectionExpression reference = method.Descendants.OfType<DirectionExpression>().First();
		Assert.That(reference.FieldDirection, Is.EqualTo(FieldDirection.Ref));
		Assert.That(reference.Expression, Is.TypeOf<InvocationExpression>());
		InvocationExpression invocation = (InvocationExpression)reference.Expression;
		Assert.That(((MemberReferenceExpression)invocation.Target).MemberName, Is.EqualTo(expectedMethod));
		Assert.That(invocation.Descendants.OfType<IdentifierExpression>().Any(identifier => identifier.Identifier == "value"), Is.True);
	}

	private sealed class RegistryCastInjector : IAstTransform
	{
		public void Run(AstNode rootNode, TransformContext context)
		{
			// Model the invalid ref-cast emitted from the registry's unbox IL.
			foreach (CastExpression cast in rootNode.Descendants.OfType<CastExpression>().ToArray())
			{
				cast.ReplaceWith(new DirectionExpression(FieldDirection.Ref, cast.Clone()));
			}
		}
	}
}
