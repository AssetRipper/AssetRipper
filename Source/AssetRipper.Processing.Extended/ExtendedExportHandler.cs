using AssetRipper.Configuration;
using AssetRipper.Export.Configuration;
using AssetRipper.Export.UnityProjects;
using AssetRipper.Processing.Extended.PathOverrides;
using AssetRipper.Processing.Scenes;

namespace AssetRipper.Processing.Extended;

/// <summary>
/// Adds the extended feature set on top of the standard <see cref="ExportHandler"/>.
/// Subclassing also flips <c>GameFileLoader.Premium</c>, which unlocks the gated settings UI.
/// </summary>
public sealed class ExtendedExportHandler : ExportHandler
{
	public const string PathOverridesKey = "PathOverrides";

	public ExtendedExportHandler(FullConfiguration settings) : base(settings)
	{
		settings.SingletonData.Add(PathOverridesKey, new JsonDataInstance<PathOverrideData>(PathOverrideDataContext.Default.PathOverrideData));
	}

	protected override IEnumerable<IAssetProcessor> GetProcessors()
	{
		foreach (IAssetProcessor processor in base.GetProcessors())
		{
			yield return processor;

			// After: overrides must win over the paths OriginalPathProcessor just assigned.
			if (processor is OriginalPathProcessor
				&& Settings.SingletonData.TryGetStoredValue(PathOverridesKey, out PathOverrideData? pathOverrides)
				&& pathOverrides.Files.Count > 0)
			{
				yield return new PathOverrideProcessor(pathOverrides);
			}
		}
	}
}
