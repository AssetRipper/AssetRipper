using System.IO;

namespace UtinyRipper.Classes.Shaders.Exporters
{
	public class ShaderMetalExporter : ShaderTextExporter
	{
		protected override void Export(BinaryReader reader, TextWriter writer)
		{
			long position = reader.BaseStream.Position;
			uint fourCC = reader.ReadUInt32();
			if (fourCC == MetalFourCC)
			{
				int offset = reader.ReadInt32();
				reader.BaseStream.Position = position + offset;
			}
			using (EndianStream stream = new EndianStream(reader.BaseStream))
			{
				EntryName = stream.ReadStringZeroTerm();
			}
			base.Export(reader, writer);
		}

		public string EntryName { get; private set; }

		private const uint MetalFourCC = 0xf00dcafe;
	}
}
