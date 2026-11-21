using AssetRipper.Assets;
using AssetRipper.Export.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_115;

namespace AssetRipper.Export.UnityProjects.Scripts;

public class ScriptExporter : IAssetExporter
{
	public ScriptExporter(IAssemblyManager assemblyManager, FullConfiguration configuration, PackageDetectionResult? packageDetection = null)
	{
		AssemblyManager = assemblyManager;
		Decompiler = new ScriptDecompiler(AssemblyManager)
		{
			LanguageVersion = configuration.ExportSettings.ScriptLanguageVersion.ToCSharpLanguageVersion(configuration.Version),
			ScriptContentLevel = configuration.ImportSettings.ScriptContentLevel,
			FullyQualifiedTypeNames = configuration.ExportSettings.ScriptTypesFullyQualified,
		};
		ExportMode = configuration.ExportSettings.ScriptExportMode;
		ReferenceAssemblyDictionary = ReferenceAssemblies.GetReferenceAssemblies(AssemblyManager, configuration.Version);

		if (packageDetection is { PackageAssemblies.Count: > 0 })
		{
			foreach (string assemblyName in packageDetection.PackageAssemblies)
			{
				// The GUID value here is never read for package scripts, CreateSkipExportPointer uses
				// ScriptGuidMap instead, but the entry must exist so GetExportType() returns Skip.
				ReferenceAssemblyDictionary[assemblyName] = ScriptHashing.CalculateAssemblyGuid(assemblyName);
				Logger.Info(LogCategory.Export, $"Skipping package assembly: {assemblyName}");
			}

			if (packageDetection.ScriptGuids.Count > 0)
			{
				foreach (KeyValuePair<string, UnityGuid> kvp in packageDetection.ScriptGuids)
				{
					ScriptGuidMap[kvp.Key] = kvp.Value;
				}
				Logger.Info(LogCategory.Export, $"Loaded {ScriptGuidMap.Count} per-script GUID(s) for package scripts");
			}
		}
	}

	public IAssemblyManager AssemblyManager { get; }
	public ScriptExportMode ExportMode { get; }
	internal ScriptDecompiler Decompiler { get; }
	internal Dictionary<string, UnityGuid> ReferenceAssemblyDictionary { get; }
	internal Dictionary<string, UnityGuid> ScriptGuidMap { get; } = new(StringComparer.Ordinal);
	private bool HasDecompiled { get; set; } = false;
	private static long MonoScriptDecompiledFileID { get; } = ExportIdHandler.GetMainExportID((int)ClassIDType.MonoScript);

	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (asset is IMonoScript script)
		{
			if (HasDecompiled)
			{
				exportCollection = new SingleRedirectExportCollection(asset, CreateExportPointer(script));
			}
			else
			{
				HasDecompiled = true;
				if (AssemblyManager.IsSet)
				{
					exportCollection = new ScriptExportCollection(this, script);
				}
				else
				{
					exportCollection = new EmptyScriptExportCollection(this, script);
				}
			}
			return true;
		}
		
		exportCollection = null;
		return false;
	}

	public AssemblyExportType GetExportType(IMonoScript script)
	{
		return GetExportType(script.GetAssemblyNameFixed());
	}

	public MetaPtr CreateExportPointer(IMonoScript script)
	{
		return GetExportType(script) switch
		{
			AssemblyExportType.Decompile => new(MonoScriptDecompiledFileID, ScriptHashing.CalculateScriptGuid(script), AssetType.Meta),
			AssemblyExportType.Skip => CreateSkipExportPointer(script),
			_ => new(ScriptHashing.CalculateScriptFileID(script), ScriptHashing.CalculateAssemblyGuid(script), AssetType.Meta),
		};
	}

	private MetaPtr CreateSkipExportPointer(IMonoScript script)
	{
		string className = script.ClassName_R.String;
		if (ScriptGuidMap.TryGetValue(className, out UnityGuid scriptGuid))
		{
			return new(MonoScriptDecompiledFileID, scriptGuid, AssetType.Meta);
		}
		
		return new(ScriptHashing.CalculateScriptFileID(script), ReferenceAssemblyDictionary[script.GetAssemblyNameFixed()], AssetType.Meta);
	}

	public AssemblyExportType GetExportType(string assemblyName)
	{
		if (ReferenceAssemblyDictionary.ContainsKey(assemblyName))
		{
			return AssemblyExportType.Skip;
		}
		else if (!AssemblyManager.IsSet)
		{
			return AssemblyExportType.Decompile;
		}
		else if (ExportMode is ScriptExportMode.Decompiled)
		{
			return AssemblyExportType.Decompile;
		}
		else if (ExportMode is ScriptExportMode.Hybrid)
		{
			return ReferenceAssemblies.IsPredefinedAssembly(assemblyName)
				? AssemblyExportType.Decompile
				: AssemblyExportType.Save;
		}
		else
		{
			return AssemblyExportType.Save;
		}
	}

	AssetType IAssetExporter.ToExportType(IUnityObjectBase asset) => AssetType.Meta;

	bool IAssetExporter.ToUnknownExportType(Type type, out AssetType assetType)
	{
		assetType = AssetType.Meta;
		return true;
	}
}
