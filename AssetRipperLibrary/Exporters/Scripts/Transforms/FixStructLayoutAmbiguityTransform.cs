using AssetRipper.Core.Logging;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using System.Runtime.InteropServices;

namespace AssetRipper.Library.Exporters.Scripts.Transforms
{
	/// <summary>
	/// Fixes ILSpy generated code which can generate <see cref="StructLayoutAttribute"/>s on
	/// structs. For <see cref="LayoutKind.Sequential"/>, ILSpy generated the attribute as
	/// <c>StructLayout(0, ...)</c>, which is ambiguous between
	/// <see cref="StructLayoutAttribute(short)"/>, and
	/// <see cref="StructLayoutAttribute(LayoutKind)"/>
	/// </summary>
	internal class FixStructLayoutAmbiguityTransform : DepthFirstAstVisitor, IAstTransform
	{
		public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
		{
			base.VisitTypeDeclaration(typeDeclaration);
			if (typeDeclaration.ClassType != ClassType.Struct)
			{
				return;
			}

			foreach (AttributeSection attributeSection in typeDeclaration.Attributes)
			{
				foreach (Attribute attribute in attributeSection.Attributes)
				{
					if (attribute.Type is not SimpleType attributeTypeName || attributeTypeName.Identifier != "StructLayout")
					{
						continue;
					}

					if (attribute.HasArgumentList)
					{
						Expression? firstArgument = attribute.Arguments.FirstOrNullObject();
						if (firstArgument != null && firstArgument is PrimitiveExpression primitive &&
							primitive.Format == LiteralFormat.DecimalNumber)
						{
							Logger.Info(LogCategory.Debug, "Primitive Value: " + primitive.Value);
							if (primitive.Value.Equals(0))
							{
								SimpleType type = new(nameof(StructLayoutAttribute));
								TypeReferenceExpression typeExpression = new(type);
								MemberReferenceExpression newArgument = new(typeExpression, nameof(LayoutKind.Sequential));

								attribute.Arguments.InsertBefore(newArgument, firstArgument);
								firstArgument.Remove();
							}
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
