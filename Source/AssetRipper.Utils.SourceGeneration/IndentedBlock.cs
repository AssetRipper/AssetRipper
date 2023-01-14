namespace AssetRipper.Utils.SourceGeneration;

public readonly ref struct IndentedBlock
{
	private readonly IndentedTextWriter writer;

	public IndentedBlock() => throw new NotSupportedException();

	public IndentedBlock(IndentedTextWriter writer)
	{
		this.writer = writer;
		writer.Indent++;
	}

	public void Dispose()
	{
		writer.Indent--;
	}
}
