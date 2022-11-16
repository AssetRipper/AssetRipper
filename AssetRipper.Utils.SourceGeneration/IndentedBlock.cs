namespace AssetRipper.Utils.SourceGeneration;

public readonly struct IndentedBlock : IDisposable
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
