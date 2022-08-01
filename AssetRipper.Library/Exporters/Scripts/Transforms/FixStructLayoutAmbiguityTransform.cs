using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using System.Runtime.InteropServices;
using Attribute = ICSharpCode.Decompiler.CSharp.Syntax.Attribute;

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
			if (typeDeclaration.ClassType != ClassType.Struct && typeDeclaration.ClassType != ClassType.Class)
			{
				return;
			}

			foreach (AttributeSection attributeSection in typeDeclaration.Attributes)
			{
				foreach (Attribute attribute in attributeSection.Attributes)
				{
					if (attribute.Type is not SimpleType attributeTypeName)
					{
						continue;
					}

					if (attributeTypeName.Identifier != "StructLayout" && attributeTypeName.Identifier != "StructLayoutAttribute")
					{
						continue;
					}

					Expression? firstArgument = attribute.Arguments.FirstOrNullObject();
					if (firstArgument != null && firstArgument is PrimitiveExpression primitive)
					{
						if (primitive.Value.Equals(0))
						{
							SimpleType type = new(nameof(LayoutKind));
							TypeReferenceExpression typeExpression = new(type);
							MemberReferenceExpression newArgument = new(typeExpression, nameof(LayoutKind.Sequential));

							attribute.Arguments.InsertBefore(firstArgument, newArgument);
							firstArgument.Remove();

							// we could implement conversion of classes to structs,
							// but can be added later if relevant.
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
