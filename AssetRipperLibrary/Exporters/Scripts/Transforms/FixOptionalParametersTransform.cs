using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;

namespace AssetRipper.Library.Exporters.Scripts.Transforms
{
	/// <summary>
	/// A transformer that modifiers all parameters that have an [Optional]
	/// attribute, removing that attribute and setting a default value.
	/// </summary>
	internal class FixOptionalParametersTransform : DepthFirstAstVisitor, IAstTransform
	{
		public override void VisitParameterDeclaration(ParameterDeclaration parameterDeclaration)
		{
			foreach (AttributeSection attributeSection in parameterDeclaration.Attributes)
			{
				foreach (Attribute attribute in attributeSection.Attributes)
				{
					if (attribute.Type is SimpleType type && type.Identifier == "Optional")
					{
						attribute.Remove();
						parameterDeclaration.DefaultExpression = new DefaultValueExpression(parameterDeclaration.Type.Clone());
					}
				}
				if (attributeSection.Attributes.Count == 0)
				{
					attributeSection.Remove();
				}
			}

			base.VisitParameterDeclaration(parameterDeclaration);
		}

		public void Run(AstNode rootNode, TransformContext context)
		{
			rootNode.AcceptVisitor(this);
		}
	}
}
