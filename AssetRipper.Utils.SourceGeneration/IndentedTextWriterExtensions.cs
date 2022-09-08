using System.CodeDom.Compiler;

namespace AssetRipper.Utils.SourceGeneration;

public static class IndentedTextWriterExtensions
{
	public static void WriteGeneratedCodeWarning(this IndentedTextWriter writer) => writer.WriteComment("Auto-generated code. Do not modify manually.");
	public static void WriteUsing(this IndentedTextWriter writer, string @namespace) => writer.WriteLine($"using {@namespace};");
	public static void WriteFileScopedNamespace(this IndentedTextWriter writer, string @namespace) => writer.WriteLine($"namespace {@namespace};");
	public static void WriteComment(this IndentedTextWriter writer, string comment) => writer.WriteLine($"// {comment}");
	public static void WriteSummaryDocumentation(this IndentedTextWriter writer, string summary) => writer.WriteXmlDocumentation("summary", summary);
	public static void WriteRemarksDocumentation(this IndentedTextWriter writer, string remarks) => writer.WriteXmlDocumentation("remarks", remarks);
	public static void WriteParameterDocumentation(this IndentedTextWriter writer, string parameterName, string parameterDescription)
	{
		writer.WriteLine($"/// <param name=\"{parameterName}\">{parameterDescription}</param>");
	}
	public static void WriteReturnsDocumentation(this IndentedTextWriter writer, string description)
	{
		writer.WriteLine($"/// <returns>{description}</returns>");
	}
	private static void WriteXmlDocumentation(this IndentedTextWriter writer, string tag, string content)
	{
		writer.WriteLine($"/// <{tag}>");
		writer.WriteLine($"/// {content}");
		writer.WriteLine($"/// </{tag}>");
	}
}
