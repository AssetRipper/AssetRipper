using ShaderTextRestorer.ShaderBlob;

namespace ShaderLabConvert
{
	public interface IUSILOptimizer
	{
		public bool Run(UShaderProgram shader, ShaderSubProgram shaderData);
	}
}
