using System;
using System.CodeDom.Compiler;

namespace AssetRipper.Utils.SourceGeneration;

public readonly struct CurlyBrackets : IDisposable
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
