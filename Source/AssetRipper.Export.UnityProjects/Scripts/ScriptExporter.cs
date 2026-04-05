using AssetRipper.Assets;
using AssetRipper.Export.Configuration;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_115;

namespace AssetRipper.Export.UnityProjects.Scripts;

public class ScriptExporter : IAssetExporter
{
	public ScriptExporter(IAssemblyManager assemblyManager, FullConfiguration configuration)
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

		// Add detected package assemblies to the reference dictionary so they get skipped.
		if (configuration.ExportSettings.PackageDetectionMode == PackageDetectionMode.Auto
			&& configuration.DetectedPackages is { Count: > 0 })
		{
			HashSet<string> packageAssemblyNames = PackageDetector.GetPackageAssemblyNames(
				assemblyManager, configuration.DetectedPackages);

			foreach (string assemblyName in packageAssemblyNames)
			{
				// The GUID value in ReferenceAssemblyDictionary doesn't matter for package scripts
				// (we use per-script .cs.meta GUIDs instead), but we need the entry to exist
				// so GetExportType() returns Skip.
				ReferenceAssemblyDictionary[assemblyName] = ScriptHashing.CalculateAssemblyGuid(assemblyName);
				Logger.Info(LogCategory.Export, $"Skipping package assembly: {assemblyName}");
			}

			// Build per-script GUID map from .cs.meta GUIDs extracted from package tarballs
			if (configuration.DetectedAssemblyGuids is { Count: > 0 })
			{
				foreach (KeyValuePair<string, string> kvp in configuration.DetectedAssemblyGuids)
				{
					ScriptGuidMap[kvp.Key] = UnityGuid.Parse(kvp.Value);
				}
				Logger.Info(LogCategory.Export, $"Loaded {ScriptGuidMap.Count} per-script GUID(s) for package scripts");
			}
		}
	}

	public IAssemblyManager AssemblyManager { get; }
	public ScriptExportMode ExportMode { get; }
	internal ScriptDecompiler Decompiler { get; }
	internal Dictionary<string, UnityGuid> ReferenceAssemblyDictionary { get; }
	/// <summary>
	/// Maps class names to their .cs.meta GUIDs for source-compiled package scripts.
	/// </summary>
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
		else
		{
			exportCollection = null;
			return false;
		}
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
		// For source-compiled packages: use per-script .cs.meta GUID + fileID 11500000
		// This matches how Unity editor references scripts in packages
		string className = script.ClassName_R.String;
		if (ScriptGuidMap.TryGetValue(className, out UnityGuid scriptGuid))
		{
			return new(MonoScriptDecompiledFileID, scriptGuid, AssetType.Meta);
		}

		// Fallback for precompiled DLLs (UnityExtensions, etc.): assembly GUID + MD4 fileID
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
