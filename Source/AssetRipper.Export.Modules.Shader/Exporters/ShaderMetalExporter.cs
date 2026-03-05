using AssetRipper.Export.Modules.Shaders.IO;
using AssetRipper.Export.Modules.Shaders.ShaderBlob;
using AssetRipper.IO.Endian;
using AssetRipper.Primitives;

namespace AssetRipper.Export.Modules.Shaders.Exporters;

public class ShaderMetalExporter : ShaderTextExporter
{
	/// <summary>
	/// 5.3.0 and greater
	/// </summary>
	public static bool HasBlob(UnityVersion version) => version.GreaterThanOrEquals(5, 3);

	public override string Name => "ShaderMetalExporter";

	public override void Export(ShaderWriter writer, ref ShaderSubProgram subProgram)
	{
		using MemoryStream memStream = new MemoryStream(subProgram.ProgramData);
		using BinaryReader reader = new BinaryReader(memStream);
		if (HasBlob(writer.Version))
		{
			long position = reader.BaseStream.Position;
			uint fourCC = reader.ReadUInt32();
			if (fourCC == MetalFourCC)
			{
				int offset = reader.ReadInt32();
				reader.BaseStream.Position = position + offset;
			}
			using EndianReader endReader = new EndianReader(reader.BaseStream, EndianType.LittleEndian);
			EntryName = endReader.ReadStringZeroTerm();
		}

		ExportText(writer, reader);
	}

	public string? EntryName { get; private set; }

	private const uint MetalFourCC = 0xf00dcafe;
}
