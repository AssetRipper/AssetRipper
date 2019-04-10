using System.IO;

namespace uTinyRipper.Classes.Shaders.Exporters
{
	public class ShaderTextExporter
	{
		public virtual void Export(byte[] shaderData, TextWriter writer)
		{
			using (MemoryStream memStream = new MemoryStream(shaderData))
			{
				using (BinaryReader reader = new BinaryReader(memStream))
				{
					Export(reader, writer);
				}
			}
		}

		protected virtual void Export(BinaryReader reader, TextWriter writer)
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

		protected const int ExpectedIndent = 5;
	}
}
