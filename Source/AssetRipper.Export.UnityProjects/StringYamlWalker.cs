using System.Globalization;

namespace AssetRipper.Export.UnityProjects;

public class StringYamlWalker : DefaultYamlWalker
{
	private readonly StringWriter stringWriter;
	private string CurrentText => stringWriter.ToString();

	public StringYamlWalker() : this(new(CultureInfo.InvariantCulture) { NewLine = "\n" })
	{
	}

	private StringYamlWalker(StringWriter stringWriter) : base(stringWriter)
	{
		this.stringWriter = stringWriter;
	}

	public void Reset()
	{
		Writer.Flush();
		stringWriter.GetStringBuilder().Clear();
		WriteHead();
	}

	public sealed override string ToString() => CurrentText;

	public string ToStringAndReset()
	{
		string result = ToString();
		Reset();
		return result;
	}
}
