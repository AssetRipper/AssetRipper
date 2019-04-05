using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.Classes.Shaders.Exporters;

namespace uTinyRipper.Classes.Shaders
{
	public struct SerializedSubShader : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			m_passes = reader.ReadAssetArray<SerializedPass>();
			Tags.Read(reader);
			LOD = reader.ReadInt32();
		}

		public void Export(ShaderWriter writer)
		{
			writer.WriteIndent(1);
			writer.Write("SubShader {\n");
			if(LOD != 0)
			{
				writer.WriteIndent(2);
				writer.Write("LOD {0}\n", LOD);
			}
			Tags.Export(writer, 2);
			foreach(SerializedPass pass in Passes)
			{
				pass.Export(writer);
			}
			writer.WriteIndent(1);
			writer.Write("}\n");
		}

		public IReadOnlyList<SerializedPass> Passes => m_passes;
		public int LOD { get; private set; }

		public SerializedTagMap Tags;
		
		private SerializedPass[] m_passes;
	}
}
