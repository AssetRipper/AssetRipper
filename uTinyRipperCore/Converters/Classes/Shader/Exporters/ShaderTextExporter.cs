using System.IO;
using uTinyRipper.Classes.Shaders;

namespace uTinyRipper.Converters.Shaders
{
	public class ShaderTextExporter
	{
		public virtual void Export(ShaderWriter writer, ref ShaderSubProgram subProgram)
		{
			byte[] exportData = subProgram.ProgramData;
			using (MemoryStream memStream = new MemoryStream(exportData))
			{
				using (BinaryReader reader = new BinaryReader(memStream))
				{
					ExportText(writer, reader);
				}
			}
		}

		protected static void ExportText(TextWriter writer, BinaryReader reader)
		{
			while (reader.BaseStream.Position != reader.BaseStream.Length)
			{
				char c = reader.ReadChar();
				if (c == '\n')
				{
					if (reader.BaseStream.Position == reader.BaseStream.Length)
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
}
