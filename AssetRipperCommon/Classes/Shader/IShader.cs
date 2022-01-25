using AssetRipper.Core.Classes.Shader.SerializedShader;
using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes.Shader
{
	public interface IShader : INamedObject
	{
		bool HasParsedForm { get; }
		ISerializedShader ParsedForm { get; }
	}

	public static class ShaderExtensions
	{
		public static string GetValidShaderName(this IShader shader)
		{
			return shader.HasParsedForm ? shader.ParsedForm.Name : shader.GetNameNotEmpty();
		}
	}
}
