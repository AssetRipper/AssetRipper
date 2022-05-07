using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public interface ISerializedShader : IHasNameString
	{
		string CustomEditorName { get; set; }
		string FallbackName { get; set; }
		public ISerializedProperties PropInfo { get; }
	}
}
