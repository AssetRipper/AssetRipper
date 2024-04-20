using AssetRipper.Assets.Bundles;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Platforms;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.Utils;

namespace AssetRipper.Import.Structure;

internal sealed partial record class GameInitializer
{
	private sealed record class CustomResourceProvider(
		PlatformGameStructure? PlatformStructure,
		PlatformGameStructure? MixedStructure)
		: IResourceProvider
	{
		public ResourceFile? FindResource(string resName)
		{
			string fixedName = FilenameUtils.FixResourcePath(resName);
			string? resPath = RequestResource(fixedName);
			if (resPath is null)
			{
				Logger.Log(LogType.Warning, LogCategory.Import, $"Resource file '{resName}' hasn't been found");
				return null;
			}

			ResourceFile resourceFile = new ResourceFile(resPath, fixedName);
			Logger.Info(LogCategory.Import, $"Resource file '{resName}' has been loaded");
			return resourceFile;
		}

		private string? RequestResource(string resource)
		{
			return PlatformStructure?.RequestResource(resource) ?? MixedStructure?.RequestResource(resource);
		}
	}
}
