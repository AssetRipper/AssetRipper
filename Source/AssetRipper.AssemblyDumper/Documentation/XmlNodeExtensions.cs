using System.Xml;

namespace AssetRipper.AssemblyDumper.Documentation;

internal static class XmlNodeExtensions
{
	internal static void AddAssemblyName(this XmlNode docElement, XmlDocument document, string assemblyName)
	{
		XmlElement assemblyElement = docElement.AddChildElement(document, "assembly");
		XmlElement nameElement = assemblyElement.AddChildElement(document, "name");
		nameElement.InnerXml = assemblyName;
	}

	internal static XmlElement AddInheritDoc(this XmlNode memberElement, XmlDocument document)
	{
		return memberElement.AddChildElement(document, "inheritdoc");
	}

	internal static XmlElement AddMember(this XmlNode membersElement, XmlDocument document, string memberName)
	{
		XmlElement memberElement = membersElement.AddChildElement(document, "member");
		XmlAttribute attribute = document.CreateAttribute("name");
		attribute.InnerXml = memberName;
		memberElement.Attributes.Append(attribute);
		return memberElement;
	}

	internal static XmlElement AddSummary(this XmlNode memberElement, XmlDocument document, string summaryContent)
	{
		XmlElement summaryElement = memberElement.AddChildElement(document, "summary");
		summaryElement.InnerXml = summaryContent;
		return summaryElement;
	}

	internal static XmlElement AddChildElement(this XmlNode parent, XmlDocument document, string name)
	{
		XmlElement element = document.CreateElement(name);
		parent.AppendChild(element);
		return element;
	}

	internal static XmlElement AddTypeMember(this XmlNode membersElement, XmlDocument document, TypeDefinition type)
	{
		return membersElement.AddMember(document, XmlUtils.GetStringReference(type));
	}

	internal static XmlElement AddMethodMember(this XmlNode membersElement, XmlDocument document, MethodDefinition method)
	{
		return membersElement.AddMember(document, XmlUtils.GetStringReference(method));
	}

	internal static XmlElement AddPropertyMember(this XmlNode membersElement, XmlDocument document, PropertyDefinition property)
	{
		return membersElement.AddMember(document, XmlUtils.GetStringReference(property));
	}

	internal static XmlElement AddFieldMember(this XmlNode membersElement, XmlDocument document, FieldDefinition field)
	{
		return membersElement.AddMember(document, XmlUtils.GetStringReference(field));
	}

	internal static XmlElement AddEventMember(this XmlNode membersElement, XmlDocument document, EventDefinition @event)
	{
		return membersElement.AddMember(document, XmlUtils.GetStringReference(@event));
	}
}
