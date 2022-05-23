using AssetRipper.Core.Logging;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using System.Collections.Generic;
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

			if (typeDeclaration.ClassType != ClassType.Struct)
			{
				return;
			}

			List<(AstType, List<string>)> requiredFields = new();
			foreach (EntityDeclaration member in typeDeclaration.Members)
			{
				if (member is FieldDeclaration field)
				{
					(AstType, List<string>) fieldInfo = new(field.ReturnType, new());
					if ((field.Modifiers & Modifiers.Static) == Modifiers.Static)
					{
						continue;
					}

					foreach (VariableInitializer variable in field.Variables)
					{
						fieldInfo.Item2.Add(variable.Name);
					}

					requiredFields.Add(fieldInfo);
				}
			}

			IEnumerable<ConstructorDeclaration> constructors = typeDeclaration.Members.Select((member) => member as ConstructorDeclaration).Where((constructor) => constructor is not null)!;

			foreach (ConstructorDeclaration? constructorDeclaration in constructors)
			{
				Debug.Assert(constructorDeclaration != null);

				if ((constructorDeclaration.Modifiers & Modifiers.Static) == Modifiers.Static)
				{
					continue;
				}

				if (constructorDeclaration.Initializer != null && !constructorDeclaration.Initializer.IsNull)
				{
					continue;
				}

				Debug.Assert(constructorDeclaration.Body != null);

				foreach ((AstType, List<string>) requiredField in requiredFields)
				{
					foreach (string fieldName in requiredField.Item2)
					{
						ExpressionStatement assignment = new(new AssignmentExpression(new MemberReferenceExpression(new ThisReferenceExpression(), fieldName), new DefaultValueExpression(requiredField.Item1.Clone())));
						Statement? firstStatement = constructorDeclaration.Body.Statements.FirstOrDefault();
						if (firstStatement == null || firstStatement.IsNull)
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
