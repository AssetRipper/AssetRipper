using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Import.Structure;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Import.Structure.Platforms;

namespace AssetRipper.Processing;

public record GameData(
	GameBundle GameBundle,
	UnityVersion ProjectVersion,
	IAssemblyManager AssemblyManager,
	PlatformGameStructure? PlatformStructure)
{
	public ProcessedAssetCollection AddNewProcessedCollection(string name)
	{
		return GameBundle.AddNewProcessedCollection(name, ProjectVersion);
	}

	public static GameData FromGameStructure(GameStructure gameStructure)
	{
		return new(gameStructure.FileCollection, gameStructure.FileCollection.GetMaxUnityVersion(), gameStructure.AssemblyManager, gameStructure.PlatformStructure);
	}
}
