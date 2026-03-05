using AssetRipper.Export.Modules.Shaders.IO;
using AssetRipper.Export.Modules.Shaders.ShaderBlob;
using AssetRipper.IO.Files.Streams;
using AssetRipper.Primitives;
using Smolv;
using SpirV;

namespace AssetRipper.Export.Modules.Shaders.Exporters;

public class ShaderVulkanExporter : ShaderTextExporter
{
	/// <summary>
	/// 2019.3 and greater
	/// </summary>
	public static bool HasProgRayTracing(UnityVersion version) => version.GreaterThanOrEquals(2019, 3);

	public override string Name => "ShaderVulkanExporter";

	public override void Export(ShaderWriter writer, ref ShaderSubProgram subProgram)
	{
		using MemoryStream ms = new MemoryStream(subProgram.ProgramData);
		using BinaryReader reader = new BinaryReader(ms);
		int requirements = reader.ReadInt32();
		int snippetCount = HasProgRayTracing(writer.Version) ? 6 : 5;
		for (int i = 0; i < snippetCount; i++)
		{
			int offset = reader.ReadInt32();
			int size = reader.ReadInt32();
			if (size > 0)
			{
				long position = ms.Position;
				ExportSnippet(writer, ms, offset, size);
				ms.Position = position;
			}
		}
	}

	private void ExportSnippet(TextWriter writer, Stream stream, int offset, int size)
	{
		using PartialStream snippetStream = new PartialStream(stream, offset, size);
		snippetStream.Position = 0;
		int decodedSize = SmolvDecoder.GetDecodedBufferSize(snippetStream);
		if (decodedSize == 0)
		{
			throw new Exception("Invalid SMOL-V shader header");
		}
		using MemoryStream decodedStream = new MemoryStream(new byte[decodedSize]);
		if (SmolvDecoder.Decode(stream, size, decodedStream))
		{
			decodedStream.Position = 0;
			Module module = Module.ReadFrom(decodedStream);
			string listing = m_disassembler.Disassemble(module, DisassemblyOptions.Default);
			ExportListing(writer, "//ShaderVulkanExporter\n" + listing);
		}
		else
		{
			throw new Exception("Unable to decode SMOL-V shader");
		}
	}

	private readonly Disassembler m_disassembler = new Disassembler();
}
