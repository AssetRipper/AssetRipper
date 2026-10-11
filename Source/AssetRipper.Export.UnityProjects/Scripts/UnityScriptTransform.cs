using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using ICSharpCode.Decompiler.TypeSystem;
using Attribute = ICSharpCode.Decompiler.CSharp.Syntax.Attribute;

namespace AssetRipper.Export.UnityProjects.Scripts;

/// <summary>
/// Adapts compiled Unity scripts for compilation in an editor project.
/// </summary>
public sealed class UnityScriptTransform : IAstTransform
{
	public void Run(AstNode rootNode, TransformContext context)
	{
		RestoreUxmlSourceGeneration(rootNode);
		RemoveCompilerOnlyAttributes(rootNode);
		FixExternalOverrides(rootNode);
		QualifyProjectTypeReferences(rootNode, context);
		new EscapeInvalidIdentifiers().Run(rootNode, context);
	}

	private static void RestoreUxmlSourceGeneration(AstNode rootNode)
	{
		foreach (TypeDeclaration type in rootNode.Descendants.OfType<TypeDeclaration>().ToArray())
		{
			if (!type.Attributes.SelectMany(section => section.Attributes).Any(attribute => GetAttributeName(attribute) is
				"UnityEngine.UIElements.UxmlElementAttribute" or "UnityEngine.UIElements.UxmlObjectAttribute"))
			{
				continue;
			}

			// Unity generates this nested type from the annotated fields and properties.
			// Keeping the compiled copy would duplicate the editor's generated definition.
			foreach (TypeDeclaration generatedType in type.Members.OfType<TypeDeclaration>()
				.Where(member => member.Name == "UxmlSerializedData").ToArray())
			{
				generatedType.Remove();
			}
			for (TypeDeclaration? declaration = type; declaration is not null; declaration = declaration.Parent as TypeDeclaration)
			{
				declaration.Modifiers |= Modifiers.Partial;
			}
		}
	}

	private static void RemoveCompilerOnlyAttributes(AstNode rootNode)
	{
		foreach (Attribute attribute in rootNode.Descendants.OfType<Attribute>().ToArray())
		{
			if (GetAttributeName(attribute) is "Unity.Jobs.DOTSCompilerGeneratedAttribute" or "Unity.Entities.MonoPInvokeCallbackAttribute")
			{
				AttributeSection? section = attribute.Parent as AttributeSection;
				attribute.Remove();
				if (section is not null && section.Attributes.Count == 0)
				{
					section.Remove();
				}
			}
		}
	}

	private static string GetAttributeName(Attribute attribute)
	{
		return attribute.GetSymbol() is IMethod constructor
			? constructor.DeclaringType.FullName
			: attribute.Type.GetResolveResult().Type.FullName;
	}

	private static void FixExternalOverrides(AstNode rootNode)
	{
		foreach (EntityDeclaration declaration in rootNode.Descendants.OfType<EntityDeclaration>())
		{
			if ((declaration.Modifiers & (Modifiers.Override | Modifiers.Protected | Modifiers.Internal)) !=
				(Modifiers.Override | Modifiers.Protected | Modifiers.Internal) || declaration.GetSymbol() is not IMember member)
			{
				continue;
			}
			if (member.DeclaringTypeDefinition is ITypeDefinition type && type.ParentModule is IModule module &&
				InheritanceHelper.GetBaseMembers(member, false).Any(baseMember =>
					baseMember.DeclaringTypeDefinition?.ParentModule is IModule baseModule && baseModule != module &&
					!baseModule.InternalsVisibleTo(module)))
			{
				declaration.Modifiers &= ~Modifiers.Internal;
			}
		}
	}

	private static void QualifyProjectTypeReferences(AstNode rootNode, TransformContext context)
	{
		TypeSystemAstBuilder builder = new() { AlwaysUseGlobal = true, AddResolveResultAnnotations = true };
		foreach (SimpleType reference in rootNode.Descendants.OfType<SimpleType>().ToArray())
		{
			IType type = reference.GetResolveResult().Type;
			if (type.GetDefinition()?.ParentModule == context.TypeSystem.MainModule && !string.IsNullOrEmpty(type.Namespace))
			{
				// Player assemblies can omit unused engine types. A name that appeared
				// unambiguous to the decompiler can conflict with the editor's full API.
				reference.ReplaceWith(builder.ConvertType(type));
			}
		}
	}
}
