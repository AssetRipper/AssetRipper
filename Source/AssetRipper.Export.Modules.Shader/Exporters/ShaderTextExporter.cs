using AssetRipper.Export.Modules.Shaders.IO;
using AssetRipper.Export.Modules.Shaders.ShaderBlob;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.Modules.Shaders.Exporters;

public class ShaderTextExporter
{
	public virtual string Name => "ShaderTextExporter";

	public virtual void Export(ShaderWriter writer, ref ShaderSubProgram subProgram)
	{
		byte[] exportData = subProgram.ProgramData;
		using MemoryStream memStream = new MemoryStream(exportData);
		using BinaryReader reader = new BinaryReader(memStream);
		ExportText(writer, reader, Name);
	}

	protected static void ExportText(TextWriter writer, BinaryReader reader) => ExportText(writer, reader, null);
	protected static void ExportText(TextWriter writer, BinaryReader reader, string? name)
	{
		List<char> characters = new List<char>();
		if (!string.IsNullOrEmpty(name))
		{
			characters.Add('/');
			characters.Add('/');
			foreach (char c in name.ToCharArray())
			{
				characters.Add(c);
			}
			characters.Add('\n');
		}
		while (reader.BaseStream.Position != reader.BaseStream.Length)
		{
			characters.Add(reader.ReadChar());
		}
		ExportText(writer, characters.ToArray());
	}
	protected static void ExportText(TextWriter writer, char[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			char c = array[i];
			if (c == '\n')
			{
				if (i == array.Length - 1)
				{
					break;
				}
				writer.Write(c);
				writer.WriteIndent(ExpectedIndent);
			}
			else
			{
				writer.Write(c);
			}
		}
	}

	protected static void ExportListing(TextWriter writer, string listing)
	{
		writer.Write('\n');
		writer.WriteIndent(ExpectedIndent);

		for (int i = 0; i < listing.Length;)
		{
			char c = listing[i++];
			bool newLine = false;
			if (c == '\r')
			{
				if (i == listing.Length)
				{
					newLine = true;
				}
				else
				{
					char nc = listing[i];
					if (nc != '\n')
					{
						newLine = true;
					}
				}
			}
			else if (c == '\n')
			{
				newLine = true;
			}

			if (newLine)
			{
				if (i == listing.Length)
				{
					break;
				}
				writer.Write(c);
				writer.WriteIndent(ExpectedIndent);
			}
			else
			{
				writer.Write(c);
			}
		}
	}

	protected const int ExpectedIndent = 5;
}
