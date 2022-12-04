using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;

namespace AssetRipper.Library.Exporters.Scripts.Transforms
{
	/// <summary>
	/// A transformer that ensure all out parameters on methods and constructors are set.
	/// </summary>
	internal class EnsureOutParamsSetTransform : DepthFirstAstVisitor, IAstTransform
	{
		public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
		{
			base.VisitMethodDeclaration(methodDeclaration);

			VisitEntityDeclaration(
				methodDeclaration,
				methodDeclaration.Body,
				methodDeclaration.Parameters
			);
		}

		public override void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
		{
			base.VisitConstructorDeclaration(constructorDeclaration);

			VisitEntityDeclaration(
				constructorDeclaration,
				constructorDeclaration.Body,
				constructorDeclaration.Parameters
			);
		}

		private void VisitEntityDeclaration(
			EntityDeclaration entityDeclaration,
			BlockStatement? body,
			AstNodeCollection<ParameterDeclaration> parameters)
		{
			if (body == null || body.IsNull)
			{
				return;
			}

			foreach (ParameterDeclaration parameter in parameters)
			{
				if (parameter.ParameterModifier != ParameterModifier.Out)
				{
					continue;
				}

				ExpressionStatement assignment = new(
					new AssignmentExpression(
						new IdentifierExpression(parameter.Name),
						new DefaultValueExpression(parameter.Type.Clone())
					)
				);

				Statement? firstStatement = body.Statements.FirstOrNullObject();
				if (firstStatement == null || firstStatement.IsNull)
				{
					body.Statements.Add(assignment);
				}
				else
				{
					body.Statements.InsertBefore(firstStatement, assignment);
				}
			}
		}

		public void Run(AstNode rootNode, TransformContext context)
		{
			rootNode.AcceptVisitor(this);
		}
	}
}
