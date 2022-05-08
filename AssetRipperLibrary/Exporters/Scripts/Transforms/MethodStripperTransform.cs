using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;

namespace AssetRipper.Library.Exporters.Scripts.Transform
{
	/// <summary>
	/// Remove all members from type except for fields and nested types
	/// </summary>
	internal class MethodStripperTransform : DepthFirstAstVisitor, IAstTransform
	{
		public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
		{
			foreach (EntityDeclaration? member in typeDeclaration.Members)
			{
				if (member is TypeDeclaration nestedType)
				{
					this.VisitTypeDeclaration(nestedType);
				}
				else if (member is ConstructorDeclaration)
				{
					// TODO: Check if constructor is for an attribute type.
					//		 We should keep attribute constructors so fields
					//       Annotated with custom attributes work properly
					member.Remove();
				}
				else if (member is not FieldDeclaration && member is not EnumMemberDeclaration)
				{
					member.Remove();
				}
			}
		}

		public void Run(AstNode rootNode, TransformContext context)
		{
			rootNode.AcceptVisitor(this);
		}
	}
}
