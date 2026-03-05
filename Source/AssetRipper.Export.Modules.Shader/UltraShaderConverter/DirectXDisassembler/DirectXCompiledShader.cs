using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.DirectXDisassembler.Blocks;
using System.Text;

namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.DirectXDisassembler;

public class DirectXCompiledShader
{
	public byte[] hash;
	/// <summary>
	/// always 1
	/// </summary>
	public int version;
	public int size;
	public int blockCount;
	public int[] blockOffsets;
	public byte[][] blockDatas;

	public ShaderBlock[] blocks;

	public ISGN Isgn => (ISGN)blocks.First(b => b is ISGN);
	public OSGN Osgn => (OSGN)blocks.First(b => b is OSGN);
	public SHDR Shdr => (SHDR)blocks.First(b => b is SHDR);

	public DirectXCompiledShader(Stream stream)
	{
		using BinaryReader reader = new BinaryReader(stream);
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
			using MemoryStream ms = new MemoryStream(blockData);
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
