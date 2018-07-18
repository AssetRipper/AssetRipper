using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.Classes.Shaders.Exporters;

namespace UtinyRipper.Classes.Shaders
{
	public struct ShaderSubProgramBlob : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			long startPosition = stream.BaseStream.Position;
			int count = stream.ReadInt32();
			long headerPosition = stream.BaseStream.Position;

			m_subPrograms = new ShaderSubProgram[count];
			for (int i = 0; i < count; i++)
			{
				stream.BaseStream.Position = headerPosition + i * 8;
				int offset = stream.ReadInt32();
				int length = stream.ReadInt32();
				
				long dataPosition = startPosition + offset;
				stream.BaseStream.Position = dataPosition;
				ShaderSubProgram subProgram = new ShaderSubProgram();
				subProgram.Read(stream);
				if (stream.BaseStream.Position != dataPosition + length)
				{
					throw new Exception($"Read less {stream.BaseStream.Position - dataPosition} than expected {length}");
				}
				m_subPrograms[i] = subProgram;
			}
		}

		public void Export(TextWriter writer, string header, Func<ShaderGpuProgramType, ShaderTextExporter> exporterInstantiator)
		{
			int j = 0;
			while (true)
			{
				int index = header.IndexOf(GpuProgramIndexName, j);
				if (index == -1)
				{
					break;
				}
				
				int length = index - j;
				writer.WriteString(header, j, length);
				j += length + GpuProgramIndexName.Length + 1;

				int subProgram = -1;
				for(int startIndex = j; j < header.Length; j++)
				{
					if(!char.IsDigit(header[j]))
					{
						string numberStr = header.Substring(startIndex, j - startIndex);
						subProgram = int.Parse(numberStr);
						break;
					}
				}

				SubPrograms[subProgram].Export(writer, exporterInstantiator);
			}
			writer.WriteString(header, j, header.Length - j);
		}

		public IReadOnlyList<ShaderSubProgram> SubPrograms => m_subPrograms;
		
		private const string GpuProgramIndexName = "GpuProgramIndex";
		
		private ShaderSubProgram[] m_subPrograms;
	}
}
