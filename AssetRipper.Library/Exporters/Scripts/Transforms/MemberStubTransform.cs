using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;

namespace AssetRipper.Library.Exporters.Scripts.Transforms
{
	/// <summary>
	/// Converts all members to stubs. Is activated for Scripted Level 1.
	/// </summary>
	internal class MemberStubTransform : DepthFirstAstVisitor, IAstTransform
	{
		private bool SupportsDefaultInterfaceImplementations { get; }
		
		public MemberStubTransform(UnityVersion unityVersion)
		{
			SupportsDefaultInterfaceImplementations = unityVersion.IsGreaterEqual("2021.2");
		}
		
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
					// Abstract and extern methods does not have a body
					if (method.Modifiers.HasFlag(Modifiers.Abstract) || method.Modifiers.HasFlag(Modifiers.Extern))
					{
						continue;
					}

					if (!SupportsDefaultInterfaceImplementations && typeDeclaration.ClassType == ClassType.Interface)
					{
						continue;
					}

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
