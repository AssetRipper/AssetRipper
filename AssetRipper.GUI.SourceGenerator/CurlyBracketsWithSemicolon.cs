using System;
using System.CodeDom.Compiler;

namespace AssetRipper.GUI.SourceGenerator;

public readonly struct CurlyBracketsWithSemicolon : IDisposable
{
	private readonly IndentedTextWriter writer;

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
