using AssetRipper.Core.Logging;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using ICSharpCode.Decompiler.TypeSystem;
using System.Diagnostics;
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

		// TODO: Refactor this method so parts of it aren't used when no constructors exist at all.

		private bool TryGetBestConstructor(ITypeDefinition type, ConstructorDeclaration currentConstructor, [NotNullWhen(true)] out IMethod? bestConstructor, out bool isBaseConstructor)
		{
			if (type.IsStatic)
			{
				goto INVALID;
			}

			IMethod ctorMethod = (IMethod)currentConstructor.GetSymbol();
			IType? baseType = type.DirectBaseTypes?.Where((t) => t.Kind != TypeKind.Interface).FirstOrDefault();
			if (baseType == null)
			{
				goto INVALID;
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

					int cost = 0;
					int parameterPosition = 0;
					foreach (IParameter parameter in constructor.Parameters)
					{
						// 3 cost per parameter
						cost += 3;
						int paramPosition = 0;
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

						parameterPosition++;
					}
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
					// exclude hidden constructors
					if (constructor.Accessibility != Accessibility.Public &&
							constructor.Accessibility != Accessibility.Protected)
					{
						Logger.Debug(baseType.Name + " " + constructor.Accessibility);
						continue;
					}

					int cost = 0;
					int parameterPosition = 0;
					foreach (IParameter parameter in constructor.Parameters)
					{
						// 3 cost per parameter
						cost += 3;
						int paramPosition = 0;
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

						parameterPosition++;
					}

					if (cost < bestCost)
					{
						bestCost = cost;
						bestConstructor = constructor;
						isBaseConstructor = true;
					}
				}

				return bestConstructor != null;
			}

		INVALID:
			bestConstructor = null;
			isBaseConstructor = default;
			return false;
		}

		public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
		{
			base.VisitTypeDeclaration(typeDeclaration);
			if (typeDeclaration.ClassType == ClassType.Class)
			{
				bool hasBaseConstructor = false;

				ITypeDefinition? type = typeDeclaration.GetSymbol() as ITypeDefinition;
				if (type == null)
				{
					Logger.Warning($"Skip ensuring valid base constructor for type declaration {typeDeclaration.Name}, as failed to get type definition");
					return;
				}

				foreach (ConstructorDeclaration? constructorDeclaration in typeDeclaration.Members.Select((member) => member as ConstructorDeclaration).Where((constructor) => constructor is not null && (constructor.Modifiers & Modifiers.Static) != Modifiers.Static))
				{
					Debug.Assert(constructorDeclaration != null);
					hasBaseConstructor = true;

					if (constructorDeclaration.Initializer != null && !constructorDeclaration.Initializer.IsNull)
					{
						continue;
					}

					if (TryGetBestConstructor(type, constructorDeclaration, out IMethod? bestConstructor, out bool isBaseConstructor))
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
								initializer.Arguments.Add(new DefaultValueExpression(context.TypeSystemAstBuilder.ConvertType(parameter.Type)));
							}
						}
						constructorDeclaration.Initializer = initializer;
					}
				}

				if (!hasBaseConstructor)
				{
					IType? baseType = type.DirectBaseTypes?.Where((t) => t.Kind != TypeKind.Interface).FirstOrDefault();
					if (baseType == null || type.IsStatic)
					{
						return;
					}

					IMethod? bestConstructor = baseType.GetConstructors()
						.OrderBy((ctor) => ctor.Parameters.Count * 2 + ((ctor.Accessibility == Accessibility.Public || ctor.Accessibility == Accessibility.Protected) ? 0 : 1)).FirstOrDefault();
					if (bestConstructor != null)
					{
						ConstructorInitializer initializer = new()
						{
							ConstructorInitializerType = ConstructorInitializerType.Base
						};
						foreach (IParameter parameter in bestConstructor.Parameters)
						{
							initializer.Arguments.Add(new DefaultValueExpression(context.TypeSystemAstBuilder.ConvertType(parameter.Type)));
						}

						ConstructorDeclaration constructorDeclaration = new()
						{
							Initializer = initializer,
							Modifiers = Modifiers.Public,
							Body = new BlockStatement(),
						};

						// todo: fix constructor being externed even though it has a body and
						//       a null statement.
						constructorDeclaration.Body.Statements.Add(Statement.Null);
						typeDeclaration.Members.Add(constructorDeclaration);
					}
				}
			}
		}

		public void Run(AstNode rootNode, TransformContext context)
		{
			this.context = context;
			rootNode.AcceptVisitor(this);
		}
	}
}
