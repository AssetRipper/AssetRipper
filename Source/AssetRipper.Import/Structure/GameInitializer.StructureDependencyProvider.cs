using AssetRipper.Assets.Bundles;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Platforms;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.Import.Structure;

internal sealed partial record class GameInitializer
{
	private sealed record class StructureDependencyProvider(
		PlatformGameStructure? PlatformStructure,
		PlatformGameStructure? MixedStructure,
		FileSystem FileSystem)
		: IDependencyProvider
	{
		public FileBase? FindDependency(FileIdentifier identifier)
		{
			string? systemFilePath = RequestDependency(identifier.PathName);
			return systemFilePath is null ? null : SchemeReader.LoadFile(systemFilePath, FileSystem);
		}

		/// <summary>
		/// Attempts to find the path for the dependency with that name.
		/// </summary>
		private string? RequestDependency(string dependency)
		{
			return PlatformStructure?.RequestDependency(dependency) ?? MixedStructure?.RequestDependency(dependency);
		}

		public void ReportMissingDependency(FileIdentifier identifier)
		{
			Logger.Log(LogType.Warning, LogCategory.Import, $"Dependency '{identifier}' wasn't found");
		}
	}
}
