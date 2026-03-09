using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.SourceGenerated.Classes.ClassID_48;

namespace AssetRipper.Export.UnityProjects.Shaders;

public sealed class RegistryShaderExporter : BinaryAssetExporter
{
	private readonly RegistryPackageBridge registryPackageBridge;

	public RegistryShaderExporter(RegistryPackageBridge registryPackageBridge)
	{
		this.registryPackageBridge = registryPackageBridge;
	}

	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (asset is IShader shader && registryPackageBridge.TryGetShaderPointer(shader, out MetaPtr pointer))
		{
			exportCollection = new SingleRedirectExportCollection(asset, pointer);
			return true;
		}

		exportCollection = null;
		return false;
	}
}
