using AssetRipper.Import.Logging;
using AssetRipper.Mining.PredefinedAssets;

namespace AssetRipper.Processing.Extended.UnityPackages;

/// <summary>
/// Parses user-uploaded package JSON. A bad file must never abort an export,
/// so failures are logged and skipped.
/// </summary>
public static class UnityPackageLoader
{
	public static List<UnityPackageData> Load(IEnumerable<string> jsonTexts)
	{
		List<UnityPackageData> packages = [];
		int index = 0;
		foreach (string json in jsonTexts)
		{
			index++;
			UnityPackageData package;
			try
			{
				package = UnityPackageData.FromJson(json);
			}
			catch (Exception ex)
			{
				Logger.Warning(LogCategory.Export, $"Package data #{index} could not be parsed and was skipped: {ex.Message}");
				continue;
			}

			if (string.IsNullOrEmpty(package.Name))
			{
				Logger.Warning(LogCategory.Export, $"Package data #{index} has no name and was skipped.");
				continue;
			}

			packages.Add(Normalize(package));
		}
		return packages;
	}

	/// <summary>
	/// <see cref="UnityPackageData.Assets"/> is a struct wrapping a dictionary. When the JSON
	/// omits the "Assets" key it is left default, and then even reading <c>Count</c> throws a
	/// <see cref="NullReferenceException"/>. The type exposes no way to ask whether it was
	/// initialized, so probing and catching is the only available guard.
	/// </summary>
	private static UnityPackageData Normalize(UnityPackageData package)
	{
		try
		{
			_ = package.Assets.Count;
		}
		catch (NullReferenceException)
		{
			// Assets is init-only, so rebuild the record rather than assigning.
			return package with { Assets = new AssetDictionary() };
		}
		return package;
	}
}
