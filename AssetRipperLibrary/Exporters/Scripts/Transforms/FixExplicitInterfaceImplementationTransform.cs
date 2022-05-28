using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using Attribute = ICSharpCode.Decompiler.CSharp.Syntax.Attribute;

namespace AssetRipper.Library.Exporters.Scripts.Transforms
{
	/// <summary>
	/// Fixes implicit interface implementations as ILSpy sometimes exports properties as seperate
	/// methods which won't compile.
	/// </summary>
	internal class FixExplicitInterfaceImplementationTransform : DepthFirstAstVisitor, IAstTransform
	{
		public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
		{
			if (methodDeclaration.Parent is not TypeDeclaration type)
			{
				return;
			}

			bool isSpecialName = false;
			foreach (AttributeSection attributeSection in methodDeclaration.Attributes)
			{
				foreach (Attribute attribute in attributeSection.Attributes)
				{
					if (attribute.Type is SimpleType attributeType && attributeType.Identifier == "SpecialName")
					{
						isSpecialName = true;
						attribute.Remove();
						break;
					}
				}

				if (attributeSection.Attributes.Count == 0)
				{
					attributeSection.Remove();
				}
			}
			if (!isSpecialName)
			{
				return;
			}

			SpecialMemberType memberType = SpecialMemberType.Unknown;
			string memberName = methodDeclaration.Name;
			if (memberName.StartsWith("get_"))
			{
				memberName = memberName.Substring(4);
				memberType = SpecialMemberType.Getter;
			}
			else if (memberName.StartsWith("set_"))
			{
				memberName = memberName.Substring(4);
				memberType = SpecialMemberType.Setter;
			}

			if (memberType != SpecialMemberType.Getter && memberType != SpecialMemberType.Setter)
			{
				return;
			}

			PropertyDeclaration? property = null;

			foreach (EntityDeclaration member in type.Members)
			{
				if (member is PropertyDeclaration prop)
				{
					if (prop.Name == memberName &&
						// todo: better type checking
						prop.PrivateImplementationType.ToString() == methodDeclaration.PrivateImplementationType.ToString())
					{
						property = prop;
						break;
					}
				}
			}

			if (property == null)
			{
				property = new PropertyDeclaration()
				{
					Name = memberName,
					PrivateImplementationType = methodDeclaration.PrivateImplementationType.Clone()
				};
				type.Members.Add(property);
			}


			switch (memberType)
			{
				case SpecialMemberType.Getter:
					if (property.ReturnType == AstType.Null)
					{
						property.ReturnType = methodDeclaration.ReturnType.Clone();
					}

					property.Getter = new Accessor();
					property.Getter.Body = (BlockStatement)methodDeclaration.Body.Clone();
					break;
				case SpecialMemberType.Setter:

					property.Setter = new Accessor();
					property.Setter.Body = (BlockStatement)methodDeclaration.Body.Clone();
					break;
			}

			methodDeclaration.Remove();
		}

		private enum SpecialMemberType
		{
			Unknown,
			Getter,
			Setter,
		}

		public void Run(AstNode rootNode, TransformContext context)
		{
			rootNode.AcceptVisitor(this);
		}
	}
}
