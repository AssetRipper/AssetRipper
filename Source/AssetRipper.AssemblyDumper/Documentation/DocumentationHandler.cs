using System.Text;
using System.Xml;

namespace AssetRipper.AssemblyDumper.Documentation;

internal static class DocumentationHandler
{
	private static readonly Dictionary<TypeDefinition, List<string>> typeDefinitionDocumentation = new();
	private static readonly Dictionary<MethodDefinition, List<string>> methodDefinitionDocumentation = new();
	private static readonly Dictionary<PropertyDefinition, List<string>> propertyDefinitionDocumentation = new();
	private static readonly Dictionary<FieldDefinition, List<string>> fieldDefinitionDocumentation = new();
	private static readonly Dictionary<EventDefinition, List<string>> eventDefinitionDocumentation = new();

	public static void MakeDocumentationFile()
	{
		XmlDocument document = XmlUtils.CreateNewDocument();
		XmlElement docElement = document.AddChildElement(document, "doc");
		docElement.AddAssemblyName(document, SharedState.AssemblyName);

		XmlElement membersElement = docElement.AddChildElement(document, "members");

		foreach ((TypeDefinition type, List<string> lines) in typeDefinitionDocumentation)
		{
			membersElement.AddTypeMember(document, type).AddSummary(document, GetSummary(lines));
		}
		foreach ((MethodDefinition type, List<string> lines) in methodDefinitionDocumentation)
		{
			membersElement.AddMethodMember(document, type).AddSummary(document, GetSummary(lines));
		}
		foreach ((PropertyDefinition type, List<string> lines) in propertyDefinitionDocumentation)
		{
			membersElement.AddPropertyMember(document, type).AddSummary(document, GetSummary(lines));
		}
		foreach ((FieldDefinition type, List<string> lines) in fieldDefinitionDocumentation)
		{
			membersElement.AddFieldMember(document, type).AddSummary(document, GetSummary(lines));
		}
		foreach ((EventDefinition type, List<string> lines) in eventDefinitionDocumentation)
		{
			membersElement.AddEventMember(document, type).AddSummary(document, GetSummary(lines));
		}

		document.SaveWithTabs($"{SharedState.AssemblyName}.xml");
	}

	private static void SaveWithTabs(this XmlDocument document, string path)
	{
		XmlWriterSettings settings = new XmlWriterSettings();
		settings.Indent = true;
		settings.IndentChars = "\t";
		using FileStream stream = File.Create(path);
		XmlWriter writer = XmlWriter.Create(stream, settings);
		document.Save(writer);
	}

	public static void AddTypeDefinitionLine(TypeDefinition type, string line)
	{
		List<string> lines = typeDefinitionDocumentation.GetOrAdd(type);
		lines.Add(line);
	}

	public static void AddMethodDefinitionLine(MethodDefinition method, string line)
	{
		List<string> lines = methodDefinitionDocumentation.GetOrAdd(method);
		lines.Add(line);
	}

	public static void AddPropertyDefinitionLine(PropertyDefinition property, string line)
	{
		List<string> lines = propertyDefinitionDocumentation.GetOrAdd(property);
		lines.Add(line);
	}

	public static void AddPropertyDefinitionLine(PropertyBase property, string line)
	{
		AddPropertyDefinitionLine(property.Definition, line);
		if (property.SpecialDefinition is not null)
		{
			AddPropertyDefinitionLine(property.SpecialDefinition, line);
		}
	}

	public static void AddPropertyDefinitionLine(ClassProperty property, string line)
	{
		AddPropertyDefinitionLine((PropertyBase)property, line);
		if (property.HasBackingFieldInDeclaringType)
		{
			AddFieldDefinitionLine(property.BackingField, line);
		}
	}

	public static void AddPropertyDefinitionLineNotSpecial(ClassProperty property, string line)
	{
		AddPropertyDefinitionLine(property.Definition, line);
		if (property.HasBackingFieldInDeclaringType)
		{
			AddFieldDefinitionLine(property.BackingField, line);
		}
	}

	public static void AddFieldDefinitionLine(FieldDefinition field, string line)
	{
		List<string> lines = fieldDefinitionDocumentation.GetOrAdd(field);
		lines.Add(line);
	}

	public static void AddEventDefinitionLine(EventDefinition @event, string line)
	{
		List<string> lines = eventDefinitionDocumentation.GetOrAdd(@event);
		lines.Add(line);
	}

	private static string GetSummary(List<string> lines)
	{
		if (lines.Count == 0)
		{
			return "";
		}

		StringBuilder sb = new();
		sb.AppendLineAndThreeTabs();
		sb.Append(lines[0]);
		for (int i = 1; i < lines.Count; i++)
		{
			sb.AppendBreakTag();
			sb.AppendLineAndThreeTabs();
			sb.Append(lines[i]);
		}
		sb.AppendLineAndThreeTabs();
		return sb.ToString();
	}
}
