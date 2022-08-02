using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;

namespace AssetRipper.Library.Exporters.Scripts.Transforms
{
	/// <summary>
	/// A transformer that ensure all out parameters on methods are set.
	/// </summary>
	internal class EnsureOutParamsSetTransform : DepthFirstAstVisitor, IAstTransform
	{
		public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
		{
			base.VisitMethodDeclaration(methodDeclaration);

			if (methodDeclaration.Body == null || methodDeclaration.Body.IsNull)
			{
				return;
			}

			foreach (ParameterDeclaration parameter in methodDeclaration.Parameters)
			{
				if ((parameter.ParameterModifier & ParameterModifier.Out) != ParameterModifier.Out)
				{
					continue;
				}

				ExpressionStatement assignment = new(new AssignmentExpression(new IdentifierExpression(parameter.Name), new DefaultValueExpression(parameter.Type.Clone())));
				Statement? firstStatement = methodDeclaration.Body.Statements.FirstOrNullObject();
				if (firstStatement == null || firstStatement.IsNull)
				{
					methodDeclaration.Body.Statements.Add(assignment);
				}
				else
				{
					methodDeclaration.Body.Statements.InsertBefore(firstStatement, assignment);
				}
			}
		}
		public void Run(AstNode rootNode, TransformContext context)
		{
			rootNode.AcceptVisitor(this);
		}
	}
}
