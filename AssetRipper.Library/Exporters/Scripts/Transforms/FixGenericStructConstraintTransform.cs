using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using System.Collections.Generic;

namespace AssetRipper.Library.Exporters.Scripts.Transforms
{
	/// <summary>
	/// Fixes the struct constraint when the generic parameter is used as a pointer.
	/// <para>
	/// To fix this in code, you replace <see langword="struct"/> with <see langword="unmanaged"/>
	/// </para>
	/// </summary>
	internal class FixGenericStructConstraintTransform : DepthFirstAstVisitor, IAstTransform
	{
		/// <summary>
		/// A stack containing all current generic type identifiers
		/// </summary>
		private readonly Stack<string> currentGenerics = new();
		/// <summary>
		/// A set containing all generic types from <see cref="currentGenerics"/> that have
		/// been found to be used as a pointer
		/// </summary>
		private readonly HashSet<string> genericPointers = new();

		private void InitTypeParameters(AstNodeCollection<TypeParameterDeclaration> typeParameters)
		{
			foreach (TypeParameterDeclaration typeParameter in typeParameters)
			{
				currentGenerics.Push(typeParameter.Name);
			}
		}

		private void ConvertConstraints(int typeParameterCount, AstNodeCollection<Constraint> constraints)
		{
			List<string> contraintsToConvert = new();
			for (int count = 0; count < typeParameterCount; count++)
			{
				string parameter = currentGenerics.Pop();
				if (genericPointers.Contains(parameter))
				{
					contraintsToConvert.Add(parameter);
					genericPointers.Remove(parameter);
				}
			}

			foreach (Constraint constraint in constraints)
			{
				if (!contraintsToConvert.Contains(constraint.TypeParameter.Identifier))
				{
					continue;
				}

				foreach (AstType type in constraint.BaseTypes)
				{
					if (type is PrimitiveType primitiveType && primitiveType.Keyword == "struct")
					{
						primitiveType.Keyword = "unmanaged";
					}
				}
			}
		}

		public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
		{
			InitTypeParameters(typeDeclaration.TypeParameters);

			base.VisitTypeDeclaration(typeDeclaration);

			ConvertConstraints(typeDeclaration.TypeParameters.Count, typeDeclaration.Constraints);
		}

		public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
		{
			InitTypeParameters(methodDeclaration.TypeParameters);

			base.VisitMethodDeclaration(methodDeclaration);

			ConvertConstraints(methodDeclaration.TypeParameters.Count, methodDeclaration.Constraints);
		}

		public override void VisitComposedType(ComposedType composedType)
		{
			base.VisitComposedType(composedType);
			if (composedType.PointerRank > 0 && composedType.BaseType is SimpleType type && currentGenerics.Contains(type.Identifier))
			{
				genericPointers.Add(type.Identifier);
			}
		}

		public void Run(AstNode rootNode, TransformContext context)
		{
			rootNode.AcceptVisitor(this);
		}
	}
}
