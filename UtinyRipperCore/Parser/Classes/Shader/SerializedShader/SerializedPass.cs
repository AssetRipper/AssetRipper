using System;
using System.Collections.Generic;
using System.IO;

namespace UtinyRipper.Classes.Shaders
{
	public struct SerializedPass : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			m_nameIndices = new Dictionary<string, int>();
			
			m_nameIndices.Read(stream);
			Type = (SerializedPassType)stream.ReadInt32();
			State.Read(stream);
			ProgramMask = stream.ReadUInt32();
			ProgVertex.Read(stream);
			ProgFragment.Read(stream);
			ProgGeometry.Read(stream);
			ProgHull.Read(stream);
			ProgDomain.Read(stream);
			HasInstancingVariant = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);

			UseName = stream.ReadStringAligned();
			Name = stream.ReadStringAligned();
			TextureName = stream.ReadStringAligned();
			Tags.Read(stream);
		}

		public void Export(TextWriter writer, Shader shader)
		{
			writer.WriteIntent(2);
			writer.Write("{0} ", Type.ToString());

			if (Type == SerializedPassType.UsePass)
			{
				writer.Write("\"{0}\"\n", UseName);
			}
			else
			{
				writer.Write("{\n");
				
				if (Type == SerializedPassType.GrabPass)
				{
					if(TextureName != string.Empty)
					{
						writer.WriteIntent(3);
						writer.Write("\"{0}\"\n", TextureName);
					}
				}
				else if (Type == SerializedPassType.Pass)
				{
					State.Export(writer);

					ProgVertex.Export(writer, shader, ShaderType.Vertex);
					ProgFragment.Export(writer, shader, ShaderType.Fragment);
					ProgGeometry.Export(writer, shader, ShaderType.Geometry);
					ProgHull.Export(writer, shader, ShaderType.Hull);
					ProgDomain.Export(writer, shader, ShaderType.Domain);

#warning ProgramMask?
#warning HasInstancingVariant?
				}
				else
				{
					throw new NotSupportedException($"Unsupported pass type {Type}");
				}

				writer.WriteIntent(2);
				writer.Write("}\n");
			}
		}
		
		public IReadOnlyDictionary<string, int> NameIndices => m_nameIndices;
		public SerializedPassType Type { get; private set; }
		public uint ProgramMask { get; private set; }
		public bool HasInstancingVariant { get; private set; }
		public string UseName { get; private set; }
		public string Name { get; private set; }
		public string TextureName { get; private set; }

		public SerializedShaderState State;
		public SerializedProgram ProgVertex;
		public SerializedProgram ProgFragment;
		public SerializedProgram ProgGeometry;
		public SerializedProgram ProgHull;
		public SerializedProgram ProgDomain;
		public SerializedTagMap Tags;

		private Dictionary<string, int> m_nameIndices;
	}
}
