using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.IO;
using System.IO;

namespace AssetRipper.Core.Converters.Shader
{
	public class ShaderMetalExporter : ShaderTextExporter
	{
		public override void Export(ShaderWriter writer, ref ShaderSubProgram subProgram)
		{
			using (MemoryStream memStream = new MemoryStream(subProgram.ProgramData))
			{
				using (BinaryReader reader = new BinaryReader(memStream))
				{
					if (AssetRipper.Core.Classes.Shader.Shader.HasBlob(writer.Version))
					{
						long position = reader.BaseStream.Position;
						uint fourCC = reader.ReadUInt32();
						if (fourCC == MetalFourCC)
						{
							int offset = reader.ReadInt32();
							reader.BaseStream.Position = position + offset;
						}
						using (EndianReader endReader = new EndianReader(reader.BaseStream, EndianType.LittleEndian))
						{
							EntryName = endReader.ReadStringZeroTerm();
						}
					}

					ExportText(writer, reader);
				}
			}
		}

		public string EntryName { get; private set; }

		private const uint MetalFourCC = 0xf00dcafe;
	}
}
