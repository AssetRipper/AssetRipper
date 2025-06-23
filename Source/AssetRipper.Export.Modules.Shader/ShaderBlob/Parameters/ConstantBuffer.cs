using System.Diagnostics;

namespace AssetRipper.Export.Modules.Shaders.ShaderBlob.Parameters;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class ConstantBuffer
{
	public ConstantBuffer() { }

	public ConstantBuffer(string name, MatrixParameter[] matrices, VectorParameter[] vectors, StructParameter[] structs, int usedSize)
	{
		Name = name;
		NameIndex = -1;
		MatrixParams = matrices;
		VectorParams = vectors;
		StructParams = structs;
		Size = usedSize;
		IsPartialCB = false;
	}

	public string Name { get; set; } = string.Empty;
	public int NameIndex { get; set; }
	public MatrixParameter[] MatrixParams { get; set; } = Array.Empty<MatrixParameter>();
	public VectorParameter[] VectorParams { get; set; } = Array.Empty<VectorParameter>();
	public StructParameter[] StructParams { get; set; } = Array.Empty<StructParameter>();
	public int Size { get; set; }
	public bool IsPartialCB { get; set; }

	public NumericShaderParameter[] AllNumericParams
	{
		get
		{
			NumericShaderParameter[] shaderParams = new NumericShaderParameter[MatrixParams.Length + VectorParams.Length];
			MatrixParams.CopyTo(shaderParams, 0);
			VectorParams.CopyTo(shaderParams, MatrixParams.Length);
			return shaderParams;
		}
	}

	private string GetDebuggerDisplay()
	{
		return Name;
	}
}
