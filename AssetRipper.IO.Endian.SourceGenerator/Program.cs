using AssetRipper.Utils.SourceGeneration;
using System.Buffers.Binary;
using System.CodeDom.Compiler;

namespace AssetRipper.IO.Endian.SourceGenerator;

internal class Program
{
	private const string PathToTargetDirectory = "../../../../AssetRipper.IO.Endian/";
	private const string ReaderStructName = "EndianSpanReader";
	private const string WriterStructName = "EndianSpanWriter";
	private const string BinaryPrimitivesNamespace = "System.Buffers.Binary";
	private const string TargetNamespace = "AssetRipper.IO.Endian";
	private const string BigEndianField = "bigEndian";
	private const string OffsetField = "offset";
	private const string DataField = "data";
	private const string SliceMethod = nameof(ReadOnlySpan<byte>.Slice);

	private static readonly List<(string, string)> list = new()
	{
		(nameof(Int16), "short"),
		(nameof(UInt16), "ushort"),
		(nameof(Int32), "int"),
		(nameof(UInt32), "uint"),
		(nameof(Int64), "long"),
		(nameof(UInt64), "ulong"),
		(nameof(Half), nameof(Half)),
		(nameof(Single), "float"),
		(nameof(Double), "double"),
	};

	private static void Main()
	{
		DoReaderStruct();
		DoWriterStruct();
		Console.WriteLine("Done!");
	}

	private static void DoReaderStruct()
	{
		using FileStream stream = File.Create($"{PathToTargetDirectory}{ReaderStructName}.g.cs");
		using StreamWriter streamWriter = new StreamWriter(stream);
		using IndentedTextWriter indentedWriter = new IndentedTextWriter(streamWriter, "\t");
		DoReaderStruct(indentedWriter);
	}

	private static void DoReaderStruct(IndentedTextWriter writer)
	{
		AddHeaderLines(writer);
		writer.WriteLine($"ref partial struct {ReaderStructName}");
		using (new CurlyBrackets(writer))
		{
			writer.WriteLine($"private readonly ReadOnlySpan<byte> {DataField};");
			writer.WriteLine($"private int {OffsetField};");
			writer.WriteLine($"private bool {BigEndianField};");
			AddLengthProperty(writer);
			AddPositionProperty(writer);
			foreach ((string typeName, string keyword) in list)
			{
				writer.WriteLineNoTabs();
				AddReadMethod(writer, typeName, keyword);
			}
		}
	}

	private static void DoWriterStruct()
	{
		using FileStream stream = File.Create($"{PathToTargetDirectory}{WriterStructName}.g.cs");
		using StreamWriter streamWriter = new StreamWriter(stream);
		using IndentedTextWriter indentedWriter = new IndentedTextWriter(streamWriter, "\t");
		DoWriterStruct(indentedWriter);
	}

	private static void DoWriterStruct(IndentedTextWriter writer)
	{
		AddHeaderLines(writer);
		writer.WriteLine($"ref partial struct {WriterStructName}");
		using (new CurlyBrackets(writer))
		{
			writer.WriteLine($"private readonly Span<byte> {DataField};");
			writer.WriteLine($"private int {OffsetField};");
			writer.WriteLine($"private bool {BigEndianField};");
			AddLengthProperty(writer);
			AddPositionProperty(writer);
			foreach ((string typeName, string keyword) in list)
			{
				writer.WriteLineNoTabs();
				AddWriteMethod(writer, typeName, keyword);
			}
		}
	}

	private static void AddHeaderLines(IndentedTextWriter writer)
	{
		writer.WriteGeneratedCodeWarning();
		writer.WriteUsing(BinaryPrimitivesNamespace);
		writer.WriteLine();
		writer.WriteFileScopedNamespace(TargetNamespace);
		writer.WriteLine();
	}

	private static void AddLengthProperty(IndentedTextWriter writer)
	{
		writer.WriteLine($"public int Length => {DataField}.{nameof(ReadOnlySpan<byte>.Length)};");
	}

	private static void AddPositionProperty(IndentedTextWriter writer)
	{
		writer.WriteLine("public int Position");
		using (new CurlyBrackets(writer))
		{
			writer.WriteLine($"get => {OffsetField};");
			writer.WriteLine($"set => {OffsetField} = value;");
		}
	}

	/// <summary>
	/// <code>
	/// public int ReadInt32()
	/// {
	///     int result = bigEndian
	///         ? BinaryPrimitives.ReadInt32BigEndian(sourceData.Slice(offset))
	///         : BinaryPrimitives.ReadInt32LittleEndian(sourceData.Slice(offset));
	///     offset += sizeof(int);
	///     return result;
	/// }
	/// </code>
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="typeName"></param>
	/// <param name="returnType"></param>
	private static void AddReadMethod(IndentedTextWriter writer, string typeName, string returnType)
	{
		const string ResultVariable = "result";
		writer.WriteLine($"public {returnType} Read{typeName}()");
		using (new CurlyBrackets(writer))
		{
			writer.WriteLine($"{returnType} {ResultVariable} = {BigEndianField}");
			using (new IndentedBlock(writer))
			{
				writer.WriteLine($"? {GetBinaryPrimitivesMethodName(typeName, true, true)}({DataField}.{SliceMethod}({OffsetField}))");
				writer.WriteLine($": {GetBinaryPrimitivesMethodName(typeName, false, true)}({DataField}.{SliceMethod}({OffsetField}));");
			}
			string sizeOfParameter = returnType is nameof(Half) ? "ushort" : returnType;
			writer.WriteLine($"{OffsetField} += sizeof({sizeOfParameter});");
			writer.WriteLine($"return {ResultVariable};");
		}
	}

	/// <summary>
	/// <code>
	/// public void Write(int value)
	/// {
	///     if (bigEndian)
	///     {
	///         BinaryPrimitives.WriteInt32BigEndian(sourceData.Slice(offset), value);
	///     }
	///     else
	///     {
	///         BinaryPrimitives.WriteInt32LittleEndian(sourceData.Slice(offset), value);
	///     }
	///     offset += sizeof(int);
	/// }
	/// </code>
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="typeName"></param>
	/// <param name="parameterType"></param>
	private static void AddWriteMethod(IndentedTextWriter writer, string typeName, string parameterType)
	{
		const string ValueParameter = "value";
		writer.WriteLine($"public void Write({parameterType} {ValueParameter})");
		using (new CurlyBrackets(writer))
		{
			writer.WriteLine($"if ({BigEndianField})");
			using (new CurlyBrackets(writer))
			{
				string methodName = GetBinaryPrimitivesMethodName(typeName, true, false);
				writer.WriteLine($"{methodName}({DataField}.{SliceMethod}({OffsetField}), {ValueParameter});");
			}
			writer.WriteLine("else");
			using (new CurlyBrackets(writer))
			{
				string methodName = GetBinaryPrimitivesMethodName(typeName, false, false);
				writer.WriteLine($"{methodName}({DataField}.{SliceMethod}({OffsetField}), {ValueParameter});");
			}
			string sizeOfParameter = parameterType is nameof(Half) ? "ushort" : parameterType;
			writer.WriteLine($"{OffsetField} += sizeof({sizeOfParameter});");
		}
	}

	private static string GetBinaryPrimitivesMethodName(string typeName, bool bigEndian, bool read)
	{
		return (bigEndian, read) switch
		{
			(true, true) => $"{nameof(BinaryPrimitives)}.Read{typeName}BigEndian",
			(false, true) => $"{nameof(BinaryPrimitives)}.Read{typeName}LittleEndian",
			(true, false) => $"{nameof(BinaryPrimitives)}.Write{typeName}BigEndian",
			(false, false) => $"{nameof(BinaryPrimitives)}.Write{typeName}LittleEndian",
		};
	}
}
