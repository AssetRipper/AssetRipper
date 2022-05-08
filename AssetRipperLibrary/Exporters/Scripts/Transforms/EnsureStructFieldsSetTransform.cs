using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using System.Diagnostics;
using System.Linq;

namespace AssetRipper.Library.Exporters.Scripts.Transforms
{
	/// <summary>
	/// A transform that ensures all fields of a struct are assigned in each
	/// declared constructor.
	/// </summary>
	internal class EnsureStructFieldsSetTransform : DepthFirstAstVisitor, IAstTransform
	{
		public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
		{
			base.VisitTypeDeclaration(typeDeclaration);

			if (typeDeclaration.ClassType == ClassType.Struct)
			{

				foreach (ConstructorDeclaration? constructorDeclaration in typeDeclaration.Members.Select((member) => member as ConstructorDeclaration).Where((constructor) => constructor is not null))
				{
					Debug.Assert(constructorDeclaration != null);

					if ((constructorDeclaration.Modifiers & Modifiers.Static) == Modifiers.Static)
					{
						continue;
					}

					if (constructorDeclaration.Initializer != null)
					{
						continue;
					}

					Debug.Assert(constructorDeclaration.Body != null);

					foreach (FieldDeclaration? fieldDeclaration in typeDeclaration.Members.Select((member) => member as FieldDeclaration).Where((field) => field is not null))
					{
						Debug.Assert(fieldDeclaration != null);

						ExpressionStatement assignment = new(new AssignmentExpression(new MemberReferenceExpression(new ThisReferenceExpression(), fieldDeclaration.Name), new DefaultValueExpression(fieldDeclaration.ReturnType.Clone())));
						Statement? firstStatement = constructorDeclaration.Body.Statements.FirstOrDefault();
						if (firstStatement == null)
						{
							constructorDeclaration.Body.Statements.Add(assignment);
						}
						else
						{
							constructorDeclaration.Body.Statements.InsertBefore(firstStatement, assignment);
						}
					}
				}
			}
		}

		public void Run(AstNode rootNode, TransformContext context)
		{
			rootNode.AcceptVisitor(this);
		}
	}
}
