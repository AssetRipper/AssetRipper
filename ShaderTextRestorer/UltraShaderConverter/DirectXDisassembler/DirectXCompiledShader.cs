using DirectXDisassembler.Blocks;
using System.IO;
using System.Linq;
using System.Text;

namespace DirectXDisassembler
{
	public class DirectXCompiledShader
	{
		public byte[] hash;
		public int version; //always 1
		public int size;
		public int blockCount;
		public int[] blockOffsets;
		public byte[][] blockDatas;

		public ShaderBlock[] blocks;

		//10/24/2021 this is really bad but I'm lazy rn
		public ISGN Isgn { get { return blocks.First(b => b != null && b.GetType() == typeof(ISGN)) as ISGN; } }
		public OSGN Osgn { get { return blocks.First(b => b != null && b.GetType() == typeof(OSGN)) as OSGN; } }
		public SHDR Shdr { get { return blocks.First(b => b != null && b.GetType() == typeof(SHDR)) as SHDR; } }

		public DirectXCompiledShader(Stream stream)
		{
			using (BinaryReader reader = new BinaryReader(stream))
			{
				reader.BaseStream.Position += 4;
				hash = reader.ReadBytes(16);
				version = reader.ReadInt32();
				size = reader.ReadInt32();
				blockCount = reader.ReadInt32();
				blockOffsets = new int[blockCount];
				for (int i = 0; i < blockCount; i++)
				{
					blockOffsets[i] = reader.ReadInt32();
				}
				blockDatas = new byte[blockCount][];
				blocks = new ShaderBlock[blockCount];
				for (int i = 0; i < blockCount; i++)
				{
					reader.BaseStream.Position = blockOffsets[i];
					string fourCc = Encoding.ASCII.GetString(reader.ReadBytes(4));
					int size = reader.ReadInt32();
					byte[] blockData = reader.ReadBytes(size);
					blockDatas[i] = blockData;
					using (MemoryStream ms = new MemoryStream(blockData))
					{
						switch (fourCc)
						{
							case "ISGN":
								blocks[i] = new ISGN(ms);
								break;
							case "OSGN":
								blocks[i] = new OSGN(ms);
								break;
							case "SHDR":
							case "SHEX":
								blocks[i] = new SHDR(ms, blocks);
								break;
						}
					}
				}
			}
		}
	}
}
