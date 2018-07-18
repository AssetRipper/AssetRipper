using System.IO;

namespace UtinyRipper.Classes.Shaders.Exporters
{
	public class ShaderMetalExporter : ShaderTextExporter
	{
		public override void Export(BinaryReader reader, TextWriter writer)
		{
			using (EndianStream stream = new EndianStream(reader.BaseStream))
			{
				stream.BaseStream.Position += 36;
				EntryName = stream.ReadStringZeroTerm();
			}
			base.Export(reader, writer);
		}

		public string EntryName { get; private set; }
	}
}
