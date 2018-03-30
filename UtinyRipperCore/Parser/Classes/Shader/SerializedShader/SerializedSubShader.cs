using System.Collections.Generic;
using System.IO;

namespace UtinyRipper.Classes.Shaders
{
	public struct SerializedSubShader : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			m_passes = stream.ReadArray<SerializedPass>();
			Tags.Read(stream);
			LOD = stream.ReadInt32();
		}

		public void Export(TextWriter writer, Shader shader)
		{
			writer.WriteIntent(1);
			writer.Write("Subshader {\n");
			if(LOD != 0)
			{
				writer.WriteIntent(2);
				writer.Write("LOD {0}\n", LOD);
			}
			Tags.Export(writer, 2);
			foreach(SerializedPass pass in Passes)
			{
				pass.Export(writer, shader);
			}
			writer.WriteIntent(1);
			writer.Write("}\n");
		}

		public IReadOnlyList<SerializedPass> Passes => m_passes;
		public int LOD { get; private set; }

		public SerializedTagMap Tags;
		
		private SerializedPass[] m_passes;
	}
}
