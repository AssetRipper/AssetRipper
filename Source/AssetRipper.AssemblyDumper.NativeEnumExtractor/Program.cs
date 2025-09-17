using AsmResolver;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.Symbols.Pdb;
using AsmResolver.Symbols.Pdb.Leaves;
using AsmResolver.Symbols.Pdb.Msf;
using AsmResolver.Symbols.Pdb.Records;
using AssetRipper.DocExtraction.MetaData;

namespace AssetRipper.AssemblyDumper.NativeEnumExtractor;

internal class Program
{
	//private static IErrorListener ErrorListener { get; } = EmptyErrorListener.Instance;
	private static IErrorListener ErrorListener { get; } = new DiagnosticBag();
	//private static IErrorListener ErrorListener { get; } = ThrowErrorListener.Instance;

	static void Main(string[] args)
	{
		string path = args[0];
		string unityVersion = args[1];
		MsfFile file = MsfFile.FromFile(path);
		PdbImage image = PdbImage.FromFile(file, new PdbReaderParameters(ErrorListener));
		//WriteSymbolsToFile(image.Symbols, "symbols.txt");
		PrintUnknownTypes(image.Symbols);
		List<UserDefinedTypeSymbol> complexTypes = GetComplexTypes(image.Symbols);
		HashSet<EnumTypeRecord> enumRecords = GetEnumRecords(image.Symbols);
		List<ConstantSymbol> constants = GetNonEnumConstants(image.Symbols);
		DocumentationFile documentationFile = MakeDocumentationFile(enumRecords, unityVersion);
		documentationFile.SaveAsJson("native_enums.json");
		Console.WriteLine($"{image.Symbols.Count} symbols!");
	}

	private static DocumentationFile MakeDocumentationFile(IEnumerable<EnumTypeRecord> records, string unityVersion)
	{
		DocumentationFile file = new();
		file.UnityVersion = unityVersion;
		foreach (EnumTypeRecord record in records)
		{
			EnumDocumentation? documentation = ParseEnumRecord(record);
			if (documentation is not null)
			{
				file.Enums.Add(documentation);
			}
		}
		return file;
	}

	private static HashSet<EnumTypeRecord> GetEnumRecords(IList<ICodeViewSymbol> symbols)
	{
		HashSet<EnumTypeRecord> records = new();
		foreach (ICodeViewSymbol symbol in symbols)
		{
			if (symbol is ConstantSymbol constantSymbol && constantSymbol.Type is EnumTypeRecord enumRecord)
			{
				records.Add(enumRecord);
			}
		}
		return records;
	}

	private static List<ConstantSymbol> GetNonEnumConstants(IList<ICodeViewSymbol> symbols)
	{
		List<ConstantSymbol> records = [];
		foreach (ICodeViewSymbol symbol in symbols)
		{
			if (symbol is ConstantSymbol constantSymbol
				&& constantSymbol.Type is not null and not EnumTypeRecord
				&& IsValidName(constantSymbol.Name.ToString().Trim()))
			{
				records.Add(constantSymbol);
			}
		}
		return records;
	}

	private static List<UserDefinedTypeSymbol> GetComplexTypes(IList<ICodeViewSymbol> symbols)
	{
		List<UserDefinedTypeSymbol> records = [];
		foreach (ICodeViewSymbol symbol in symbols)
		{
			if (symbol is UserDefinedTypeSymbol constantSymbol
				&& constantSymbol.Type is ClassTypeRecord)
			{
				records.Add(constantSymbol);
			}
		}
		return records;
	}

	private static bool IsValidName(string? name)
	{
		return !string.IsNullOrEmpty(name) && !name.Contains('<') && !name.StartsWith("std::", StringComparison.Ordinal);
	}

	private static void PrintUnknownTypes(IList<ICodeViewSymbol> symbols)
	{
		foreach ((CodeViewSymbolType type, HashSet<int> sizes) in GetUnknownTypes(symbols))
		{
			Console.WriteLine($"Unknown Type: {type}");
			Console.WriteLine($"Sizes: {string.Join(", ", sizes)}");
			Console.WriteLine();
		}
	}

	private static Dictionary<CodeViewSymbolType, HashSet<int>> GetUnknownTypes(IList<ICodeViewSymbol> symbols)
	{
		Dictionary<CodeViewSymbolType, HashSet<int>> unknownTypes = new();
		foreach (ICodeViewSymbol symbol in symbols)
		{
			if (symbol is UnknownSymbol unknownSymbol)
			{
				if (!unknownTypes.TryGetValue(unknownSymbol.CodeViewSymbolType, out HashSet<int>? sizes))
				{
					sizes = [];
					unknownTypes.Add(unknownSymbol.CodeViewSymbolType, sizes);
				}
				sizes.Add(unknownSymbol.Data.Length);
			}
		}
		return unknownTypes;
	}

	private static void WriteSymbolsToFile(IList<ICodeViewSymbol> symbols, string path)
	{
		using FileStream stream = File.Create(path);
		using StreamWriter writer = new(stream);
		foreach (ICodeViewSymbol symbol in symbols)
		{
			switch (symbol)
			{
				case ConstantSymbol constantSymbol:
					writer.WriteLine($"Constant, Value: {constantSymbol.Value} Type: {constantSymbol.Type?.LeafKind} Name: {constantSymbol.Name}");
					break;
				case PublicSymbol publicSymbol:
					if (publicSymbol.IsCode)
					{
						Console.WriteLine($"Public Code: {publicSymbol.Name}");
					}
					writer.WriteLine($"Public, Function: {publicSymbol.IsFunction} Name: {publicSymbol.Name}");
					break;
				case UserDefinedTypeSymbol userDefinedTypeSymbol:
					writer.WriteLine($"User Defined Type, {userDefinedTypeSymbol.Type?.LeafKind} Name: {userDefinedTypeSymbol.Name}");
					break;
				case UnknownSymbol unknownSymbol:
					writer.WriteLine($"Unknown, Type: {unknownSymbol.CodeViewSymbolType} Size: {unknownSymbol.Data.Length}");
					break;
			}
		}
	}

	private static EnumDocumentation? ParseEnumRecord(EnumTypeRecord record)
	{
		if (record.Fields is null)
		{
			return null;
		}

		string cppName = record.Name;
		if (cppName.Contains('<') || cppName.Contains('>'))
		{
			return null;//No generics or anonymous enums
		}
		string[] nameSegments = cppName.Split("::", StringSplitOptions.RemoveEmptyEntries);

		EnumDocumentation documentation = new();
		documentation.Name = nameSegments[^1];
		documentation.FullName = string.Join('.', nameSegments);
		documentation.ElementType = ((SimpleTypeRecord)record.BaseType!).Kind switch
		{
			SimpleTypeKind.SignedCharacter => ElementType.I1,
			SimpleTypeKind.UnsignedCharacter => ElementType.U1,
			SimpleTypeKind.SByte => ElementType.I1,
			SimpleTypeKind.Byte => ElementType.U1,
			SimpleTypeKind.Int16Short or SimpleTypeKind.Int16 => ElementType.I2,
			SimpleTypeKind.UInt16Short or SimpleTypeKind.UInt16 => ElementType.U2,
			SimpleTypeKind.Int32Long or SimpleTypeKind.Int32 => ElementType.I4,
			SimpleTypeKind.UInt32Long or SimpleTypeKind.UInt32 => ElementType.U4,
			SimpleTypeKind.Int64Quad or SimpleTypeKind.Int64 => ElementType.I8,
			SimpleTypeKind.UInt64Quad or SimpleTypeKind.UInt64 => ElementType.U8,
			_ => throw new NotSupportedException(),
		};

		foreach (EnumerateField field in record.Fields!.Entries.Cast<EnumerateField>())
		{
			EnumMemberDocumentation enumMember = new();
			enumMember.Name = field.Name;
			enumMember.Value = field.Value switch
			{
				byte b => unchecked((sbyte)b),
				ushort us => unchecked((short)us),
				uint ui => unchecked((int)ui),
				ulong ul => unchecked((long)ul),
				sbyte sb => sb,
				short s => s,
				int i => i,
				long l => l,
				char c => unchecked((short)c),
				_ => throw new NotSupportedException()
			};
			documentation.Members.Add(enumMember.Name, enumMember);
		}

		return documentation;
	}
}
