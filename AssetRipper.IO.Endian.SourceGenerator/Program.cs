using AssetRipper.Utils.SourceGeneration;
using System.Buffers.Binary;
using System.CodeDom.Compiler;

namespace AssetRipper.IO.Endian.SourceGenerator;

internal class Program
{
	private const string PathToTargetDirectory = "../../../../AssetRipper.IO.Endian/";
	private const string PathToTestsDirectory = "../../../../AssetRipper.IO.Endian.Tests/";
	private const string ReaderStructName = "EndianSpanReader";
	private const string WriterStructName = "EndianSpanWriter";
	private const string TestsClassName = "EndianSpanTests";
	private const string BinaryPrimitivesNamespace = "System.Buffers.Binary";
	private const string TargetNamespace = "AssetRipper.IO.Endian";
	private const string TestsNamespace = TargetNamespace + ".Tests";
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

	private static readonly List<(string, string)> otherList = new()
	{
		(nameof(Boolean), "bool"),
		(nameof(Byte), "byte"),
		(nameof(SByte), "sbyte"),
		(nameof(Char), "char"),
	};

	private static void Main()
	{
		DoReaderStruct();
		DoWriterStruct();
		DoTests();
		Console.WriteLine("Done!");
	}

	private static void DoReaderStruct()
	{
		using IndentedTextWriter writer = IndentedTextWriterFactory.Create(PathToTargetDirectory, ReaderStructName);
		DoReaderStruct(writer);
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
		using IndentedTextWriter writer = IndentedTextWriterFactory.Create(PathToTargetDirectory, WriterStructName);
		DoWriterStruct(writer);
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

	private static void DoTests()
	{
		using IndentedTextWriter writer = IndentedTextWriterFactory.Create(PathToTestsDirectory, TestsClassName);
		DoTests(writer);
	}

	private static void DoTests(IndentedTextWriter writer)
	{
		writer.WriteGeneratedCodeWarning();
		writer.WriteFileScopedNamespace(TestsNamespace);
		writer.WriteLine();
		writer.WriteLine($"public partial class {TestsClassName}");
		using (new CurlyBrackets(writer))
		{
			bool first = true;
			foreach ((string typeName, string keyWord) in list.Union(otherList))
			{
				if (first)
				{
					first = false;
				}
				else
				{
					writer.WriteLineNoTabs();
				}
				AddTestMethod(writer, typeName, keyWord, false);
				writer.WriteLineNoTabs();
				AddTestMethod(writer, typeName, keyWord, true);
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
		writer.WriteLine($"public readonly int Length => {DataField}.{nameof(ReadOnlySpan<byte>.Length)};");
	}

	private static void AddPositionProperty(IndentedTextWriter writer)
	{
		writer.WriteLine("public int Position");
		using (new CurlyBrackets(writer))
		{
			writer.WriteLine($"readonly get => {OffsetField};");
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
			writer.WriteLine($"{OffsetField} += {SizeOfExpression(returnType)};");
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
			writer.WriteLine($"{OffsetField} += {SizeOfExpression(parameterType)};");
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
	
	/// <summary>
	/// <code>
	/// [Test]
	/// public void BooleanBigEndian()
	/// {
	///     byte[] data = new byte[sizeof(bool)];
	///     bool value1 = RandomData.NextBoolean();
	/// 
	///     EndianSpanWriter writer = new EndianSpanWriter(data, EndianType.BigEndian);
	///     writer.Write(value1);
	///     Assert.That(writer.Position, Is.EqualTo(sizeof(bool)));
	/// 
	///     EndianSpanReader reader = new EndianSpanReader(data, EndianType.BigEndian);
	///     bool value2 = reader.ReadBoolean();
	///     Assert.That(reader.Position, Is.EqualTo(sizeof(bool)));
	///     Assert.That(value2, Is.EqualTo(value1));
	/// }
	/// </code>
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="typeName"></param>
	/// <param name="parameterType"></param>
	/// <param name="bigEndian"></param>
	private static void AddTestMethod(IndentedTextWriter writer, string typeName, string parameterType, bool bigEndian)
	{
		string endianName = bigEndian ? "BigEndian" : "LittleEndian";
		writer.WriteLine("[Test]");
		writer.WriteLine($"public void {typeName}{endianName}()");
		using (new CurlyBrackets(writer))
		{
			writer.WriteLine($"byte[] data = new byte[{SizeOfExpression(parameterType)}];");
			writer.WriteLine($"{parameterType} value1 = RandomData.Next{typeName}();");
			writer.WriteLineNoTabs();
			writer.WriteLine($"{WriterStructName} writer = new {WriterStructName}(data, EndianType.{endianName});");
			writer.WriteLine("writer.Write(value1);");
			writer.WriteLine($"Assert.That(writer.Position, Is.EqualTo({SizeOfExpression(parameterType)}));");
			writer.WriteLineNoTabs();
			writer.WriteLine($"{ReaderStructName} reader = new {ReaderStructName}(data, EndianType.{endianName});");
			writer.WriteLine($"{parameterType} value2 = reader.Read{typeName}();");
			writer.WriteLine($"Assert.That(reader.Position, Is.EqualTo({SizeOfExpression(parameterType)}));");
			writer.WriteLine("Assert.That(value2, Is.EqualTo(value1));");
		}
	}

	private static string SizeOfExpression(string type)
	{
		return type is nameof(Half) ? "sizeof(ushort)" : $"sizeof({type})";
	}
}
