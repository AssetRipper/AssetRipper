namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.DirectXDisassembler.Blocks;

public sealed class ISGN : ShaderBlock
{
	public int inputCount;
	public int unknown;
	public Input[] inputs;

	public override string FourCC => "ISGN";

	public ISGN(Stream stream)
	{
		using BinaryReader reader = new BinaryReader(stream);
		inputCount = reader.ReadInt32();
		unknown = reader.ReadInt32();
		inputs = new Input[inputCount];
		for (int i = 0; i < inputCount; i++)
		{
			Input input = new Input(reader);
			inputs[i] = input;
		}
		reader.Align(4);
	}
	public class Input
	{
		public int nameIndex;
		public string name;
		public int index;
		public int sysValue;
		public int format;
		public int register;
		public byte mask;
		public byte usedMask;
		public Input(BinaryReader reader)
		{
			nameIndex = reader.ReadInt32();
			index = reader.ReadInt32();
			sysValue = reader.ReadInt32();
			format = reader.ReadInt32();
			register = reader.ReadInt32();
			mask = reader.ReadByte();
			usedMask = reader.ReadByte();
			reader.BaseStream.Position += 2;
			long markedPos = reader.BaseStream.Position;
			reader.BaseStream.Position = nameIndex;
			name = reader.ReadNullString();
			reader.BaseStream.Position = markedPos;
		}
	}
}
