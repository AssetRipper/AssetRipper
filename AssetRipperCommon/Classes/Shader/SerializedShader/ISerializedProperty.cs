using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public interface ISerializedProperty : IHasName
	{
		string Description { get; set; }
		string[] Attributes { get; set; }
		SerializedPropertyType Type { get; set; }
		SerializedPropertyFlag Flags { get; set; }
		float DefValue0 { get; set; }
		float DefValue1 { get; set; }
		float DefValue2 { get; set; }
		float DefValue3 { get; set; }
		ISerializedTextureProperty DefTexture { get; }
	}
}
