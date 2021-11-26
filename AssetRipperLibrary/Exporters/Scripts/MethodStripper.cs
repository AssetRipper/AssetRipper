using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;

namespace AssetRipper.Library.Exporters.Scripts
{
	/// <summary>
	/// Remove all members from type except for fields and nested types
	/// </summary>
	internal class MethodStripper : DepthFirstAstVisitor, IAstTransform
	{
		public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
		{
			foreach (var member in typeDeclaration.Members)
			{
				if (member is FieldDeclaration)
				{
					continue;
				}
				if (member is TypeDeclaration)
				{
					continue;
				}
				member.Remove();
			}
		}

		public void Run(AstNode rootNode, TransformContext context)
		{
			rootNode.AcceptVisitor(this);
		}
	}
}
