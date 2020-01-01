using System;

namespace uTinyRipper.Classes.Shaders
{
	public struct ShaderSubProgramBlob : IAssetReadable
	{
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasUnknown(Version version) => version.IsGreaterEqual(2019, 3);

		public void Read(AssetReader reader)
		{
			long startPosition = reader.BaseStream.Position;
			int count = reader.ReadInt32();
			long headerPosition = reader.BaseStream.Position;
			int entrySize = HasUnknown(reader.Version) ? 12 : 8;

			SubPrograms = new ShaderSubProgram[count];
			for (int i = 0; i < count; i++)
			{
				reader.BaseStream.Position = headerPosition + i * entrySize;
				int offset = reader.ReadInt32();
				int length = reader.ReadInt32();
				int unknown = 0;
				if (HasUnknown(reader.Version))
				{
					unknown = reader.ReadInt32();
				}
				
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

				// we don't know shader type so pass vertex
				SubPrograms[subIndex].Export(writer, ShaderType.Vertex);
			}
			writer.WriteString(header, j, header.Length - j);
		}

		public ShaderSubProgram[] SubPrograms { get; set; }
		
		private const string GpuProgramIndexName = "GpuProgramIndex";
	}
}
