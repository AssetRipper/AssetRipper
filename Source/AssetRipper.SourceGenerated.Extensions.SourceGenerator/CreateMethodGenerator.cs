using AssetRipper.Text.SourceGeneration;
using SGF;
using System.CodeDom.Compiler;

namespace AssetRipper.SourceGenerated.Extensions.SourceGenerator;

[IncrementalGenerator]
public class CreateMethodGenerator() : IncrementalGenerator(nameof(CreateMethodGenerator))
{
	public override void OnInitialize(SgfInitializationContext context)
	{
		context.RegisterPostInitializationOutput(static (context) =>
		{
			context.AddSource("AssetCreator.g.cs", GenerateCode());
		});
	}

	private static string GenerateCode()
	{
		StringWriter stringWriter = new() { NewLine = "\n" };
		using IndentedTextWriter writer = IndentedTextWriterFactory.Create(stringWriter);

		writer.WriteGeneratedCodeWarning();
		writer.WriteLineNoTabs();
		writer.WriteUsing("AssetRipper.Assets.Collections");
		writer.WriteUsing("AssetRipper.Primitives");
		foreach (ClassIDType classId in GetValues())
		{
			writer.WriteUsing($"AssetRipper.SourceGenerated.Classes.ClassID_{(int)classId}");
		}
		writer.WriteLineNoTabs();
		writer.WriteFileScopedNamespace("AssetRipper.SourceGenerated.Extensions");
		writer.WriteLineNoTabs();
		writer.WriteLine("public static partial class AssetCreator");
		using (new CurlyBrackets(writer))
		{
			writer.WriteLine("extension(ProcessedAssetCollection collection)");
			using (new CurlyBrackets(writer))
			{
				foreach (ClassIDType classId in GetValues())
				{
					string enumValueName = classId.ToString();
					string name = GetName(enumValueName);
					writer.WriteLine($"public I{name} Create{name}() => collection.CreateAsset((int)ClassIDType.{enumValueName}, {name}.Create);");
				}
			}
			foreach (ClassIDType classId in GetValues())
			{
				string name = GetName(classId.ToString());
				writer.WriteLine($"public static I{name} Create{name}(UnityVersion version) => CreateCollection(version).Create{name}();");
			}
		}

		return stringWriter.ToString();
	}

	private static string GetName(string enumValueName)
	{
		int index = enumValueName.IndexOf('_');
		if (index < 0)
		{
			return enumValueName;
		}
		else
		{
			return enumValueName.Substring(0, index);
		}
	}

	private static IEnumerable<ClassIDType> GetValues()
	{
		foreach (ClassIDType value in Enum.GetValues(typeof(ClassIDType)).Cast<ClassIDType>())
		{
			if (value is ClassIDType.NavMeshData_194 or ClassIDType.LightProbes_197 or ClassIDType.VideoClip_327 or ClassIDType.AvatarMask_1011)
			{
				continue;
			}

			yield return value;
		}
	}
}
