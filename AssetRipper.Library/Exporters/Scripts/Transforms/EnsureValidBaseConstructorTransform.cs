using AssetRipper.Core.Logging;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using ICSharpCode.Decompiler.TypeSystem;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AssetRipper.Library.Exporters.Scripts.Transforms
{
	/// <summary>
	/// A transform that ensures each constructor that doesn't define an initializer
	/// calls a valid base constructor
	/// </summary>
	internal class EnsureValidBaseConstructorTransform : DepthFirstAstVisitor, IAstTransform
	{
		private TransformContext context;

		public EnsureValidBaseConstructorTransform()
		{
			this.context = null!;
		}

		private static int GetConstructorCost(IMethod constructor, ConstructorDeclaration? currentConstructor, bool onlyAccessible = false)
		{
			// return max value cost for a non-accessible constructor (if toggled)
			if (onlyAccessible && !(constructor.Accessibility == Accessibility.Public || constructor.Accessibility == Accessibility.Protected))
			{
				return int.MaxValue;
			}
			int cost = 0;
			int parameterPosition = 0;
			foreach (IParameter parameter in constructor.Parameters)
			{
				// 3 cost per parameter
				cost += 3;
				int paramPosition = 0;
				if (currentConstructor != null)
				{
					foreach (ParameterDeclaration param in currentConstructor.Parameters)
					{
						if (param.Name == parameter.Name)
						{
							cost--;
							// todo: remove cost for parameter with same type
							break;
						}

						paramPosition++;
					}
				}

				parameterPosition++;
			}
			return cost;
		}

		private static bool TryGetBestConstructor(ITypeDefinition type, ConstructorDeclaration currentConstructor, [NotNullWhen(true)] out IMethod? bestConstructor, out bool isBaseConstructor)
		{
			bestConstructor = null;
			isBaseConstructor = default;

			if (type.IsStatic)
			{
				return false;
			}

			IMethod ctorMethod = (IMethod)currentConstructor.GetSymbol();
			IType? baseType = type.DirectBaseTypes?.Where((t) => t.Kind != TypeKind.Interface).FirstOrDefault();
			if (baseType == null)
			{
				return false;
			}

			if (currentConstructor.Initializer?.GetSymbol() is not IMethod)
			{
				// higher score is worse
				int bestCost = int.MaxValue;
				bestConstructor = null;
				isBaseConstructor = false;
				foreach (IMethod constructor in type.GetConstructors())
				{
					// exclude own constructor
					if (constructor.MetadataToken == ctorMethod.MetadataToken)
					{
						continue;
					}

					int cost = GetConstructorCost(constructor, currentConstructor);
					// subtract 1 cost to prefer defined constructors over base constructors
					cost--;

					if (cost < bestCost)
					{
						bestCost = cost;
						bestConstructor = constructor;
						isBaseConstructor = false;
					}
				}

				foreach (IMethod constructor in baseType.GetConstructors())
				{
					int cost = GetConstructorCost(constructor, currentConstructor, true);

					if (cost < bestCost)
					{
						bestCost = cost;
						bestConstructor = constructor;
						isBaseConstructor = true;
					}
				}

				return bestConstructor != null;
			}

			return false;
		}

		public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
		{
			base.VisitTypeDeclaration(typeDeclaration);

			if (typeDeclaration.ClassType != ClassType.Class)
			{
				return;
			}

			bool hasBaseConstructor = false;
			IMethod? bestConstructor = null;

			if (typeDeclaration.GetSymbol() is not ITypeDefinition type)
			{
				Logger.Warning($"Skip ensuring valid base constructor for type declaration {typeDeclaration.Name}, as failed to get type definition");
				return;
			}

			IEnumerable<ConstructorDeclaration> nonStaticConstructors = typeDeclaration.Members
				.Select((member) => member as ConstructorDeclaration)
				.Where((constructor) => constructor is not null && (constructor.Modifiers & Modifiers.Static) != Modifiers.Static)!;

			foreach (ConstructorDeclaration constructorDeclaration in nonStaticConstructors)
			{
				hasBaseConstructor = true;

				if (constructorDeclaration.Initializer != null && !constructorDeclaration.Initializer.IsNull)
				{
					continue;
				}

				if (TryGetBestConstructor(type, constructorDeclaration, out bestConstructor, out bool isBaseConstructor))
				{
					ConstructorInitializer initializer = new()
					{
						ConstructorInitializerType = isBaseConstructor ? ConstructorInitializerType.Base : ConstructorInitializerType.This
					};

					foreach (IParameter parameter in bestConstructor.Parameters)
					{
						bool hasParameterMatch = false;
						foreach (ParameterDeclaration param in constructorDeclaration.Parameters)
						{
							if (param.Name == parameter.Name)
							{
								hasParameterMatch = true;
								break;
							}
						}

						if (hasParameterMatch)
						{
							initializer.Arguments.Add(new IdentifierExpression(parameter.Name));
						}
						else
						{
							initializer.Arguments.Add(new DefaultValueExpression(ScriptUtilities.ConvertType(context.TypeSystemAstBuilder, parameter.Type)));
						}
					}

					constructorDeclaration.Initializer = initializer;
					if (constructorDeclaration.Body.IsNull)
					{
						constructorDeclaration.Body = new BlockStatement();
						constructorDeclaration.Modifiers &= ~Modifiers.Extern;
					}
				}
			}

			if (hasBaseConstructor)
			{
				return;
			}

			IType? baseType = type.DirectBaseTypes?.Where((t) => t.Kind != TypeKind.Interface)
				.FirstOrDefault();
			if (baseType == null || type.IsStatic)
			{
				return;
			}

			bestConstructor = baseType.GetConstructors()
				.OrderBy((ctor) => GetConstructorCost(ctor, null, true)).FirstOrDefault();
			if (bestConstructor != null)
			{
				ConstructorInitializer initializer = new()
				{
					ConstructorInitializerType = ConstructorInitializerType.Base
				};

				foreach (IParameter parameter in bestConstructor.Parameters)
				{
					initializer.Arguments.Add(new DefaultValueExpression(ScriptUtilities.ConvertType(context.TypeSystemAstBuilder, parameter.Type)));
				}


				ConstructorDeclaration constructorDeclaration = new()
				{
					Initializer = initializer,
					Body = new BlockStatement(),
					Modifiers = Modifiers.Public
				};

				typeDeclaration.Members.Add(constructorDeclaration);
			}

		}

		public void Run(AstNode rootNode, TransformContext context)
		{
			this.context = context;
			rootNode.AcceptVisitor(this);
		}
	}
}
