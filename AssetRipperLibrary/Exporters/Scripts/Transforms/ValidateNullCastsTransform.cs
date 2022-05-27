using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;

namespace AssetRipper.Library.Exporters.Scripts.Transforms
{
	/// <summary>
	/// A transform that validates all casts of null to a type
	/// by instead using the default expression.
	/// </summary>
	internal class ValidateNullCastsTransform : DepthFirstAstVisitor, IAstTransform
	{
		public override void VisitCastExpression(CastExpression castExpression)
		{
			if (castExpression.Expression is NullReferenceExpression)
			{
				castExpression.Expression = new DefaultValueExpression(castExpression.Type.Clone());
			}
			base.VisitCastExpression(castExpression);
		}

		public void Run(AstNode rootNode, TransformContext context)
		{
			rootNode.AcceptVisitor(this);
		}
	}
}
