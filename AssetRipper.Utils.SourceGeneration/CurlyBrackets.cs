namespace AssetRipper.Utils.SourceGeneration;

public readonly ref struct CurlyBrackets
{
	private readonly IndentedTextWriter writer;

	public CurlyBrackets() => throw new NotSupportedException();

	public CurlyBrackets(IndentedTextWriter writer)
	{
		this.writer = writer;
		writer.WriteLine("{");
		writer.Indent++;
	}

	public void Dispose()
	{
		writer.Indent--;
		writer.WriteLine("}");
	}
}
