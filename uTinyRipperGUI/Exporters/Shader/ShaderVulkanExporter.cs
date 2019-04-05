using Smolv;
using SpirV;
using System;
using System.IO;
using uTinyRipper;
using uTinyRipper.Classes.Shaders.Exporters;

namespace uTinyRipperGUI.Exporters
{
	public class ShaderVulkanExporter : ShaderTextExporter
	{
		public override void Export(byte[] shaderData, TextWriter writer)
		{
			using (MemoryStream ms = new MemoryStream(shaderData))
			{
				using (BinaryReader reader = new BinaryReader(ms))
				{
					const int SnippetCount = 5;
					int unknown = reader.ReadInt32();
					for (int i = 0; i < SnippetCount; i++)
					{
						int offset = reader.ReadInt32();
						int size = reader.ReadInt32();

						if (size > 0)
						{
							ExportSnippet(ms, offset, size, writer);
						}
					}
				}
			}
		}

		private void ExportSnippet(Stream stream, int offset, int size, TextWriter writer)
		{
			using (PartialStream snippetStream = new PartialStream(stream, offset, size))
			{
				int decodedSize = SmolvDecoder.GetDecodedBufferSize(snippetStream);
				if (decodedSize == 0)
				{
					throw new Exception("Invalid SMOL-V shader header");
				}
				using (MemoryStream decodedStream = new MemoryStream(new byte[decodedSize]))
				{
					if (SmolvDecoder.Decode(stream, size, decodedStream))
					{
						decodedStream.Position = 0;
						Module module = Module.ReadFrom(decodedStream);
						string listing = m_disassembler.Disassemble(module, DisassemblyOptions.Default);
						ExportListing(listing, writer);
					}
					else
					{
						throw new Exception("Unable to decode SMOL-V shader");
					}
				}
			}
		}

		private void ExportListing(string listing, TextWriter writer)
		{
			writer.WriteLine();
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

		private readonly Disassembler m_disassembler = new Disassembler();
	}
}
