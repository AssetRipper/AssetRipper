using System;
using System.Collections.Generic;

namespace uTinyRipper.Classes.Shaders
{
	public struct ShaderSubProgramBlob : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			long startPosition = reader.BaseStream.Position;
			int count = reader.ReadInt32();
			long headerPosition = reader.BaseStream.Position;

			SubPrograms = new ShaderSubProgram[count];
			for (int i = 0; i < count; i++)
			{
				reader.BaseStream.Position = headerPosition + i * 8;
				int offset = reader.ReadInt32();
				int length = reader.ReadInt32();
				
				long dataPosition = startPosition + offset;
				reader.BaseStream.Position = dataPosition;
				ShaderSubProgram subProgram = new ShaderSubProgram();
				subProgram.Read(reader);
				if (reader.BaseStream.Position != dataPosition + length)
				{
					throw new Exception($"Read less {reader.BaseStream.Position - dataPosition} than expected {length}");
				}
				SubPrograms[i] = subProgram;
			}
		}

		public void Export(ShaderWriter writer, string header)
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

				int subIndex = -1;
				for(int startIndex = j; j < header.Length; j++)
				{
					if(!char.IsDigit(header[j]))
					{
						string numberStr = header.Substring(startIndex, j - startIndex);
						subIndex = int.Parse(numberStr);
						break;
					}
				}

				ref ShaderSubProgram subProgram = ref SubPrograms[subIndex];
				// we don't know shader type so pass vertex
				subProgram.Export(writer, ShaderType.Vertex);
			}
			writer.WriteString(header, j, header.Length - j);
		}

		public ShaderSubProgram[] SubPrograms { get; set; }
		
		private const string GpuProgramIndexName = "GpuProgramIndex";
	}
}
