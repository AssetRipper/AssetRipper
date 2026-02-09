using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using System.Text;

namespace AssetRipper.Tools.TypeTreeExtractor;

internal static class Program
{
	private static readonly string outputDirectory = System.IO.Path.Join(AppContext.BaseDirectory, "Output");

	static void Main(string[] args)
	{
		System.IO.Directory.CreateDirectory(outputDirectory);
		if (args.Length == 0)
		{
			Console.WriteLine("No arguments");
		}
		else
		{
			LoadFiles(args);
		}
		Console.ReadKey();
		return;
	}

	private static void LoadFiles(string[] files)
	{
		foreach (string file in files)
		{
			LoadFile(file);
		}
	}

	private static void LoadFile(string fullName)
	{
		Console.WriteLine(fullName);
		try
		{
			FileBase file = SchemeReader.LoadFile(fullName, LocalFileSystem.Instance);
			file.ReadContentsRecursively();
			SaveTypeTrees(file);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}

	private static void SaveTypeTrees(FileBase file)
	{
		if (file is SerializedFile serializedFile)
		{
			SaveTypeTrees(serializedFile);
		}
		else if (file is FileContainer container)
		{
			SaveTypeTrees(container);
		}
	}

	private static void SaveTypeTrees(FileContainer container)
	{
		foreach (SerializedFile serializedFile in container.SerializedFiles)
		{
			SaveTypeTrees(serializedFile);
		}
		foreach (FileContainer internalContainer in container.FileLists)
		{
			SaveTypeTrees(internalContainer);
		}
	}

	private static void SaveTypeTrees(SerializedFile file)
	{
		if (!file.HasTypeTree)
		{
			return;
		}

		StringBuilder sb = new();
		foreach (SerializedType type in file.Types.ToArray().OrderBy(t => t.TypeID))
		{
			Console.WriteLine($"\tType ID: {type.TypeID,-10} Script Index: {type.ScriptTypeIndex,-5} Node Count: {type.OldType.Nodes.Count}");

			AppendTypeTree(sb, type);
		}
		foreach (SerializedTypeReference type in file.RefTypes.ToArray().OrderBy(t => t.TypeID))
		{
			Console.WriteLine($"\tType ID: {type.TypeID,-10} Script Index: {type.ScriptTypeIndex,-5} Node Count: {type.OldType.Nodes.Count}");
			Console.WriteLine($"\t\t{type.AsmName} {type.FullName}");

			AppendTypeTree(sb, type);
		}
		string text = sb.ToString();
		if (!string.IsNullOrWhiteSpace(text))
		{
			string filePath = System.IO.Path.Join(outputDirectory, file.Name + ".txt");
			System.IO.File.WriteAllText(filePath, text);
		}
	}

	private static void AppendTypeTree(StringBuilder sb, SerializedTypeBase type)
	{
		string typeTreeText = type.OldType.Dump;
		if (!string.IsNullOrEmpty(typeTreeText))
		{
			if (type is SerializedTypeReference reference)
			{
				sb.AppendLine($"// {reference.AsmName} {reference.FullName}");
			}
			AppendClassIDLine(sb, type);
			sb.AppendLine(typeTreeText);
		}
	}

	private static void AppendClassIDLine(StringBuilder sb, SerializedTypeBase type)
	{
		string typeName = type.OldType.Nodes.Count > 0 ? type.OldType.Nodes[0].Type : "Unknown";
		string strippedSuffix = type.IsStrippedType ? " Stripped" : "";
		sb.AppendLine($"// classID{{{type.TypeID}}}: {typeName}{strippedSuffix}");
	}
}
