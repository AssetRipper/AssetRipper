using AssetRipper.Text.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using SGF;
using System.CodeDom.Compiler;
using System.Text;

namespace AssetRipper.Processing.SourceGenerator;

[IncrementalGenerator]
public class PolyfillGenerator() : IncrementalGenerator(nameof(PolyfillGenerator))
{
	public override void OnInitialize(SgfInitializationContext context)
	{
		context.RegisterPostInitializationOutput(static (context) =>
		{
			byte[] assemblyBytes = CompilePolyfills();
			string code = GenerateCode(assemblyBytes);
			context.AddSource("EmbeddedAssembly.cs", code);
		});
	}

	private static string GenerateCode(byte[] assemblyBytes)
	{
		StringWriter stringWriter = new() { NewLine = "\n" };
		using IndentedTextWriter writer = IndentedTextWriterFactory.Create(stringWriter);

		writer.WriteGeneratedCodeWarning();
		writer.WriteLineNoTabs();
		writer.WriteFileScopedNamespace("AssetRipper.Processing.Assemblies");
		writer.WriteLineNoTabs();
		writer.WriteLine("static partial class EmbeddedAssembly");
		using (new CurlyBrackets(writer))
		{
			writer.WriteLine("private static readonly byte[] Bytes = new byte[]");
			using (new CurlyBracketsWithSemicolon(writer))
			{
				foreach (byte b in assemblyBytes)
				{
					writer.WriteLine(b.ToString() + ",");
				}
			}
		}

		return stringWriter.ToString();
	}

	private static byte[] CompilePolyfills()
	{
		List<SyntaxTree> syntaxTrees = [];

		foreach (string code in Polyfills.Get())
		{
			AddCode(syntaxTrees, code);
		}

		using MemoryStream polyfillOutputStream = new();

		// Emit compiled assembly into MemoryStream
		CSharpCompilation compilation = CreateCompilation(syntaxTrees, []);
		EmitResult result = compilation.Emit(polyfillOutputStream);
		return result.Success ? polyfillOutputStream.ToArray() : [];
	}

	private static void AddCode(List<SyntaxTree> syntaxTrees, string code)
	{
		SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code, encoding: Encoding.UTF8);
		syntaxTrees.Add(syntaxTree);
	}

	private static CSharpCompilation CreateCompilation(IEnumerable<SyntaxTree> syntaxTrees, IEnumerable<MetadataReference> references)
	{
		// Define compilation options
		CSharpCompilationOptions compilationOptions = new(OutputKind.DynamicallyLinkedLibrary);

		// Create the compilation
		CSharpCompilation compilation = CSharpCompilation.Create(
			"System.Polyfill",
			syntaxTrees,
			references,
			compilationOptions
		);
		return compilation;
	}
}
