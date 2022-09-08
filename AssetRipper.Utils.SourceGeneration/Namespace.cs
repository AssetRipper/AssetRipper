using System;
using System.CodeDom.Compiler;

namespace AssetRipper.Utils.SourceGeneration;

public readonly struct Namespace : IDisposable
{
	private readonly CurlyBrackets curlyBrackets;

	public Namespace() => throw new NotSupportedException();

	public Namespace(IndentedTextWriter writer, string @namespace)
	{
		writer.WriteLine($"namespace {@namespace}");
		curlyBrackets = new CurlyBrackets(writer);
	}

	public void Dispose()
	{
		curlyBrackets.Dispose();
	}
}
