using AssetRipper.Assets;
using AssetRipper.Configuration;
using AssetRipper.Export.Configuration;
using AssetRipper.Export.UnityProjects;
using AssetRipper.Export.UnityProjects.EngineAssets;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Mining.PredefinedAssets;
using AssetRipper.Processing.Extended.Deduplication;
using AssetRipper.Processing.Extended.PathOverrides;
using AssetRipper.Processing.Extended.UnityPackages;
using AssetRipper.Processing.Editor;
using AssetRipper.Processing.Scenes;

namespace AssetRipper.Processing.Extended;

/// <summary>
/// Adds the extended feature set on top of the standard <see cref="ExportHandler"/>.
/// Subclassing also flips <c>GameFileLoader.Premium</c>, which unlocks the gated settings UI.
/// </summary>
public sealed class ExtendedExportHandler : ExportHandler
{
	public const string PathOverridesKey = "PathOverrides";
	public const string PackageDataKey = "PackageData";

	private readonly DeduplicationMap deduplicationMap = new();

	public ExtendedExportHandler(FullConfiguration settings) : base(settings)
	{
		settings.SingletonData.Add(PathOverridesKey, new JsonDataInstance<PathOverrideData>(PathOverrideDataContext.Default.PathOverrideData));
		settings.ListData.Add(PackageDataKey, new List<string>());
	}

	protected override IEnumerable<IAssetProcessor> GetProcessors()
	{
		// A fresh run must not inherit the previous run's decisions.
		deduplicationMap.Clear();

		foreach (IAssetProcessor processor in base.GetProcessors())
		{
			// Before: deduplicate so every later processor sees the canonical assets.
			if (processor is EditorFormatProcessor && Settings.ProcessingSettings.EnableAssetDeduplication)
			{
				yield return new AssetDeduplicationProcessor(deduplicationMap);
			}

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

	/// <remarks>
	/// Runs before <c>DoFinalOverrides</c>, and the exporter stack tries the most recently
	/// registered handler first. The stock engine-asset exporter therefore still wins and
	/// user packages act as the fallback, which is the precedence we want.
	/// </remarks>
	protected override void BeforeExport(ProjectExporter projectExporter)
	{
		foreach (UnityPackageData package in LoadPackages())
		{
			projectExporter.OverrideExporter<IUnityObjectBase>(new EngineAssetsExporter(PackageAssetCacheFactory.Create(package)));
		}

		if (deduplicationMap.Count > 0)
		{
			projectExporter.OverrideExporter<IUnityObjectBase>(new DeduplicationExporter(deduplicationMap));
		}
	}

	protected override IEnumerable<IPostExporter> GetPostExporters()
	{
		List<UnityPackageData> packages = LoadPackages();
		foreach (IPostExporter postExporter in base.GetPostExporters())
		{
			// Replace the stock manifest exporter with one that knows about user packages.
			yield return postExporter is PackageManifestPostExporter
				? new ExtendedPackageManifestPostExporter(packages)
				: postExporter;
		}
	}

	private List<UnityPackageData> LoadPackages()
	{
		DataSet? set = Settings.ListData[PackageDataKey];
		return set is null ? [] : UnityPackageLoader.Load(set.Strings);
	}
}
