using System.IO;

namespace uTinyRipper.Classes.Shaders.Exporters
{
	public class ShaderMetalExporter : ShaderTextExporter
	{
		public ShaderMetalExporter(Version version)
		{
			m_version = version;
		}

		protected override void Export(BinaryReader reader, TextWriter writer)
		{
			if (Shader.IsEncoded(m_version))
			{
				long position = reader.BaseStream.Position;
				uint fourCC = reader.ReadUInt32();
				if (fourCC == MetalFourCC)
				{
					int offset = reader.ReadInt32();
					reader.BaseStream.Position = position + offset;
				}
				using (EndianReader endReader = new EndianReader(reader.BaseStream))
				{
					EntryName = endReader.ReadStringZeroTerm();
				}
			}
			base.Export(reader, writer);
		}

		public string EntryName { get; private set; }

		private const uint MetalFourCC = 0xf00dcafe;

		private Version m_version;
	}
}
