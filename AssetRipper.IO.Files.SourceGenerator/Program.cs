using AssetRipper.IO.Files.SourceGenerator.Json;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AssetRipper.IO.Files.SourceGenerator
{
	internal class Program
	{
		const string GeneratorProjectDirectory = "../../../";
		const string RepositoryDirectory = GeneratorProjectDirectory + "../";
		const string OutputDirectory = RepositoryDirectory + "AssetRipper.IO.Files/";
		static void Main(string[] args)
		{
			GenerateForDirectory(GeneratorProjectDirectory + "Formats/SerializedFile/LocalSerializedObjectIdentifier/");
			GenerateForDirectory(GeneratorProjectDirectory + "Formats/SerializedFile/ObjectInfo/");
			GenerateForDirectory(GeneratorProjectDirectory + "Formats/SerializedFile/TypeTreeNode/");
			GenerateForDirectory(GeneratorProjectDirectory + "Formats/SerializedFile/SerializedFileHeader/");
			GenerateForDirectory(GeneratorProjectDirectory + "Formats/SerializedFile/FileIdentifier/");
		}

		private static void GenerateForDirectory(string directory)
		{
			ParseJsonFiles(directory, out TypeDeclaration declaration, out List<TypeDefinition> versions, out TypeDefinition? versionIndependent);

			Dictionary<string, string> propertyTypeDictionary = BuildPropertyTypeDictionary(versions);

			string destination = GetDestinationDirectory(declaration);

			{
				using IndentedTextWriter writer = OpenWrite($"{destination}I{declaration.Name}.g.cs");
				Generator.MakeInterface(writer, declaration, propertyTypeDictionary);
			}
			foreach (TypeDefinition version in versions)
			{
				using IndentedTextWriter writer = OpenWrite($"{destination}{declaration.Name}_{version.Version}.g.cs");
				Generator.MakeType(writer, declaration, version, propertyTypeDictionary);
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
				string text = System.IO.File.ReadAllText(fileInfo.FullName);
				if (fileInfo.Name == declarationFileName)
				{
					declaration = JsonSerializer.Deserialize<TypeDeclaration>(text);
				}
				else
				{
					TypeDefinition definition = JsonSerializer.Deserialize<TypeDefinition>(text) ?? throw new NullReferenceException();
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

		private static IndentedTextWriter OpenWrite(string path)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new NullReferenceException());
			return new IndentedTextWriter(new StreamWriter(path, false) { AutoFlush = true }, "\t");
		}

		private static string GetDestinationDirectory(TypeDeclaration declaration)
		{
			const string prefix = "AssetRipper.IO.Files.";
			if (declaration.Namespace.StartsWith(prefix, StringComparison.Ordinal))
			{
				string subDirectory = declaration.Namespace.Substring(prefix.Length).Replace('.', '/');
				return $"{OutputDirectory}{subDirectory}/";
			}
			else
			{
				throw new ArgumentException(null, nameof(declaration));
			}
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
	}
}
