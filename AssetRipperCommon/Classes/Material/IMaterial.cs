using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Shader;

namespace AssetRipper.Core.Classes.Material
{
	public interface IMaterial : INamedObject
	{
		PPtr<IShader> ShaderPtr { get; }
	}
}
