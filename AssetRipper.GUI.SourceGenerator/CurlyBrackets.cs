using System;
using System.CodeDom.Compiler;

namespace AssetRipper.GUI.SourceGenerator;

public readonly struct CurlyBrackets : IDisposable
{
	private readonly IndentedTextWriter writer;

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
