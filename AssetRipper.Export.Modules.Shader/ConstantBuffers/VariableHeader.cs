namespace ShaderTextRestorer.ConstantBuffers
{
	internal class VariableHeader
	{
		public uint NameOffset { get; set; }
		public uint StartOffset { get; set; }
		public uint TypeOffset { get; set; }
		public Variable Variable { get; set; }
	}
}
