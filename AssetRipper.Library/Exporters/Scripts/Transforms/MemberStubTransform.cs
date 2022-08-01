using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;

namespace AssetRipper.Library.Exporters.Scripts.Transform
{
	/// <summary>
	/// Converts all members to stubs. Is activated for Scripted Level 1.
	/// </summary>
	internal class MemberStubTransform : DepthFirstAstVisitor, IAstTransform
	{
		public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
		{
			foreach (EntityDeclaration? member in typeDeclaration.Members)
			{
				if (member is TypeDeclaration nestedType)
				{
					VisitTypeDeclaration(nestedType);
				}
				else if (member is ConstructorDeclaration constructor)
				{
					constructor.Body = new BlockStatement();
				}
				else if (member is PropertyDeclaration property)
				{
					if (!property.Getter.IsNull && !property.Getter.Body.IsNull)
					{
						property.Getter.Body = new BlockStatement();
						property.Getter.Body.Statements.Add(new ReturnStatement(new DefaultValueExpression(property.ReturnType.Clone())));
					}
					if (!property.Setter.IsNull && !property.Setter.Body.IsNull)
					{
						property.Setter.Body = new BlockStatement();
					}
					member.Remove();
				}
				else if (member is CustomEventDeclaration ev)
				{
					if (!ev.AddAccessor.IsNull && !ev.AddAccessor.Body.IsNull)
					{
						ev.AddAccessor.Body = new BlockStatement();
					}
					if (!ev.RemoveAccessor.IsNull && !ev.RemoveAccessor.Body.IsNull)
					{
						ev.RemoveAccessor.Body = new BlockStatement();
					}
				}
				else if (member is MethodDeclaration method)
				{
					method.Body = new BlockStatement();
					if (method.ReturnType is not PrimitiveType returnType || returnType.Keyword != "void")
					{
						method.Body.Statements.Add(new ReturnStatement(new DefaultValueExpression(method.ReturnType.Clone())));
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
