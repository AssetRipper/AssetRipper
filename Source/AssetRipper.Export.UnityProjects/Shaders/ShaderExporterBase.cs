using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_48;

namespace AssetRipper.Export.UnityProjects.Shaders;

public abstract class ShaderExporterBase : BinaryAssetExporter
{
	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (asset is IShader shader)
		{
			exportCollection = new ShaderExportCollection(this, shader);
			return true;
		}
		else
		{
			exportCollection = null;
			return false;
		}
	}
}
