using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files;

namespace AssetRipper.Assets.Export
{
	public interface IProjectAssetContainer : IExportContainer
	{
		AssetCollection File { get; }
		TemporaryAssetCollection TemporaryCollection { get; }
		UnityGUID SceneNameToGUID(string name);
	}
}
