using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public interface ISerializedShader : IHasName
	{
		string CustomEditorName { get; set; }
		string FallbackName { get; set; }
		public ISerializedProperties PropInfo { get; }
	}
}
