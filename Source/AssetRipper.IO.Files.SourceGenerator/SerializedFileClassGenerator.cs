using AssetRipper.IO.Files.SourceGenerator.Json;
using System.Text.Json;

namespace AssetRipper.IO.Files.SourceGenerator;

internal static class SerializedFileClassGenerator
{
	internal static void GenerateSerializedFileClasses()
	{
		GenerateForDirectory(Program.GeneratorProjectDirectory + "Formats/SerializedFile/LocalSerializedObjectIdentifier/");
		GenerateForDirectory(Program.GeneratorProjectDirectory + "Formats/SerializedFile/ObjectInfo/");
		GenerateForDirectory(Program.GeneratorProjectDirectory + "Formats/SerializedFile/TypeTreeNode/");
		GenerateForDirectory(Program.GeneratorProjectDirectory + "Formats/SerializedFile/SerializedFileHeader/");
		GenerateForDirectory(Program.GeneratorProjectDirectory + "Formats/SerializedFile/FileIdentifier/");
	}

	private static Dictionary<string, string> BuildPropertyTypeDictionary(List<TypeDefinition> versions)
	{
		if (versions.Count < 1)
		{
			throw new ArgumentException(null, nameof(versions));
		}

		Dictionary<string, string> result = new();
		foreach (TypeDefinition definition in versions)
		{
			foreach (FieldDefinition field in definition.AllFields)
			{
				if (!string.IsNullOrEmpty(field.FieldName))
				{
					string fieldType = field.TypeIsEnum(out string? enumName) ? enumName : field.TypeName;
					if (result.TryGetValue(field.FieldName, out string? type))
					{
						type = PrimitiveHandler.GetCommonType(type, fieldType);
					}
					else
					{
						type = fieldType;
					}
					result[field.FieldName] = type;
				}
			}
		}
		return result;
	}

	private static void GenerateForDirectory(string directory)
	{
		ParseJsonFiles(directory, out TypeDeclaration declaration, out List<TypeDefinition> versions, out TypeDefinition? versionIndependent);

		Dictionary<string, string> propertyTypeDictionary = BuildPropertyTypeDictionary(versions);

		string destination = GetDestinationDirectory(declaration);

		Directory.CreateDirectory(destination);
		{
			using IndentedTextWriter writer = IndentedTextWriterFactory.Create(destination, $"I{declaration.Name}");
			Generator.MakeInterface(writer, declaration, propertyTypeDictionary);
		}
		foreach (TypeDefinition version in versions)
		{
			using IndentedTextWriter writer = IndentedTextWriterFactory.Create(destination, $"{declaration.Name}_{version.Version}");
			Generator.MakeType(writer, declaration, version, propertyTypeDictionary);
		}
	}

	private static string GetDestinationDirectory(TypeDeclaration declaration)
	{
		const string prefix = "AssetRipper.IO.Files.";
		if (declaration.Namespace.StartsWith(prefix, StringComparison.Ordinal))
		{
			string subDirectory = declaration.Namespace.Substring(prefix.Length).Replace('.', '/');
			return $"{Program.OutputDirectory}{subDirectory}/";
		}
		else
		{
			throw new ArgumentException(null, nameof(declaration));
		}
	}

	private static void ParseJsonFiles(string directory, [NotNull] out TypeDeclaration? declaration, out List<TypeDefinition> versions, out TypeDefinition? versionIndependent)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(directory);
		if (!directoryInfo.Exists)
		{
			throw new DirectoryNotFoundException(directory);
		}

		string declarationFileName = $"{directoryInfo.Name}.json";

		versions = new();
		versionIndependent = null;
		declaration = null;
		foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles("*.json"))
		{
			string text = File.ReadAllText(fileInfo.FullName);
			if (fileInfo.Name == declarationFileName)
			{
				declaration = JsonSerializer.Deserialize(text, InternalSerializerContext.Default.TypeDeclaration);
			}
			else
			{
				TypeDefinition definition = JsonSerializer.Deserialize(text, InternalSerializerContext.Default.TypeDefinition) ?? throw new NullReferenceException();
				if (definition.Version == -1)
				{
					if (versionIndependent is null)
					{
						versionIndependent = definition;
					}
					else
					{
						throw new Exception();
					}
				}
				else
				{
					versions.Add(definition);
				}
			}
		}

		if (declaration is null)
		{
			throw new NullReferenceException();
		}
	}
}
