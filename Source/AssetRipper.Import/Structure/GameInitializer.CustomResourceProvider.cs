using AssetRipper.Assets.Bundles;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Platforms;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.ResourceFiles;

namespace AssetRipper.Import.Structure;

internal sealed partial record class GameInitializer
{
	private sealed record class CustomResourceProvider(
		PlatformGameStructure? PlatformStructure,
		PlatformGameStructure? MixedStructure,
		FileSystem FileSystem)
		: IResourceProvider
	{
		public ResourceFile? FindResource(string resName)
		{
			string fixedName = SpecialFileNames.FixResourcePath(resName);
			string? resPath = RequestResource(fixedName);
			if (resPath is null)
			{
				Logger.Log(LogType.Warning, LogCategory.Import, $"Resource file '{resName}' hasn't been found");
				return null;
			}

			ResourceFile resourceFile = new ResourceFile(resPath, fixedName, FileSystem);
			Logger.Info(LogCategory.Import, $"Resource file '{resName}' has been loaded");
			return resourceFile;
		}

		private string? RequestResource(string resource)
		{
			return PlatformStructure?.RequestResource(resource) ?? MixedStructure?.RequestResource(resource);
		}
	}
}
