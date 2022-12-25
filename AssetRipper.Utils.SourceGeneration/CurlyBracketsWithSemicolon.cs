namespace AssetRipper.Utils.SourceGeneration;

public readonly ref struct CurlyBracketsWithSemicolon
{
	private readonly IndentedTextWriter writer;

	public CurlyBracketsWithSemicolon() => throw new NotSupportedException();

	public CurlyBracketsWithSemicolon(IndentedTextWriter writer)
	{
		this.writer = writer;
		writer.WriteLine("{");
		writer.Indent++;
	}

	public void Dispose()
	{
		writer.Indent--;
		writer.WriteLine("};");
	}
}
