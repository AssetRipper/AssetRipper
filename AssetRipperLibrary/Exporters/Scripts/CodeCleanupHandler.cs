using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;

namespace AssetRipper.Library.Exporters.Scripts
{
	/// <summary>
	/// Cleans up code to make it reduce the number of compiler errors
	/// </summary>
	internal class CodeCleanupHandler : DepthFirstAstVisitor, IAstTransform
	{
		private readonly CodeCleanupSettings settings;

		public CodeCleanupHandler(CodeCleanupSettings? settings = null)
		{
			this.settings = settings ?? new CodeCleanupSettings();
		}

		private bool RemoveInvalidEntity(EntityDeclaration entityDeclaration)
		{
			if (!settings.RemoveInvalidMembers)
			{
				return false;
			}

			if (entityDeclaration.Name.StartsWith("<"))
			{
				entityDeclaration.Remove();
				return true;
			}


			return false;
		}

		public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
		{
			if (RemoveInvalidEntity(methodDeclaration))
			{
				return;
			}

			base.VisitMethodDeclaration(methodDeclaration);

			if (!settings.EnsureOutParametersSet || methodDeclaration.Body == null)
			{
				return;
			}

			foreach (ParameterDeclaration parameter in methodDeclaration.Parameters)
			{
				if ((parameter.ParameterModifier & ParameterModifier.Out) != ParameterModifier.Out)
				{
					continue;
				}

				ExpressionStatement assignment = new(new AssignmentExpression(new IdentifierExpression(parameter.Name), new DefaultValueExpression()));
				Statement? firstStatement = methodDeclaration.Body.Statements.FirstOrNullObject();
				if (firstStatement == null)
				{
					methodDeclaration.Body.Statements.Add(assignment);
				}
				else
				{
					methodDeclaration.Body.Statements.InsertBefore(firstStatement, assignment);
				}
			}
		}

		public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
		{
			if (RemoveInvalidEntity(fieldDeclaration))
			{
				return;
			}

			base.VisitFieldDeclaration(fieldDeclaration);
		}

		public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
		{
			if (RemoveInvalidEntity(typeDeclaration))
			{
				return;
			}

			base.VisitTypeDeclaration(typeDeclaration);
		}

		public void Run(AstNode rootNode, TransformContext context)
		{
			rootNode.AcceptVisitor(this);
		}
	}
}
