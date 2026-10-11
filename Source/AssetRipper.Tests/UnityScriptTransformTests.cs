using AssetRipper.Export.UnityProjects.Scripts;
using AssetRipper.Tests.Fixtures;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

namespace AssetRipper.Tests;

public class UnityScriptTransformTests
{
	[Test]
	public void UxmlClassesRegenerateTheirSerializationCodeWithoutLosingUserMembers()
	{
		CSharpDecompiler decompiler = new(typeof(UxmlSourceFixture).Assembly.Location, new DecompilerSettings());
		decompiler.AstTransforms.Add(new UnityScriptTransform());
		SyntaxTree tree = decompiler.DecompileType(new FullTypeName(typeof(UxmlSourceFixture).FullName!));
		TypeDeclaration root = tree.Descendants.OfType<TypeDeclaration>().Single(type => type.Name == nameof(UxmlSourceFixture));

		Assert.That(root.Modifiers.HasFlag(Modifiers.Partial), Is.True);
		Assert.That(root.Members.OfType<PropertyDeclaration>().Any(property => property.Name == "Value"), Is.True);
		Assert.That(root.Members.OfType<TypeDeclaration>().Any(type => type.Name == "UnrelatedNestedType"), Is.True);
		Assert.That(tree.Descendants.OfType<TypeDeclaration>().Any(type => type.Name == "UxmlSerializedData"), Is.False);
		TypeDeclaration nested = root.Members.OfType<TypeDeclaration>().Single(type => type.Name == "NestedObject");
		Assert.That(nested.Modifiers.HasFlag(Modifiers.Partial), Is.True);
	}
}
