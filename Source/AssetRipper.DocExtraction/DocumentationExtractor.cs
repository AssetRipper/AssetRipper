using AssetRipper.DocExtraction.MetaData;

namespace AssetRipper.DocExtraction;

public static class DocumentationExtractor
{
	public static DocumentationFile ExtractDocumentation(string unityVersion, string engineXmlPath, string editorXmlPath, string engineDllPath, string editorDllPath)
	{
		Dictionary<string, string> typeSummaries = new();
		Dictionary<string, string> fieldSummaries = new();
		Dictionary<string, string> propertySummaries = new();
		XmlDocumentParser.ExtractDocumentationFromXml(engineXmlPath, typeSummaries, fieldSummaries, propertySummaries);
		XmlDocumentParser.ExtractDocumentationFromXml(editorXmlPath, typeSummaries, fieldSummaries, propertySummaries);

		Dictionary<string, ClassDocumentation> classDictionary = new();
		Dictionary<string, EnumDocumentation> enumDictionary = new();
		Dictionary<string, StructDocumentation> structDictionary = new();
		AssemblyParser.ExtractDocumentationFromAssembly(engineDllPath, typeSummaries, fieldSummaries, propertySummaries, classDictionary, enumDictionary, structDictionary);
		AssemblyParser.ExtractDocumentationFromAssembly(editorDllPath, typeSummaries, fieldSummaries, propertySummaries, classDictionary, enumDictionary, structDictionary);

		return new DocumentationFile()
		{
			UnityVersion = unityVersion,
			Classes = classDictionary.Values.ToList(),
			Enums = enumDictionary.Values.ToList(),
			Structs = structDictionary.Values.ToList(),
		};
	}
}