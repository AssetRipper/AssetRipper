using AssetRipper.Import.Structure.Assembly;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Extensions;
using System.Globalization;

namespace AssetRipper.Export.UnityProjects.Project;

internal static class AddressablesLayout
{
	private const string SettingsNamespace = "UnityEditor.AddressableAssets.Settings";
	private const string GroupSchemasNamespace = "UnityEditor.AddressableAssets.Settings.GroupSchemas";
	private const string RootDirectory = "Assets/AddressableAssetsData";
	private const string GroupDirectory = RootDirectory + "/AssetGroups";
	private const string GroupSchemaDirectory = GroupDirectory + "/Schemas";
	private const string GroupTemplateDirectory = RootDirectory + "/AssetGroupTemplates";
	private const string AssetExtension = "asset";

	internal readonly record struct Layout(string DirectoryPath, string FileName, UnityGuid Guid, bool IsBootstrapCandidate);

	public static bool TryGetLayout(IMonoBehaviour monoBehaviour, [NotNullWhen(true)] out Layout layout)
	{
		layout = default;
		if (monoBehaviour.IsComponentOnGameObject() || !monoBehaviour.TryGetScript(out IMonoScript? script))
		{
			return false;
		}

		string namespaceName = script.Namespace.String;
		string className = script.GetNonGenericClassName();
		string? directoryPath = null;
		string? fileName = null;
		bool isBootstrapCandidate = false;

		if (namespaceName == SettingsNamespace && className == "AddressableAssetSettings")
		{
			directoryPath = RootDirectory;
			fileName = "AddressableAssetSettings";
			isBootstrapCandidate = true;
		}
		else if (namespaceName == SettingsNamespace && className == "AddressableAssetGroup")
		{
			directoryPath = GroupDirectory;
			fileName = GetStableFileName(monoBehaviour);
		}
		else if ((namespaceName == SettingsNamespace || namespaceName == GroupSchemasNamespace) && className.EndsWith("GroupSchema", StringComparison.Ordinal))
		{
			directoryPath = GroupSchemaDirectory;
			fileName = GetStableFileName(monoBehaviour);
		}
		else if (namespaceName == SettingsNamespace && className is "AddressableAssetGroupTemplate" or "AddressableAssetGroupSchemaTemplate")
		{
			directoryPath = GroupTemplateDirectory;
			fileName = GetStableFileName(monoBehaviour);
		}
		else if (namespaceName == SettingsNamespace && className == "AddressableAssetGroupSchemaSet")
		{
			directoryPath = GroupDirectory;
			fileName = GetStableFileName(monoBehaviour);
		}

		if (directoryPath is null || fileName is null)
		{
			return false;
		}

		string relativePath = $"{directoryPath}/{fileName}.asset";
		string guidSeed = string.Join("|",
			"AssetRipper",
			"Addressables",
			script.GetAssemblyNameFixed(),
			namespaceName,
			className,
			relativePath,
			monoBehaviour.PathID.ToString(CultureInfo.InvariantCulture));
		UnityGuid guid = UnityGuid.Md5Hash(guidSeed);

		monoBehaviour.OverrideDirectory ??= directoryPath;
		monoBehaviour.OverrideName ??= fileName;
		monoBehaviour.OverrideExtension ??= AssetExtension;

		layout = new Layout(directoryPath, fileName, guid, isBootstrapCandidate);
		return true;
	}

	private static string GetStableFileName(IMonoBehaviour monoBehaviour)
	{
		string name = monoBehaviour.GetBestName();
		name = FileSystem.RemoveCloneSuffixes(name);
		name = FileSystem.RemoveInstanceSuffixes(name);
		name = FileSystem.FixInvalidFileNameCharacters(name.Trim());
		return string.IsNullOrWhiteSpace(name) ? monoBehaviour.ClassName : name;
	}
}
