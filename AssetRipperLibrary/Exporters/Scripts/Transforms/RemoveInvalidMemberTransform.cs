using AssetRipper.Core.Logging;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;

namespace AssetRipper.Library.Exporters.Scripts.Transforms
{
	/// <summary>
	/// A transformer that removed all invalid members
	/// </summary>
	internal class RemoveInvalidMemberTransform : DepthFirstAstVisitor, IAstTransform
	{
        private static bool RemoveInvalidEntity(EntityDeclaration entityDeclaration)
		{
			if (!IsValidName(entityDeclaration.Name))
			{
				entityDeclaration.Remove();
				return true;
			}
			else if (entityDeclaration is FieldDeclaration fieldDeclaration)
			{
				foreach (VariableInitializer variable in fieldDeclaration.Variables)
				{
					if (!IsValidName(variable.Name))
					{
						variable.Remove();
					}
				}

				if (fieldDeclaration.Variables.Count == 0)
				{
					fieldDeclaration.Remove();
					return true;
				}
			}

			static bool IsValidName(string name)
			{
				return !name.StartsWith("<");
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
		}

		public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
		{
			if (RemoveInvalidEntity(fieldDeclaration))
			{
				return;
			}

			base.VisitFieldDeclaration(fieldDeclaration);
		}

		public override void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
		{
			base.VisitConstructorDeclaration(constructorDeclaration);
		}

		public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
		{
			if (RemoveInvalidEntity(propertyDeclaration))
			{
				return;
			}

			base.VisitPropertyDeclaration(propertyDeclaration);
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
