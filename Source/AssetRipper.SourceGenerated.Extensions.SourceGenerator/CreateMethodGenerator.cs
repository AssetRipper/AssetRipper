using AssetRipper.Text.SourceGeneration;
using Microsoft.CodeAnalysis;
using SGF;
using System.CodeDom.Compiler;
using System.Diagnostics;

namespace AssetRipper.SourceGenerated.Extensions.SourceGenerator;

[IncrementalGenerator]
public class CreateMethodGenerator() : IncrementalGenerator(nameof(CreateMethodGenerator))
{
	public override void OnInitialize(SgfInitializationContext context)
	{
		IncrementalValuesProvider<List<(int ClassId, string EnumValueName, string Name)>> values = context.SyntaxProvider.ForAttributeWithMetadataName("AssetRipper.SourceGenerated.Extensions.Attributes.AttributeCreatorMarkerAttribute", static (node, _) => true, static (context, _) =>
		{
			if (context.Attributes[0].ConstructorArguments[0].Value is not INamedTypeSymbol { TypeKind: TypeKind.Enum } classIdType)
			{
				return [];
			}

			// Get the enum values from the classIdType

			Dictionary<int, string> enumValues = classIdType.GetMembers().OfType<IFieldSymbol>()
				.Where(f => f.HasConstantValue)
				.Select(f => (f.Name, f.ConstantValue as int? ?? -1))
				.Where(f => f.Item2 >= 0)
				.ToDictionary(f => f.Item2, f => f.Item1);

			INamespaceSymbol? classesNamespace = classIdType.ContainingModule?.GlobalNamespace
				.GetNamespaceMembers().FirstOrDefault(n => n.Name == "AssetRipper")
				?.GetNamespaceMembers().FirstOrDefault(n => n.Name == "SourceGenerated")
				?.GetNamespaceMembers().FirstOrDefault(n => n.Name == "Classes");
			if (classesNamespace is null)
			{
				return [];
			}

			List<(int ClassId, string EnumValueName, string Name)> result = new();
			foreach (INamespaceSymbol classNamespace in classesNamespace.GetNamespaceMembers())
			{
				if (!classNamespace.Name.StartsWith("ClassID_", StringComparison.Ordinal))
				{
					continue;
				}
				if (!int.TryParse(classNamespace.Name["ClassID_".Length..], out int classId))
				{
					continue;
				}
				if (!enumValues.TryGetValue(classId, out string enumValueName))
				{
					continue;
				}

				bool anyClassNotAbstract = classNamespace.GetTypeMembers().Any(t => !t.IsStatic && !t.IsAbstract && t.TypeKind == TypeKind.Class);

				if (!anyClassNotAbstract)
				{
					continue;
				}

				string? interfaceName = classNamespace.GetTypeMembers().FirstOrDefault(t => t.TypeKind == TypeKind.Interface)?.Name;
				if (interfaceName is null || interfaceName.Length < 2 || interfaceName[0] != 'I')
				{
					continue;
				}

				string name = interfaceName[1..];

				result.Add((classId, enumValueName, name));
			}

			return result;
		}).WithComparer(Comparer.Instance);

		context.RegisterSourceOutput(values, static (context, values) =>
		{
			context.AddSource("AssetCreator.g.cs", GenerateCode(values));
		});
	}

	private static string GenerateCode(List<(int ClassId, string EnumValueName, string Name)> values)
	{
		StringWriter stringWriter = new() { NewLine = "\n" };
		using IndentedTextWriter writer = IndentedTextWriterFactory.Create(stringWriter);

		writer.WriteGeneratedCodeWarning();
		writer.WriteLineNoTabs();
		writer.WriteFileScopedNamespace("AssetRipper.SourceGenerated.Extensions");
		writer.WriteLineNoTabs();
		writer.WriteLine("public static partial class AssetCreator");
		using (new CurlyBrackets(writer))
		{
			writer.WriteLine("extension(global::AssetRipper.Assets.Collections.ProcessedAssetCollection collection)");
			using (new CurlyBrackets(writer))
			{
				foreach ((int ClassId, string EnumValueName, string Name) in values)
				{
					// Special case for the following classes
					// NavMeshData (194 or 238)
					// LightProbes (197 or 258)
					// VideoClip (327 or 329)
					// AvatarMask (1011 or 319)
					string classIdString = ClassId switch
					{
						194 or 238 => "collection.Version.LessThan(5) ? 194 : 238",
						197 or 258 => "collection.Version.LessThan(5) ? 197 : 258",
						327 or 329 => "collection.Version.GreaterThanOrEquals(5, 6, 0, global::AssetRipper.Primitives.UnityVersionType.Beta, 5) && collection.Version.LessThan(5, 6, 0, global::AssetRipper.Primitives.UnityVersionType.Beta, 10) ? 327 : 329",
						1011 or 319 => "collection.Version.LessThan(5, 6, 0, global::AssetRipper.Primitives.UnityVersionType.Beta, 3) ? 1011 : 319",
						_ => $"(int)global::AssetRipper.SourceGenerated.ClassIDType.{EnumValueName}"
					};
					writer.WriteLine($"public global::AssetRipper.SourceGenerated.Classes.ClassID_{ClassId}.I{Name} Create{Name}() => collection.CreateAsset({classIdString}, global::AssetRipper.SourceGenerated.Classes.ClassID_{ClassId}.{Name}.Create);");
				}
			}
			foreach ((int ClassId, _, string Name) in values)
			{
				writer.WriteLine($"public static global::AssetRipper.SourceGenerated.Classes.ClassID_{ClassId}.I{Name} Create{Name}(global::AssetRipper.Primitives.UnityVersion version) => CreateCollection(version).Create{Name}();");
			}
		}

		return stringWriter.ToString();
	}

	private sealed class Comparer : IEqualityComparer<List<(int ClassId, string EnumValueName, string Name)>>
	{
		public static readonly Comparer Instance = new();
		public bool Equals(List<(int ClassId, string EnumValueName, string Name)>? x, List<(int ClassId, string EnumValueName, string Name)>? y)
		{
			if (x is null && y is null)
			{
				return true;
			}
			if (x is null || y is null || x.Count != y.Count)
			{
				return false;
			}
			return x.SequenceEqual(y);
		}
		public int GetHashCode(List<(int ClassId, string EnumValueName, string Name)> obj)
		{
			unchecked
			{
				int hash = 17;
				foreach ((int ClassId, string EnumValueName, string Name) item in obj)
				{
					hash = hash * 31 + item.ClassId.GetHashCode();
					hash = hash * 31 + item.EnumValueName.GetHashCode();
					hash = hash * 31 + item.Name.GetHashCode();
				}
				return hash;
			}
		}
	}
}
