using AssetRipper.Core.Logging;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Resolver;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using ICSharpCode.Decompiler.CSharp.TypeSystem;
using ICSharpCode.Decompiler.TypeSystem;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AssetRipper.Library.Exporters.Scripts
{
	/// <summary>
	/// Remove all members from type except for fields and nested types
	/// </summary>
	internal class MethodStripper : DepthFirstAstVisitor, IAstTransform
	{
		/// <summary>
		/// Current transform context. Is used for accessing the current module for the resolver.
		/// </summary>
		private TransformContext _context;

		/// <summary>
		/// Strips all attributes from the attribute list provided, removing the specified node
		/// if (for example) a CompilerGenerated attribute is persent.
		/// </summary>
		/// <param name="node">The node</param>
		/// <param name="attributes">The attributes on the node</param>
		/// <returns>Whether or not <paramref name="node"/> was removed.</returns>
		private bool StripInvalidAttributesFromNode(AstNode node, AstNodeCollection<AttributeSection> attributes)
		{
			// we may want to add the type reference as well
			ITypeResolveContext resolver = new CSharpTypeResolveContext(this._context.CurrentModule);
			foreach (var attributeSection in attributes)
			{
				foreach (var attribute in attributeSection.Attributes)
				{
					var attributeTypeRef = attribute.Type.ToTypeReference();
					if (attributeTypeRef is SpecialType)
					{
						// special types in general shouldn't exist as attribute types
						// but this can be changed layer
						attribute.Remove();
						continue;
					}

					Logger.Info("Attribute reference type: " + attributeTypeRef.GetType().FullName);

					var attributeType = attributeTypeRef.Resolve(resolver);
					string fullName = attributeType.FullName;

					Logger.Warning("Resolved reference type: " + fullName);
					if (fullName.Contains("CompilerGenerated"))
					{
						// may want to change how we get the name of the node
						// so it's more user-friendly
						// also should the log level be changed?
						Logger.Warning($"Removing Compiler Generated {node.GetType().Name}.");
						node.Remove();
						return true;
					}
					// IteractorStateMachine generally is used IEnumerator methods.
					// The methods themselves are fine, but this attribute references the
					// compiler-generated type, which would get removed.
					else if (fullName.Contains("IteratorStateMachine"))
					{
						attribute.Remove();
					}
				}

				// remove attribute section if it's empty
				if (attributeSection.Attributes.Count == 0)
					attributeSection.Remove();
			}

			return false;
		}

        public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
		{
			this.DoVisitFieldDeclaration(fieldDeclaration);
		}

		private bool DoVisitFieldDeclaration(FieldDeclaration fieldDeclaration)
		{
			// strip all invalid attributes from field
			if (this.StripInvalidAttributesFromNode(fieldDeclaration, fieldDeclaration.Attributes))
				return false;

			foreach (var initializer in fieldDeclaration.Variables)
			{
				if (initializer.Name.Contains("<"))
				{
					Logger.Info("Fuck off random ass field: " + initializer.Name);
					initializer.Remove();
				}
			}
			if (fieldDeclaration.Variables.Count == 0)
			{
				fieldDeclaration.Remove();
				return false;
			}
			return true;
		}

		public override void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
		{
			this.DoVisitConstructorDeclaration(constructorDeclaration);
		}

		private bool DoVisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
		{
			if (this.StripInvalidAttributesFromNode(constructorDeclaration, constructorDeclaration.Attributes))
				return false;

			foreach (var statement in constructorDeclaration.Body.Statements)
			{
				statement.Remove();
			}
			return true;
		}

		private bool VisitStructConstructorDeclaration(TypeDeclaration declaration, List<FieldDeclaration> fields, ConstructorDeclaration constructorDeclaration)
		{
			if (!this.DoVisitConstructorDeclaration(constructorDeclaration))
				return false;

			return true;
		}

		private bool VisitClassConstructorDeclaration(TypeDeclaration declaration, ConstructorDeclaration constructorDeclaration)
		{
			if (!this.DoVisitConstructorDeclaration(constructorDeclaration))
				return false;

			return true;
		}

		public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
		{
			this.DoVisitTypeDeclaration(typeDeclaration);
		}

		private bool DoVisitTypeDeclaration(TypeDeclaration typeDeclaration)
		{
			// strip all invalid attributes from type, but break from method if the type was removed.
			if (this.StripInvalidAttributesFromNode(typeDeclaration, typeDeclaration.Attributes))
				return false;

			List<TypeDeclaration> nestedTypes = new();
			List<FieldDeclaration> fields = new();
			List<ConstructorDeclaration> constructors = new();

			// first pass
			foreach (var member in typeDeclaration.Members)
			{
				if (member is TypeDeclaration nestedType)
				{
					nestedTypes.Add(nestedType);
				}
				else if (member is FieldDeclaration field)
				{
					fields.Add(field);
				}
				else if (member is ConstructorDeclaration constructor)
				{
					constructors.Add(constructor);
				}
				else if (member is EnumMemberDeclaration)
				{
					continue;
				}
				else
				{
					member.Remove();
				}
			}

			// second pass
			int index = 0;
			while (index < fields.Count)
			{
				if (!this.DoVisitFieldDeclaration(fields[index]))
				{
					fields.RemoveAt(index);
				}
				else
					index++;
			}

			index = 0;
			while (index < constructors.Count)
			{
				var constructor = constructors[index];
				if (!this.DoVisitConstructorDeclaration(constructor))
				{
					constructors.RemoveAt(index);
				}
				else
					index++;
			}

			index = 0;
			while (index < nestedTypes.Count)
			{
				if (!this.DoVisitTypeDeclaration(nestedTypes[index]))
				{
					nestedTypes.RemoveAt(index);
				}
				else
					index++;
			}
			return true;
		}

		public void Run(AstNode rootNode, TransformContext context)
		{
			_context = context;
			rootNode.AcceptVisitor(this);
		}
	}
}
